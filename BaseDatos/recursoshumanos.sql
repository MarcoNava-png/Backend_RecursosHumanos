SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

CREATE SCHEMA nomina;

CREATE SCHEMA recursos_humanos;

CREATE FUNCTION nomina.fn_agregar_concepto(p_id_nomina integer, p_clave text, p_cantidad numeric, p_importe numeric, p_extra numeric DEFAULT 0, p_id_hist integer DEFAULT NULL::integer, p_id_infonavit integer DEFAULT NULL::integer, p_id_fonacot integer DEFAULT NULL::integer, p_id_pension integer DEFAULT NULL::integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    c        RECORD;
    n        RECORD;
    v_uma    NUMERIC;
    v_limite NUMERIC;
    v_exento NUMERIC := 0;
BEGIN
    p_importe := round(COALESCE(p_importe, 0), 2);
    SELECT * INTO c FROM nomina.vw_bi_conceptos_nomina WHERE clave_concepto = p_clave;
    IF NOT FOUND THEN RAISE EXCEPTION 'Concepto % no existe', p_clave; END IF;
    IF p_importe <= 0 AND (c.tipo_concepto <> 'informativo' OR COALESCE(p_cantidad, 0) = 0) THEN RETURN; END IF;

    IF c.tipo_concepto = 'percepcion' AND c.porcentaje_exento > 0 THEN
        SELECT nm.id_empleado, pn.fecha_pago INTO n
          FROM nomina.tb_bi_nominas nm JOIN nomina.vw_bi_periodos_nomina pn USING (id_periodo_nomina)
         WHERE nm.id_nomina = p_id_nomina;
        v_uma := nomina.fn_indicador('uma', n.fecha_pago);
        v_limite := p_importe * c.porcentaje_exento / 100;
        IF c.tope_veces_uma IS NOT NULL THEN
            v_limite := LEAST(v_limite, c.tope_veces_uma * v_uma
                              * CASE WHEN c.multiplicador = 'ninguno' THEN 1 ELSE GREATEST(COALESCE(p_extra, 0), 1) END);
        END IF;
        IF c.acumula_anual THEN
            v_limite := LEAST(v_limite, GREATEST(c.tope_veces_uma * v_uma
                              * CASE WHEN c.multiplicador = 'ninguno' THEN 1 ELSE GREATEST(COALESCE(p_extra, 0), 1) END
                              - nomina.fn_exento_acumulado(n.id_empleado, c.tipo_exencion,
                                              EXTRACT(YEAR FROM n.fecha_pago)::int, p_id_nomina)
                              - COALESCE((SELECT SUM(d.importe_exento) FROM nomina.tb_bi_nomina_detalle d
                                            JOIN nomina.vw_bi_conceptos_nomina k ON k.id_concepto = d.id_concepto
                                           WHERE d.id_nomina = p_id_nomina AND k.tipo_exencion = c.tipo_exencion), 0), 0));
        END IF;
        v_exento := round(LEAST(p_importe, v_limite), 2);
    ELSIF c.tipo_concepto <> 'percepcion' THEN
        v_exento := 0;
    END IF;

    INSERT INTO nomina.tb_bi_nomina_detalle
           (id_nomina, id_concepto, cantidad, importe_gravado, importe_exento,
            id_hist_concepto, id_credito_infonavit, id_credito_fonacot, id_pension)
    VALUES (p_id_nomina, c.id_concepto, COALESCE(p_cantidad, 0), p_importe - v_exento, v_exento,
            p_id_hist, p_id_infonavit, p_id_fonacot, p_id_pension);
END;
$$;

CREATE FUNCTION nomina.fn_aplicar_deducciones(p_id_nomina integer, p_dias_calendario numeric, p_dias_retiro numeric, p_incluir_infonavit boolean, p_liquidar_prestamos boolean) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    n      RECORD;
    r      RECORD;
    v_umi  NUMERIC;
    v_imp  NUMERIC;
    v_per  INT;
    v_base NUMERIC;
BEGIN
    SELECT nm.*, pn.fecha_inicio, pn.fecha_fin, pn.fecha_pago, pn.id_empresa, pn.periodicidad, pn.id_periodicidad_pago
      INTO n
      FROM nomina.tb_bi_nominas nm JOIN nomina.vw_bi_periodos_nomina pn USING (id_periodo_nomina)
     WHERE nm.id_nomina = p_id_nomina;
    v_umi := nomina.fn_indicador('umi', n.fecha_pago);

    IF p_incluir_infonavit THEN
        FOR r IN SELECT c.*, td.clave AS tipo_descuento FROM nomina.tb_bi_creditos_infonavit c
                   JOIN nomina.tb_bi_tipos_descuento td ON td.id_tipo_descuento = c.id_tipo_descuento
                  WHERE c.id_empleado = n.id_empleado AND c.activo AND c.fecha_inicio <= n.fecha_fin
                    AND (c.fecha_suspension IS NULL OR c.fecha_suspension > n.fecha_inicio)
        LOOP
            v_imp := CASE r.tipo_descuento
                WHEN 'porcentaje' THEN n.salario_base_cotizacion * r.valor_descuento / 100 * p_dias_retiro
                WHEN 'cuota_fija' THEN r.valor_descuento / 30.4 * p_dias_calendario
                ELSE r.valor_descuento * v_umi * 2 / 61 * p_dias_calendario END;
            IF r.incluye_seguro_vivienda THEN v_imp := v_imp + 15.0 / 61 * p_dias_calendario; END IF;
            PERFORM nomina.fn_agregar_concepto(p_id_nomina, 'D006', p_dias_calendario, v_imp, 0, NULL, r.id_credito_infonavit);
        END LOOP;
    END IF;

    FOR r IN SELECT * FROM nomina.tb_bi_creditos_fonacot c
              WHERE c.id_empleado = n.id_empleado AND c.activo AND c.saldo > 0 AND c.fecha_inicio <= n.fecha_fin
    LOOP
        SELECT GREATEST(COUNT(*), 1) INTO v_per FROM nomina.vw_bi_periodos_nomina p
         WHERE p.id_empresa = n.id_empresa AND p.tipo_nomina = 'ordinaria' AND p.id_periodicidad_pago = n.id_periodicidad_pago
           AND date_trunc('month', p.fecha_pago) = date_trunc('month', n.fecha_pago) AND p.activo;
        v_imp := CASE WHEN p_liquidar_prestamos THEN r.saldo ELSE LEAST(r.saldo, r.retencion_mensual / v_per) END;
        PERFORM nomina.fn_agregar_concepto(p_id_nomina, 'D007', 1, v_imp, 0, NULL, NULL, r.id_credito_fonacot);
    END LOOP;

    FOR r IN SELECT h.*, c.clave_concepto FROM nomina.tb_bi_conceptos_hist h
               JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = h.id_concepto
              WHERE h.id_empleado = n.id_empleado AND h.activo AND h.saldo_restante > 0
                AND h.parcialidades_pagadas < h.total_parcialidades AND h.fecha_inicio_descuento <= n.fecha_fin
    LOOP
        v_imp := CASE WHEN p_liquidar_prestamos OR r.parcialidades_pagadas = r.total_parcialidades - 1
                      THEN r.saldo_restante
                      ELSE LEAST(r.saldo_restante, round(r.monto_total_deuda / r.total_parcialidades, 2)) END;
        PERFORM nomina.fn_agregar_concepto(p_id_nomina, r.clave_concepto, 1, v_imp, 0, r.id_hist_concepto);
    END LOOP;

    PERFORM nomina.fn_totalizar_nomina(p_id_nomina);
    SELECT total_percepciones - isr_retenido - imss_obrero INTO v_base FROM nomina.tb_bi_nominas WHERE id_nomina = p_id_nomina;
    FOR r IN SELECT pa.*, td.clave AS tipo_descuento FROM nomina.tb_bi_pensiones_alimenticias pa
               JOIN nomina.tb_bi_tipos_descuento td ON td.id_tipo_descuento = pa.id_tipo_descuento
              WHERE pa.id_empleado = n.id_empleado AND pa.activo AND pa.fecha_inicio <= n.fecha_fin
                AND (pa.fecha_fin IS NULL OR pa.fecha_fin >= n.fecha_inicio)
    LOOP
        v_imp := CASE r.tipo_descuento WHEN 'porcentaje' THEN v_base * r.valor / 100 ELSE r.valor END;
        PERFORM nomina.fn_agregar_concepto(p_id_nomina, 'D008', 1, v_imp, 0, NULL, NULL, NULL, r.id_pension);
    END LOOP;

    PERFORM nomina.fn_totalizar_nomina(p_id_nomina);
    UPDATE nomina.tb_bi_nominas SET observaciones = 'Neto negativo: revisar deducciones'
     WHERE id_nomina = p_id_nomina AND neto_a_pagar < 0;
END;
$$;

CREATE FUNCTION nomina.fn_autorizar_periodo(p_id_periodo integer, p_id_empleado_autoriza integer, p_id_usuario integer DEFAULT 1) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE nomina.tb_bi_periodos_nomina
       SET estatus_cierre = 'autorizado', id_autorizador_nomina = p_id_empleado_autoriza, fecha_autorizacion = now(),
           id_usuario_modificacion = p_id_usuario, fecha_modificacion = now()
     WHERE id_periodo_nomina = p_id_periodo AND estatus_cierre = 'calculado';
    IF NOT FOUND THEN RAISE EXCEPTION 'Solo se autoriza un periodo calculado'; END IF;
    UPDATE nomina.tb_bi_nominas SET estatus_recibo = 'autorizado', id_usuario_modificacion = p_id_usuario,
                                    fecha_modificacion = now()
     WHERE id_periodo_nomina = p_id_periodo AND estatus_recibo = 'calculado';
END;
$$;

CREATE FUNCTION nomina.fn_calcular_aguinaldo(p_id_periodo integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    p         RECORD;
    e         RECORD;
    v_id      INT;
    v_total   INT := 0;
    v_desde   DATE;
    v_hasta   DATE;
    v_dias    INT;
    v_sd      NUMERIC;
    v_imp     NUMERIC;
    v_grav    NUMERIC;
BEGIN
    SELECT * INTO p FROM nomina.vw_bi_periodos_nomina WHERE id_periodo_nomina = p_id_periodo;
    IF p.tipo_nomina <> 'aguinaldo' THEN RAISE EXCEPTION 'El periodo no es de aguinaldo'; END IF;
    IF p.estatus_cierre NOT IN ('abierto','calculado') THEN RAISE EXCEPTION 'El periodo está %', p.estatus_cierre; END IF;
    DELETE FROM nomina.tb_bi_nominas WHERE id_periodo_nomina = p_id_periodo;

    v_desde := make_date(p.ejercicio, 1, 1);
    v_hasta := make_date(p.ejercicio, 12, 31);

    FOR e IN
        SELECT emp.id_empleado, emp.fecha_ingreso, ep.dias_aguinaldo, ep.descontar_faltas_aguinaldo,
               s.salario_base, s.tarifa_por_hora
          FROM recursos_humanos.tb_bi_empleados emp
          JOIN nomina.tb_bi_empleados_nomina en ON en.id_empleado = emp.id_empleado AND en.activo
          JOIN nomina.tb_bi_esquemas_prestaciones ep ON ep.id_esquema_prestaciones = en.id_esquema_prestaciones
          JOIN LATERAL (SELECT * FROM recursos_humanos.tb_bi_historial_sueldos s
                         WHERE s.id_empleado = emp.id_empleado AND s.activo AND s.fecha_inicio_vigencia <= p.fecha_pago
                         ORDER BY s.fecha_inicio_vigencia DESC LIMIT 1) s ON TRUE
         WHERE emp.id_empresa = p.id_empresa AND emp.activo
           AND emp.id_estatus_laboral = (SELECT id_estatus_laboral FROM recursos_humanos.tb_bi_estatus_laborales WHERE clave = 'activo')
           AND emp.fecha_ingreso <= v_hasta
    LOOP
        v_dias := LEAST(v_hasta - GREATEST(v_desde, e.fecha_ingreso) + 1, 365);
        IF e.descontar_faltas_aguinaldo THEN
            v_dias := v_dias - (SELECT COUNT(DISTINCT fecha_dia) FROM recursos_humanos.vw_bi_asistencia_diaria
                                 WHERE id_empleado = e.id_empleado AND activo
                                   AND estatus_asistencia IN ('falta_injustificada','permiso_sin_goce')
                                   AND fecha_dia BETWEEN v_desde AND v_hasta);
        END IF;

        v_sd := e.salario_base;
        IF v_sd IS NULL THEN
            SELECT COALESCE(SUM(d.importe_total) / NULLIF(SUM(n.dias_pagados), 0), 0) INTO v_sd
              FROM nomina.tb_bi_nomina_detalle d
              JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = d.id_concepto AND c.clave_concepto IN ('P002','P003')
              JOIN nomina.tb_bi_nominas n ON n.id_nomina = d.id_nomina AND n.id_empleado = e.id_empleado
              JOIN nomina.vw_bi_periodos_nomina pp ON pp.id_periodo_nomina = n.id_periodo_nomina
             WHERE pp.ejercicio = p.ejercicio AND pp.tipo_nomina = 'ordinaria' AND n.estatus_recibo <> 'cancelado';
        END IF;

        v_imp := round(v_sd * e.dias_aguinaldo * GREATEST(v_dias, 0) / 365.0, 2);
        CONTINUE WHEN v_imp <= 0;

        INSERT INTO nomina.tb_bi_nominas (id_periodo_nomina, id_empleado, salario_diario, dias_pagados)
        VALUES (p_id_periodo, e.id_empleado, v_sd, GREATEST(v_dias, 0)) RETURNING id_nomina INTO v_id;
        PERFORM nomina.fn_agregar_concepto(v_id, 'P005', e.dias_aguinaldo * v_dias / 365.0, v_imp);

        SELECT importe_gravado INTO v_grav FROM nomina.tb_bi_nomina_detalle d
          JOIN nomina.vw_bi_conceptos_nomina c USING (id_concepto) WHERE d.id_nomina = v_id AND c.clave_concepto = 'P005';
        UPDATE nomina.tb_bi_nominas SET isr_causado = nomina.fn_isr_art174(v_grav, v_sd * 30.4, p.fecha_pago)
         WHERE id_nomina = v_id;
        PERFORM nomina.fn_agregar_concepto(v_id, 'D002', 1, nomina.fn_isr_art174(v_grav, v_sd * 30.4, p.fecha_pago));
        PERFORM nomina.fn_aplicar_deducciones(v_id, 0, 0, FALSE, FALSE);
        v_total := v_total + 1;
    END LOOP;

    UPDATE nomina.tb_bi_periodos_nomina SET estatus_cierre = 'calculado' WHERE id_periodo_nomina = p_id_periodo;
    RETURN v_total;
END;
$$;

CREATE FUNCTION nomina.fn_calcular_finiquito(p_id_baja integer, p_fecha_pago date DEFAULT NULL::date) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    b           RECORD;
    e           RECORD;
    v_periodo   INT;
    v_id        INT;
    v_sd        NUMERIC;
    v_sdi       NUMERIC;
    v_sm        NUMERIC;
    v_anios     NUMERIC;
    v_anios_ex  INT;
    v_ult_pag   DATE;
    v_dias      INT;
    v_vac       NUMERIC;
    v_ultimo_aniv DATE;
    v_imp       NUMERIC;
    v_grav_ord  NUMERIC;
    v_grav_sep  NUMERIC;
    v_isr       NUMERIC;
    v_mensual   NUMERIC;
    v_prima     BOOLEAN;
BEGIN
    SELECT bl.*, rpa.clave AS aplica_prima_antiguedad, rpa.anios_minimos,
           mb.aplica_indemnizacion_90_dias, mb.aplica_20_dias_por_anio
      INTO b
      FROM recursos_humanos.tb_bi_bajas_laborales bl
      JOIN recursos_humanos.tb_bi_motivos_baja mb ON mb.id_motivo_baja = bl.id_motivo_baja
      JOIN recursos_humanos.tb_bi_reglas_prima_antiguedad rpa ON rpa.id_regla_prima_antiguedad = mb.id_regla_prima_antiguedad
     WHERE bl.id_baja = p_id_baja;
    IF b IS NULL THEN RAISE EXCEPTION 'Baja % no existe', p_id_baja; END IF;
    p_fecha_pago := COALESCE(p_fecha_pago, b.fecha_baja);

    SELECT emp.id_empleado, emp.id_empresa, emp.fecha_ingreso, ep.dias_aguinaldo, ep.porcentaje_prima_vacacional,
           rp.area_salario_minimo, pe.nombre || ' ' || pe.apellido_paterno AS nombre
      INTO e
      FROM recursos_humanos.tb_bi_empleados emp
      JOIN recursos_humanos.tb_bi_personas pe ON pe.id_persona = emp.id_persona
      JOIN nomina.tb_bi_empleados_nomina en ON en.id_empleado = emp.id_empleado
      JOIN nomina.tb_bi_esquemas_prestaciones ep ON ep.id_esquema_prestaciones = en.id_esquema_prestaciones
      JOIN nomina.vw_bi_registros_patronales rp ON rp.id_registro_patronal = en.id_registro_patronal
     WHERE emp.id_empleado = b.id_empleado;

    IF b.id_nomina_finiquito IS NOT NULL THEN
        IF EXISTS (SELECT 1 FROM nomina.tb_bi_nominas WHERE id_nomina = b.id_nomina_finiquito AND estatus_recibo = 'timbrado') THEN
            RAISE EXCEPTION 'El finiquito ya está timbrado';
        END IF;
        SELECT id_periodo_nomina INTO v_periodo FROM nomina.tb_bi_nominas WHERE id_nomina = b.id_nomina_finiquito;
        UPDATE recursos_humanos.tb_bi_bajas_laborales SET id_nomina_finiquito = NULL WHERE id_baja = p_id_baja;
        DELETE FROM nomina.tb_bi_nominas WHERE id_nomina = b.id_nomina_finiquito;
    ELSE
        INSERT INTO nomina.tb_bi_periodos_nomina (id_empresa, id_tipo_nomina, id_periodicidad_pago, ejercicio, numero_periodo,
                                                  descripcion, fecha_inicio, fecha_fin, fecha_pago, dias_pago)
        SELECT e.id_empresa,
               (SELECT id_tipo_nomina FROM nomina.tb_bi_tipos_nomina WHERE clave = 'finiquito'),
               (SELECT id_periodicidad_pago FROM nomina.tb_bi_periodicidades_pago WHERE clave = 'unica'),
               EXTRACT(YEAR FROM b.fecha_baja)::int,
               COALESCE(MAX(numero_periodo), 0) + 1, left('Finiquito ' || e.nombre, 100),
               b.fecha_baja, b.fecha_baja, p_fecha_pago, 0
          FROM nomina.vw_bi_periodos_nomina
         WHERE id_empresa = e.id_empresa AND tipo_nomina = 'finiquito' AND ejercicio = EXTRACT(YEAR FROM b.fecha_baja)
        RETURNING id_periodo_nomina INTO v_periodo;
    END IF;

    SELECT s.salario_base INTO v_sd FROM recursos_humanos.tb_bi_historial_sueldos s
     WHERE s.id_empleado = e.id_empleado AND s.activo AND s.fecha_inicio_vigencia <= b.fecha_baja
     ORDER BY s.fecha_inicio_vigencia DESC LIMIT 1;
    SELECT m.salario_diario_integrado, m.salario_diario INTO v_sdi, v_imp
      FROM nomina.vw_bi_movimientos_imss m
     WHERE m.id_empleado = e.id_empleado AND m.activo AND m.tipo_movimiento <> 'baja' AND m.fecha_movimiento <= b.fecha_baja
     ORDER BY m.fecha_movimiento DESC LIMIT 1;
    v_sd := COALESCE(v_sd, v_imp, 0);
    IF v_sdi IS NULL OR v_sdi < v_sd THEN
        SELECT s.sdi INTO v_sdi FROM nomina.fn_sdi_sugerido(e.id_empleado, v_sd, b.fecha_baja) s;
    END IF;
    v_sm := nomina.fn_indicador(CASE WHEN e.area_salario_minimo = 'frontera' THEN 'salario_minimo_frontera'
                                     ELSE 'salario_minimo_general' END, b.fecha_baja);
    v_anios := round((b.fecha_baja - e.fecha_ingreso + 1) / 365.0, 4);
    v_anios_ex := FLOOR(v_anios)::int + CASE WHEN v_anios - FLOOR(v_anios) > 0.5 THEN 1 ELSE 0 END;

    INSERT INTO nomina.tb_bi_nominas (id_periodo_nomina, id_empleado, salario_diario, salario_diario_integrado)
    VALUES (v_periodo, e.id_empleado, v_sd, v_sdi) RETURNING id_nomina INTO v_id;

    SELECT MAX(pp.fecha_fin) INTO v_ult_pag
      FROM nomina.tb_bi_nominas n JOIN nomina.vw_bi_periodos_nomina pp ON pp.id_periodo_nomina = n.id_periodo_nomina
     WHERE n.id_empleado = e.id_empleado AND pp.tipo_nomina = 'ordinaria' AND n.estatus_recibo <> 'cancelado';
    v_dias := b.fecha_baja - GREATEST(COALESCE(v_ult_pag, e.fecha_ingreso - 1), e.fecha_ingreso - 1);
    IF v_dias > 0 THEN
        PERFORM nomina.fn_agregar_concepto(v_id, 'P001', v_dias, v_sd * v_dias);
    END IF;
    UPDATE nomina.tb_bi_nominas SET dias_pagados = GREATEST(v_dias, 0) WHERE id_nomina = v_id;

    v_dias := b.fecha_baja - GREATEST(make_date(EXTRACT(YEAR FROM b.fecha_baja)::int, 1, 1), e.fecha_ingreso) + 1;
    PERFORM nomina.fn_agregar_concepto(v_id, 'P005', e.dias_aguinaldo * v_dias / 365.0,
                                       v_sd * e.dias_aguinaldo * v_dias / 365.0);

    SELECT COALESCE(SUM(dias_otorgados + dias_excepcion - dias_disfrutados), 0) INTO v_vac
      FROM recursos_humanos.tb_bi_saldos_vacaciones
     WHERE id_empleado = e.id_empleado AND activo AND fecha_caducidad >= b.fecha_baja;
    v_ultimo_aniv := (e.fecha_ingreso + make_interval(years => FLOOR(v_anios)::int))::date;
    v_vac := v_vac + recursos_humanos.fn_dias_vacaciones_ley(FLOOR(v_anios)::int + 1)
                     * (b.fecha_baja - v_ultimo_aniv + 1) / 365.0;
    v_vac := round(v_vac, 2);
    PERFORM nomina.fn_agregar_concepto(v_id, 'P010', v_vac, v_sd * v_vac);
    PERFORM nomina.fn_agregar_concepto(v_id, 'P004', v_vac, v_sd * v_vac * e.porcentaje_prima_vacacional / 100);

    v_prima := b.aplica_prima_antiguedad <> 'no' AND v_anios >= b.anios_minimos;
    IF v_prima THEN
        PERFORM nomina.fn_agregar_concepto(v_id, 'P011', round(12 * v_anios, 2),
                                           12 * v_anios * LEAST(v_sd, 2 * v_sm), v_anios_ex);
    END IF;
    IF b.aplica_indemnizacion_90_dias THEN
        PERFORM nomina.fn_agregar_concepto(v_id, 'P012', 90, 90 * v_sdi, v_anios_ex);
    END IF;
    IF b.aplica_20_dias_por_anio THEN
        PERFORM nomina.fn_agregar_concepto(v_id, 'P012', round(20 * v_anios, 2), 20 * v_anios * v_sdi, v_anios_ex);
    END IF;

    SELECT COALESCE(SUM(d.importe_gravado) FILTER (WHERE c.tipo_exencion <> 'separacion'), 0),
           COALESCE(SUM(d.importe_gravado) FILTER (WHERE c.tipo_exencion = 'separacion'), 0)
      INTO v_grav_ord, v_grav_sep
      FROM nomina.tb_bi_nomina_detalle d JOIN nomina.vw_bi_conceptos_nomina c USING (id_concepto)
     WHERE d.id_nomina = v_id AND c.tipo_concepto = 'percepcion';
    v_mensual := round(v_sd * 30, 2);
    v_isr := nomina.fn_isr_tarifa(v_grav_ord, 'mensual', p_fecha_pago);
    IF v_grav_sep > 0 THEN
        IF v_grav_sep >= v_mensual AND v_mensual > 0 THEN
            v_isr := v_isr + round(v_grav_sep * nomina.fn_isr_tarifa(v_mensual, 'mensual', p_fecha_pago) / v_mensual, 2);
        ELSE
            v_isr := v_isr + nomina.fn_isr_tarifa(v_mensual + v_grav_sep, 'mensual', p_fecha_pago)
                           - nomina.fn_isr_tarifa(v_mensual, 'mensual', p_fecha_pago);
        END IF;
    END IF;
    UPDATE nomina.tb_bi_nominas SET isr_causado = v_isr WHERE id_nomina = v_id;
    PERFORM nomina.fn_agregar_concepto(v_id, 'D002', 1, v_isr);

    PERFORM nomina.fn_aplicar_deducciones(v_id, 0, 0, FALSE, TRUE);

    UPDATE nomina.tb_bi_periodos_nomina SET estatus_cierre = 'calculado' WHERE id_periodo_nomina = v_periodo;
    UPDATE recursos_humanos.tb_bi_bajas_laborales
       SET id_nomina_finiquito = v_id,
           monto_finiquito_calculado = (SELECT neto_a_pagar FROM nomina.tb_bi_nominas WHERE id_nomina = v_id)
     WHERE id_baja = p_id_baja;
    RETURN v_id;
END;
$$;

CREATE FUNCTION nomina.fn_calcular_nomina(p_id_periodo integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    p            RECORD;
    e            RECORD;
    r            RECORD;
    v_id_nomina  INT;
    v_total      INT := 0;
    v_cal        INT;
    v_alta_ini   DATE;
    v_alta_fin   DATE;
    v_dias_alta  INT;
    v_dias_base  NUMERIC;
    v_faltas     INT;
    v_sin_goce   INT;
    v_incap      INT;
    v_vac        INT;
    v_domingos   INT;
    v_horas      NUMERIC;
    v_horas_sup  NUMERIC;
    v_ret_falta  INT;
    v_ret_mes    INT;
    v_ya         INT;
    v_dias_pag   NUMERIC;
    v_sd         NUMERIC;
    v_sdi        NUMERIC;
    v_sbc        NUMERIC;
    v_tarifa     NUMERIC;
    v_gravado    NUMERIC;
    v_isr        NUMERIC;
    v_subsidio   NUMERIC;
    v_ausencias  INT;
    v_prima_rt   NUMERIC;
    v_imss_obr   NUMERIC;
    v_imss_pat   NUMERIC;
BEGIN
    SELECT * INTO p FROM nomina.vw_bi_periodos_nomina WHERE id_periodo_nomina = p_id_periodo;
    IF p IS NULL THEN RAISE EXCEPTION 'Periodo % no existe', p_id_periodo; END IF;
    IF p.tipo_nomina <> 'ordinaria' THEN RAISE EXCEPTION 'Use la función del tipo % ', p.tipo_nomina; END IF;
    IF p.estatus_cierre NOT IN ('abierto','calculado') THEN
        RAISE EXCEPTION 'El periodo está %; ya no se puede recalcular', p.estatus_cierre;
    END IF;

    PERFORM recursos_humanos.fn_generar_asistencia_rango(p.fecha_inicio, LEAST(p.fecha_fin, CURRENT_DATE - 1));

    DELETE FROM nomina.tb_bi_nominas WHERE id_periodo_nomina = p_id_periodo;
    v_cal := p.fecha_fin - p.fecha_inicio + 1;

    FOR e IN
        SELECT emp.id_empleado, emp.fecha_ingreso, en.id_registro_patronal, en.id_esquema_prestaciones,
               rp.area_salario_minimo, ep.porcentaje_prima_vacacional, ep.porcentaje_prima_dominical,
               (SELECT MAX(b.fecha_baja) FROM recursos_humanos.tb_bi_bajas_laborales b
                 WHERE b.id_empleado = emp.id_empleado AND b.activo) AS fecha_baja
          FROM recursos_humanos.tb_bi_empleados emp
          JOIN nomina.tb_bi_empleados_nomina en ON en.id_empleado = emp.id_empleado AND en.activo
          JOIN nomina.vw_bi_registros_patronales rp ON rp.id_registro_patronal = en.id_registro_patronal
          JOIN nomina.tb_bi_esquemas_prestaciones ep ON ep.id_esquema_prestaciones = en.id_esquema_prestaciones
         WHERE emp.id_empresa = p.id_empresa AND emp.activo
           AND en.id_periodicidad_pago = p.id_periodicidad_pago
           AND emp.fecha_ingreso <= p.fecha_fin
           AND emp.id_estatus_laboral IN (SELECT id_estatus_laboral FROM recursos_humanos.tb_bi_estatus_laborales
                                           WHERE clave IN ('activo','baja'))
    LOOP
        CONTINUE WHEN e.fecha_baja IS NOT NULL AND e.fecha_baja < p.fecha_inicio;

        SELECT * INTO r FROM recursos_humanos.tb_bi_historial_sueldos s
         WHERE s.id_empleado = e.id_empleado AND s.activo AND s.fecha_inicio_vigencia <= p.fecha_fin
           AND (s.fecha_fin_vigencia IS NULL OR s.fecha_fin_vigencia >= p.fecha_inicio)
         ORDER BY s.fecha_inicio_vigencia DESC LIMIT 1;
        CONTINUE WHEN NOT FOUND;
        v_tarifa := r.tarifa_por_hora;

        SELECT m.salario_diario, m.salario_diario_integrado, m.salario_base_cotizacion
          INTO v_sd, v_sdi, v_sbc
          FROM nomina.vw_bi_movimientos_imss m
         WHERE m.id_empleado = e.id_empleado AND m.activo AND m.tipo_movimiento <> 'baja'
           AND m.fecha_movimiento <= p.fecha_fin
         ORDER BY m.fecha_movimiento DESC, m.id_movimiento_imss DESC LIMIT 1;
        IF r.salario_base IS NOT NULL THEN v_sd := r.salario_base; END IF;
        v_sd := COALESCE(v_sd, 0);
        IF v_sdi IS NULL OR v_sdi < v_sd THEN
            SELECT s.sdi, s.sbc INTO v_sdi, v_sbc FROM nomina.fn_sdi_sugerido(e.id_empleado, v_sd, p.fecha_fin) s;
        END IF;

        v_alta_ini := GREATEST(p.fecha_inicio, e.fecha_ingreso);
        v_alta_fin := LEAST(p.fecha_fin, COALESCE(e.fecha_baja, p.fecha_fin));
        v_dias_alta := v_alta_fin - v_alta_ini + 1;
        v_dias_base := CASE WHEN v_dias_alta = v_cal THEN p.dias_pago
                            ELSE round(p.dias_pago * v_dias_alta / v_cal, 2) END;

        SELECT COUNT(DISTINCT fecha_dia) FILTER (WHERE estatus_asistencia = 'falta_injustificada'),
               COUNT(DISTINCT fecha_dia) FILTER (WHERE estatus_asistencia = 'permiso_sin_goce'),
               COUNT(DISTINCT fecha_dia) FILTER (WHERE estatus_asistencia = 'vacaciones'),
               COUNT(DISTINCT fecha_dia) FILTER (WHERE estatus_asistencia IN ('a_tiempo','retardo')
                                                   AND EXTRACT(ISODOW FROM fecha_dia) = 7),
               COALESCE(SUM(horas_cubiertas), 0)
          INTO v_faltas, v_sin_goce, v_vac, v_domingos, v_horas
          FROM recursos_humanos.vw_bi_asistencia_diaria
         WHERE id_empleado = e.id_empleado AND activo AND fecha_dia BETWEEN v_alta_ini AND v_alta_fin;

        SELECT COALESCE(SUM(LEAST(i.fecha_fin, v_alta_fin) - GREATEST(i.fecha_inicio, v_alta_ini) + 1), 0)
          INTO v_incap
          FROM recursos_humanos.tb_bi_incapacidades_imss i
         WHERE i.id_empleado = e.id_empleado AND i.activo
           AND i.fecha_inicio <= v_alta_fin AND i.fecha_fin >= v_alta_ini;

        v_ret_falta := 0;
        SELECT rg.*, pa.clave AS periodo_acumulacion_retardos INTO r FROM recursos_humanos.tb_bi_historial_horarios h
          JOIN recursos_humanos.tb_bi_reglas_asistencia rg ON rg.id_regla_asistencia = h.id_regla_asistencia
          JOIN recursos_humanos.tb_bi_periodos_acumulacion pa ON pa.id_periodo_acumulacion = rg.id_periodo_acumulacion
         WHERE h.id_empleado = e.id_empleado AND h.activo AND h.fecha_inicio_vigencia <= p.fecha_fin
         ORDER BY h.fecha_inicio_vigencia DESC LIMIT 1;
        IF FOUND AND r.aplica_sancion_falta THEN
            IF r.periodo_acumulacion_retardos = 'mensual' THEN
                SELECT COUNT(*) INTO v_ret_mes FROM recursos_humanos.vw_bi_asistencia_diaria
                 WHERE id_empleado = e.id_empleado AND activo AND estatus_asistencia = 'retardo'
                   AND fecha_dia BETWEEN date_trunc('month', p.fecha_fin)::date AND p.fecha_fin;
                SELECT COALESCE(SUM(d.cantidad), 0)::int INTO v_ya
                  FROM nomina.tb_bi_nomina_detalle d
                  JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = d.id_concepto AND c.clave_concepto = 'I003'
                  JOIN nomina.tb_bi_nominas n ON n.id_nomina = d.id_nomina AND n.id_empleado = e.id_empleado
                  JOIN nomina.vw_bi_periodos_nomina pp ON pp.id_periodo_nomina = n.id_periodo_nomina
                 WHERE pp.id_periodo_nomina <> p_id_periodo AND pp.tipo_nomina = 'ordinaria'
                   AND pp.fecha_fin < p.fecha_inicio AND pp.fecha_inicio >= date_trunc('month', p.fecha_fin)::date;
                v_ret_falta := GREATEST(v_ret_mes / r.maximo_retardos_permitidos - v_ya, 0);
            ELSE
                SELECT COUNT(*) / r.maximo_retardos_permitidos INTO v_ret_falta
                  FROM recursos_humanos.vw_bi_asistencia_diaria
                 WHERE id_empleado = e.id_empleado AND activo AND estatus_asistencia = 'retardo'
                   AND fecha_dia BETWEEN v_alta_ini AND v_alta_fin;
            END IF;
        END IF;

        INSERT INTO nomina.tb_bi_nominas (id_periodo_nomina, id_empleado, salario_diario, salario_diario_integrado,
                                          salario_base_cotizacion, dias_falta, dias_incapacidad)
        VALUES (p_id_periodo, e.id_empleado, v_sd, v_sdi, v_sbc, v_faltas + v_sin_goce + v_ret_falta, v_incap)
        RETURNING id_nomina INTO v_id_nomina;

        IF v_tarifa IS NULL OR (SELECT salario_base FROM recursos_humanos.tb_bi_historial_sueldos
                                 WHERE id_empleado = e.id_empleado AND activo AND fecha_inicio_vigencia <= p.fecha_fin
                                   AND (fecha_fin_vigencia IS NULL OR fecha_fin_vigencia >= p.fecha_inicio)
                                 ORDER BY fecha_inicio_vigencia DESC LIMIT 1) IS NOT NULL THEN
            v_dias_pag := GREATEST(v_dias_base - v_faltas - v_sin_goce - v_ret_falta - LEAST(v_incap, v_dias_base), 0);
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'P001', v_dias_pag, v_sd * v_dias_pag);
        ELSE
            v_dias_pag := GREATEST(v_dias_base - LEAST(v_incap, v_dias_base), 0);
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'P002', v_horas, v_horas * v_tarifa);
            IF v_ret_falta > 0 AND v_horas > 0 THEN
                PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'D003', v_ret_falta,
                    v_ret_falta * v_tarifa * v_horas / GREATEST((SELECT COUNT(DISTINCT fecha_dia)
                        FROM recursos_humanos.vw_bi_asistencia_diaria
                       WHERE id_empleado = e.id_empleado AND activo AND horas_cubiertas > 0
                         AND fecha_dia BETWEEN v_alta_ini AND v_alta_fin), 1));
            END IF;
        END IF;
        UPDATE nomina.tb_bi_nominas SET dias_pagados = v_dias_pag WHERE id_nomina = v_id_nomina;

        SELECT COALESCE(SUM(EXTRACT(EPOCH FROM (h.hora_salida - h.hora_entrada)) / 3600.0), 0) INTO v_horas_sup
          FROM recursos_humanos.tb_bi_suplencias s
          JOIN recursos_humanos.tb_bi_historial_horarios h ON h.id_historial_horario = s.id_historial_horario
         WHERE s.id_empleado_suplente = e.id_empleado AND s.activo AND s.estatus_aprobacion = 'aprobada'
           AND s.fecha_suplencia BETWEEN v_alta_ini AND v_alta_fin;
        IF v_horas_sup > 0 THEN
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'P003', v_horas_sup, v_horas_sup * COALESCE(v_tarifa, v_sd / 8));
        END IF;

        IF v_vac > 0 THEN
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'P004', v_vac, v_vac * v_sd * e.porcentaje_prima_vacacional / 100);
        END IF;
        IF v_domingos > 0 THEN
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'P006', v_domingos,
                                               v_domingos * v_sd * e.porcentaje_prima_dominical / 100, v_domingos);
        END IF;

        FOR r IN SELECT c.clave_concepto, cr.importe FROM nomina.tb_bi_conceptos_recurrentes cr
                   JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = cr.id_concepto
                  WHERE cr.id_empleado = e.id_empleado AND cr.activo AND c.tipo_concepto = 'percepcion'
                    AND cr.fecha_inicio <= p.fecha_fin AND (cr.fecha_fin IS NULL OR cr.fecha_fin >= p.fecha_inicio)
        LOOP
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, r.clave_concepto, 1, r.importe);
        END LOOP;
        FOR r IN SELECT c.clave_concepto, i.cantidad, i.importe FROM nomina.tb_bi_incidencias_nomina i
                   JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = i.id_concepto
                  WHERE i.id_periodo_nomina = p_id_periodo AND i.id_empleado = e.id_empleado AND i.activo
                    AND c.tipo_concepto = 'percepcion'
        LOOP
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, r.clave_concepto, r.cantidad, r.importe,
                                               CASE WHEN r.clave_concepto = 'P007' THEN CEIL(v_cal / 7.0) ELSE 0 END);
        END LOOP;

        SELECT COALESCE(SUM(d.importe_gravado), 0) INTO v_gravado
          FROM nomina.tb_bi_nomina_detalle d JOIN nomina.vw_bi_conceptos_nomina c USING (id_concepto)
         WHERE d.id_nomina = v_id_nomina AND c.tipo_concepto = 'percepcion';
        v_isr := nomina.fn_isr_periodo(v_gravado, p.periodicidad, p.dias_pago, p.fecha_pago);
        v_subsidio := nomina.fn_subsidio_empleo(v_gravado, p.dias_pago, p.fecha_pago);
        UPDATE nomina.tb_bi_nominas SET isr_causado = v_isr, subsidio_causado = v_subsidio WHERE id_nomina = v_id_nomina;
        PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'D002', 1, GREATEST(v_isr - v_subsidio, 0));
        IF LEAST(v_subsidio, v_isr) > 0 THEN
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'I004', 1, LEAST(v_subsidio, v_isr));
        END IF;

        SELECT pr.prima INTO v_prima_rt FROM nomina.tb_bi_primas_riesgo_trabajo pr
         WHERE pr.id_registro_patronal = e.id_registro_patronal AND pr.activo AND pr.fecha_inicio <= p.fecha_fin
         ORDER BY pr.fecha_inicio DESC LIMIT 1;
        v_ausencias := v_faltas + v_sin_goce + v_ret_falta;
        INSERT INTO nomina.tb_bi_nomina_cuotas_imss (id_nomina, clave_ramo, dias_cotizados, base, cuota_patron, cuota_obrero)
        SELECT v_id_nomina, q.clave_ramo, q.dias, q.base, q.cuota_patron, q.cuota_obrero
          FROM nomina.fn_cuotas_imss(v_sbc, v_sd, v_dias_alta - v_incap, v_dias_alta - v_incap - v_ausencias,
                                     v_dias_alta - v_ausencias, COALESCE(v_prima_rt, 0.54355), p.fecha_fin,
                                     e.area_salario_minimo) q;
        SELECT COALESCE(SUM(cuota_obrero), 0), COALESCE(SUM(cuota_patron), 0) INTO v_imss_obr, v_imss_pat
          FROM nomina.tb_bi_nomina_cuotas_imss WHERE id_nomina = v_id_nomina;
        UPDATE nomina.tb_bi_nominas SET imss_patronal = v_imss_pat WHERE id_nomina = v_id_nomina;
        PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'D001', 1, v_imss_obr);

        IF v_incap > 0 THEN PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'I001', v_incap, 0); END IF;
        IF v_faltas + v_sin_goce > 0 THEN PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'I002', v_faltas + v_sin_goce, 0); END IF;
        IF v_ret_falta > 0 THEN PERFORM nomina.fn_agregar_concepto(v_id_nomina, 'I003', v_ret_falta, 0); END IF;

        FOR r IN SELECT c.clave_concepto, cr.importe FROM nomina.tb_bi_conceptos_recurrentes cr
                   JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = cr.id_concepto
                  WHERE cr.id_empleado = e.id_empleado AND cr.activo AND c.tipo_concepto = 'deduccion'
                    AND cr.fecha_inicio <= p.fecha_fin AND (cr.fecha_fin IS NULL OR cr.fecha_fin >= p.fecha_inicio)
                 UNION ALL
                 SELECT c.clave_concepto, i.importe FROM nomina.tb_bi_incidencias_nomina i
                   JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = i.id_concepto
                  WHERE i.id_periodo_nomina = p_id_periodo AND i.id_empleado = e.id_empleado AND i.activo
                    AND c.tipo_concepto = 'deduccion'
        LOOP
            PERFORM nomina.fn_agregar_concepto(v_id_nomina, r.clave_concepto, 1, r.importe);
        END LOOP;

        PERFORM nomina.fn_aplicar_deducciones(v_id_nomina, v_dias_alta, v_dias_alta - v_ausencias, TRUE, FALSE);
        UPDATE nomina.tb_bi_nominas SET observaciones = COALESCE(observaciones || '. ', '') || 'Sin percepciones en el periodo'
         WHERE id_nomina = v_id_nomina AND total_percepciones = 0;
        v_total := v_total + 1;
    END LOOP;

    UPDATE nomina.tb_bi_periodos_nomina SET estatus_cierre = 'calculado', fecha_modificacion = now()
     WHERE id_periodo_nomina = p_id_periodo;
    RETURN v_total;
END;
$$;

CREATE FUNCTION nomina.fn_calcular_ptu(p_id_ptu integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    t          RECORD;
    p          RECORD;
    r          RECORD;
    v_desde    DATE;
    v_hasta    DATE;
    v_sum_dias NUMERIC;
    v_sum_sal  NUMERIC;
    v_id       INT;
    v_grav     NUMERIC;
    v_total    INT := 0;
BEGIN
    SELECT * INTO t FROM nomina.tb_bi_ptu WHERE id_ptu = p_id_ptu;
    SELECT * INTO p FROM nomina.vw_bi_periodos_nomina WHERE id_periodo_nomina = t.id_periodo_nomina;
    IF p IS NULL OR p.tipo_nomina <> 'ptu' THEN RAISE EXCEPTION 'Liga un periodo de tipo ptu al reparto'; END IF;
    IF p.estatus_cierre NOT IN ('abierto','calculado') THEN RAISE EXCEPTION 'El periodo está %', p.estatus_cierre; END IF;
    v_desde := make_date(t.ejercicio, 1, 1);
    v_hasta := make_date(t.ejercicio, 12, 31);

    DELETE FROM nomina.tb_bi_nominas WHERE id_periodo_nomina = p.id_periodo_nomina;
    SET LOCAL client_min_messages = warning;
    DROP TABLE IF EXISTS tmp_prom;
    CREATE TEMP TABLE tmp_prom ON COMMIT DROP AS
        SELECT id_empleado, promedio_tres_anios FROM nomina.tb_bi_ptu_empleados WHERE id_ptu = p_id_ptu;
    DELETE FROM nomina.tb_bi_ptu_empleados WHERE id_ptu = p_id_ptu;

    INSERT INTO nomina.tb_bi_ptu_empleados (id_ptu, id_empleado, dias_trabajados, salario_anual, ptu_por_dias,
                                            ptu_por_salarios, ptu_calculada, tope_tres_meses, promedio_tres_anios, ptu_final)
    SELECT p_id_ptu, x.id_empleado, x.dias, x.salario, 0, 0, 0,
           round(COALESCE(x.sd, x.salario / NULLIF(x.dias, 0), 0) * 90, 2),
           COALESCE(tp.promedio_tres_anios, 0), 0
      FROM (
        SELECT emp.id_empleado,
               (LEAST(COALESCE(b.fecha_baja, v_hasta), v_hasta) - GREATEST(emp.fecha_ingreso, v_desde) + 1)
                 - (SELECT COUNT(DISTINCT fecha_dia) FROM recursos_humanos.vw_bi_asistencia_diaria a
                     WHERE a.id_empleado = emp.id_empleado AND a.activo
                       AND a.estatus_asistencia IN ('falta_injustificada','permiso_sin_goce')
                       AND a.fecha_dia BETWEEN v_desde AND v_hasta) AS dias,
               COALESCE((SELECT SUM(d.importe_total) FROM nomina.tb_bi_nomina_detalle d
                           JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = d.id_concepto
                                                               AND c.clave_concepto IN ('P001','P002','P003','P004')
                           JOIN nomina.tb_bi_nominas n ON n.id_nomina = d.id_nomina AND n.id_empleado = emp.id_empleado
                           JOIN nomina.vw_bi_periodos_nomina pp ON pp.id_periodo_nomina = n.id_periodo_nomina
                          WHERE pp.ejercicio = t.ejercicio AND pp.tipo_nomina = 'ordinaria'
                            AND n.estatus_recibo <> 'cancelado'), 0) AS salario,
               s.salario_base AS sd
          FROM recursos_humanos.tb_bi_empleados emp
          JOIN nomina.tb_bi_empleados_nomina en ON en.id_empleado = emp.id_empleado AND en.activo AND NOT en.excluido_ptu
          LEFT JOIN LATERAL (SELECT MAX(fecha_baja) AS fecha_baja FROM recursos_humanos.tb_bi_bajas_laborales
                              WHERE id_empleado = emp.id_empleado AND activo) b ON TRUE
          LEFT JOIN LATERAL (SELECT salario_base FROM recursos_humanos.tb_bi_historial_sueldos
                              WHERE id_empleado = emp.id_empleado AND activo AND fecha_inicio_vigencia <= v_hasta
                              ORDER BY fecha_inicio_vigencia DESC LIMIT 1) s ON TRUE
         WHERE emp.id_empresa = t.id_empresa AND emp.activo AND emp.fecha_ingreso <= v_hasta
           AND (b.fecha_baja IS NULL OR b.fecha_baja >= v_desde)
      ) x
      LEFT JOIN tmp_prom tp ON tp.id_empleado = x.id_empleado
     WHERE x.dias >= 60;

    IF t.tope_salario_confianza IS NOT NULL THEN
        UPDATE nomina.tb_bi_ptu_empleados pe SET salario_anual = LEAST(pe.salario_anual, t.tope_salario_confianza * pe.dias_trabajados)
          FROM nomina.tb_bi_empleados_nomina en
         WHERE pe.id_ptu = p_id_ptu AND en.id_empleado = pe.id_empleado AND NOT en.sindicalizado;
    END IF;

    SELECT SUM(dias_trabajados), SUM(salario_anual) INTO v_sum_dias, v_sum_sal
      FROM nomina.tb_bi_ptu_empleados WHERE id_ptu = p_id_ptu;
    IF v_sum_dias IS NULL THEN
        RAISE EXCEPTION 'Ningún empleado trabajó al menos 60 días en %; no hay a quién repartir PTU', t.ejercicio;
    END IF;
    IF COALESCE(v_sum_sal, 0) = 0 THEN
        RAISE EXCEPTION 'El ejercicio % no tiene salarios pagados en nómina ordinaria; no se puede repartir por salarios', t.ejercicio;
    END IF;

    UPDATE nomina.tb_bi_ptu_empleados SET
        ptu_por_dias     = round(t.monto_repartir / 2 * dias_trabajados / NULLIF(v_sum_dias, 0), 2),
        ptu_por_salarios = round(t.monto_repartir / 2 * salario_anual / NULLIF(v_sum_sal, 0), 2)
     WHERE id_ptu = p_id_ptu;
    UPDATE nomina.tb_bi_ptu_empleados SET
        ptu_calculada = COALESCE(ptu_por_dias, 0) + COALESCE(ptu_por_salarios, 0),
        ptu_final = LEAST(COALESCE(ptu_por_dias, 0) + COALESCE(ptu_por_salarios, 0),
                          GREATEST(tope_tres_meses, promedio_tres_anios))
     WHERE id_ptu = p_id_ptu;

    FOR r IN SELECT pe.*, COALESCE(s.salario_base, pe.salario_anual / NULLIF(pe.dias_trabajados, 0), 0) AS sd
               FROM nomina.tb_bi_ptu_empleados pe
               LEFT JOIN LATERAL (SELECT salario_base FROM recursos_humanos.tb_bi_historial_sueldos
                                   WHERE id_empleado = pe.id_empleado AND activo AND fecha_inicio_vigencia <= p.fecha_pago
                                   ORDER BY fecha_inicio_vigencia DESC LIMIT 1) s ON TRUE
              WHERE pe.id_ptu = p_id_ptu AND pe.ptu_final > 0
    LOOP
        INSERT INTO nomina.tb_bi_nominas (id_periodo_nomina, id_empleado, salario_diario, dias_pagados)
        VALUES (p.id_periodo_nomina, r.id_empleado, r.sd, r.dias_trabajados) RETURNING id_nomina INTO v_id;
        PERFORM nomina.fn_agregar_concepto(v_id, 'P009', r.dias_trabajados, r.ptu_final);
        SELECT importe_gravado INTO v_grav FROM nomina.tb_bi_nomina_detalle d
          JOIN nomina.vw_bi_conceptos_nomina c USING (id_concepto) WHERE d.id_nomina = v_id AND c.clave_concepto = 'P009';
        UPDATE nomina.tb_bi_nominas SET isr_causado = nomina.fn_isr_art174(v_grav, r.sd * 30.4, p.fecha_pago)
         WHERE id_nomina = v_id;
        PERFORM nomina.fn_agregar_concepto(v_id, 'D002', 1, nomina.fn_isr_art174(v_grav, r.sd * 30.4, p.fecha_pago));
        PERFORM nomina.fn_aplicar_deducciones(v_id, 0, 0, FALSE, FALSE);
        v_total := v_total + 1;
    END LOOP;

    UPDATE nomina.tb_bi_ptu SET estatus = 'calculado' WHERE id_ptu = p_id_ptu;
    UPDATE nomina.tb_bi_periodos_nomina SET estatus_cierre = 'calculado' WHERE id_periodo_nomina = p.id_periodo_nomina;
    RETURN v_total;
END;
$$;

CREATE FUNCTION nomina.fn_cerrar_periodo(p_id_periodo integer, p_id_usuario integer DEFAULT 1) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM nomina.vw_bi_periodos_nomina
                    WHERE id_periodo_nomina = p_id_periodo AND estatus_cierre IN ('autorizado','timbrado')) THEN
        RAISE EXCEPTION 'Primero debe autorizarse el periodo';
    END IF;

    UPDATE nomina.tb_bi_conceptos_hist h
       SET parcialidades_pagadas = LEAST(h.parcialidades_pagadas + x.veces, h.total_parcialidades),
           saldo_restante = GREATEST(h.saldo_restante - x.importe, 0),
           id_usuario_modificacion = p_id_usuario, fecha_modificacion = now()
      FROM (SELECT d.id_hist_concepto, COUNT(*) AS veces, SUM(d.importe_total) AS importe
              FROM nomina.tb_bi_nomina_detalle d JOIN nomina.tb_bi_nominas n USING (id_nomina)
             WHERE n.id_periodo_nomina = p_id_periodo AND n.estatus_recibo <> 'cancelado' AND d.id_hist_concepto IS NOT NULL
             GROUP BY d.id_hist_concepto) x
     WHERE h.id_hist_concepto = x.id_hist_concepto;

    UPDATE nomina.tb_bi_creditos_fonacot f
       SET saldo = GREATEST(f.saldo - x.importe, 0), id_usuario_modificacion = p_id_usuario, fecha_modificacion = now()
      FROM (SELECT d.id_credito_fonacot, SUM(d.importe_total) AS importe
              FROM nomina.tb_bi_nomina_detalle d JOIN nomina.tb_bi_nominas n USING (id_nomina)
             WHERE n.id_periodo_nomina = p_id_periodo AND n.estatus_recibo <> 'cancelado' AND d.id_credito_fonacot IS NOT NULL
             GROUP BY d.id_credito_fonacot) x
     WHERE f.id_credito_fonacot = x.id_credito_fonacot;

    UPDATE nomina.tb_bi_periodos_nomina SET estatus_cierre = 'cerrado', id_usuario_modificacion = p_id_usuario,
                                            fecha_modificacion = now()
     WHERE id_periodo_nomina = p_id_periodo;
END;
$$;

CREATE FUNCTION nomina.fn_cuotas_imss(p_sbc numeric, p_salario_diario numeric, p_dias_eym numeric, p_dias_otros numeric, p_dias_retiro numeric, p_prima_rt numeric, p_fecha date, p_area text DEFAULT 'general'::text) RETURNS TABLE(clave_ramo character varying, dias numeric, base numeric, cuota_patron numeric, cuota_obrero numeric)
    LANGUAGE plpgsql STABLE
    AS $$
DECLARE
    v_uma  NUMERIC := nomina.fn_indicador('uma', p_fecha);
    v_sm   NUMERIC := nomina.fn_indicador(CASE WHEN p_area = 'frontera' THEN 'salario_minimo_frontera'
                                               ELSE 'salario_minimo_general' END, p_fecha);
    v_cv   NUMERIC;
    v_es_minimo BOOLEAN := p_salario_diario <= v_sm;
    r      RECORD;
    v_pat  NUMERIC;
    v_obr  NUMERIC;
BEGIN
    IF p_sbc <= v_sm THEN
        SELECT porcentaje INTO v_cv FROM nomina.tb_bi_cesantia_vejez_patron
         WHERE anio = EXTRACT(YEAR FROM p_fecha) AND veces_uma_desde = 0;
    ELSE
        SELECT porcentaje INTO v_cv FROM nomina.tb_bi_cesantia_vejez_patron
         WHERE anio = EXTRACT(YEAR FROM p_fecha) AND veces_uma_desde > 0
           AND veces_uma_desde <= round(p_sbc / v_uma, 2)
         ORDER BY veces_uma_desde DESC LIMIT 1;
        IF v_cv IS NULL THEN
            SELECT porcentaje INTO v_cv FROM nomina.tb_bi_cesantia_vejez_patron
             WHERE anio = EXTRACT(YEAR FROM p_fecha) AND veces_uma_desde > 0
             ORDER BY veces_uma_desde LIMIT 1;
        END IF;
    END IF;
    IF v_cv IS NULL THEN RAISE EXCEPTION 'No hay tabla de cesantía y vejez para %', EXTRACT(YEAR FROM p_fecha); END IF;

    FOR r IN SELECT * FROM nomina.tb_bi_cuotas_imss c
              WHERE c.activo AND c.fecha_inicio <= p_fecha AND (c.fecha_fin IS NULL OR c.fecha_fin >= p_fecha)
              ORDER BY c.id_cuota_imss
    LOOP
        clave_ramo := r.clave_ramo;
        dias := CASE r.tipo_dias WHEN 'eym' THEN p_dias_eym WHEN 'otros' THEN p_dias_otros ELSE p_dias_retiro END;
        dias := GREATEST(dias, 0);
        base := CASE r.base_calculo WHEN 'uma' THEN v_uma
                                    WHEN 'excedente_3_uma' THEN GREATEST(p_sbc - 3 * v_uma, 0)
                                    ELSE p_sbc END;
        v_pat := COALESCE(r.porcentaje_patron, CASE r.clave_ramo WHEN 'rt' THEN p_prima_rt WHEN 'cv' THEN v_cv ELSE 0 END);
        v_obr := r.porcentaje_obrero;
        cuota_patron := round(base * dias * v_pat / 100, 2);
        cuota_obrero := round(base * dias * v_obr / 100, 2);
        IF v_es_minimo THEN
            cuota_patron := cuota_patron + cuota_obrero;
            cuota_obrero := 0;
        END IF;
        IF base > 0 AND dias > 0 THEN RETURN NEXT; END IF;
    END LOOP;
END;
$$;

CREATE FUNCTION nomina.fn_exento_acumulado(p_id_empleado integer, p_tipo_exencion text, p_ejercicio integer, p_excluir_nomina integer) RETURNS numeric
    LANGUAGE sql STABLE
    AS $$
    SELECT COALESCE(SUM(d.importe_exento), 0)
      FROM nomina.tb_bi_nomina_detalle d
      JOIN nomina.tb_bi_nominas n           ON n.id_nomina = d.id_nomina
      JOIN nomina.vw_bi_periodos_nomina p   ON p.id_periodo_nomina = n.id_periodo_nomina
      JOIN nomina.vw_bi_conceptos_nomina c  ON c.id_concepto = d.id_concepto
     WHERE n.id_empleado = p_id_empleado AND c.tipo_exencion = p_tipo_exencion
       AND EXTRACT(YEAR FROM p.fecha_pago) = p_ejercicio
       AND n.estatus_recibo <> 'cancelado' AND n.id_nomina <> COALESCE(p_excluir_nomina, 0);
$$;

CREATE FUNCTION nomina.fn_factor_integracion(p_id_esquema integer, p_anios_cumplidos integer) RETURNS numeric
    LANGUAGE sql STABLE
    AS $$
    SELECT round(1 + e.dias_aguinaldo / 365.0
                   + recursos_humanos.fn_dias_vacaciones_ley(p_anios_cumplidos + 1)
                     * e.porcentaje_prima_vacacional / 100 / 365.0, 4)
      FROM nomina.tb_bi_esquemas_prestaciones e
     WHERE e.id_esquema_prestaciones = p_id_esquema;
$$;

CREATE FUNCTION nomina.fn_generar_periodos(p_id_empresa integer, p_ejercicio integer, p_periodicidad text DEFAULT 'quincenal'::text) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v        INT;
    v_id_per INT := (SELECT id_periodicidad_pago FROM nomina.tb_bi_periodicidades_pago WHERE clave = p_periodicidad);
BEGIN
    IF p_periodicidad = 'quincenal' THEN
        INSERT INTO nomina.tb_bi_periodos_nomina (id_empresa, id_periodicidad_pago, ejercicio, numero_periodo, descripcion,
                                                  fecha_inicio, fecha_fin, fecha_pago, dias_pago)
        SELECT p_id_empresa, v_id_per, p_ejercicio, (EXTRACT(MONTH FROM m)::int - 1) * 2 + q,
               CASE WHEN q = 1 THEN '1ra' ELSE '2da' END || ' quincena ' ||
               (ARRAY['enero','febrero','marzo','abril','mayo','junio','julio','agosto','septiembre','octubre',
                      'noviembre','diciembre'])[EXTRACT(MONTH FROM m)::int] || ' ' || p_ejercicio,
               CASE WHEN q = 1 THEN m::date ELSE (m + INTERVAL '15 days')::date END,
               CASE WHEN q = 1 THEN (m + INTERVAL '14 days')::date ELSE (m + INTERVAL '1 month - 1 day')::date END,
               CASE WHEN q = 1 THEN (m + INTERVAL '14 days')::date ELSE (m + INTERVAL '1 month - 1 day')::date END,
               15
          FROM generate_series(make_date(p_ejercicio, 1, 1), make_date(p_ejercicio, 12, 1), INTERVAL '1 month') m
         CROSS JOIN (VALUES (1), (2)) t(q)
        ON CONFLICT DO NOTHING;
    ELSIF p_periodicidad = 'semanal' THEN
        INSERT INTO nomina.tb_bi_periodos_nomina (id_empresa, id_periodicidad_pago, ejercicio, numero_periodo, descripcion,
                                                  fecha_inicio, fecha_fin, fecha_pago, dias_pago)
        SELECT p_id_empresa, v_id_per, p_ejercicio, row_number() OVER (ORDER BY d)::int,
               'Semana ' || row_number() OVER (ORDER BY d) || ' ' || p_ejercicio,
               d::date, (d + INTERVAL '6 days')::date, (d + INTERVAL '5 days')::date, 7
          FROM generate_series(date_trunc('week', make_date(p_ejercicio, 1, 4))::date,
                               make_date(p_ejercicio, 12, 31), INTERVAL '7 days') d
        ON CONFLICT DO NOTHING;
    ELSIF p_periodicidad = 'mensual' THEN
        INSERT INTO nomina.tb_bi_periodos_nomina (id_empresa, id_periodicidad_pago, ejercicio, numero_periodo, descripcion,
                                                  fecha_inicio, fecha_fin, fecha_pago, dias_pago)
        SELECT p_id_empresa, v_id_per, p_ejercicio, EXTRACT(MONTH FROM m)::int,
               initcap(to_char(m, 'TMMonth')) || ' ' || p_ejercicio,
               m::date, (m + INTERVAL '1 month - 1 day')::date, (m + INTERVAL '1 month - 1 day')::date, 30
          FROM generate_series(make_date(p_ejercicio, 1, 1), make_date(p_ejercicio, 12, 1), INTERVAL '1 month') m
        ON CONFLICT DO NOTHING;
    ELSE
        RAISE EXCEPTION 'Periodicidad % no soportada', p_periodicidad;
    END IF;
    GET DIAGNOSTICS v = ROW_COUNT;
    RETURN v;
END;
$$;

CREATE FUNCTION nomina.fn_indicador(p_clave text, p_fecha date) RETURNS numeric
    LANGUAGE sql STABLE
    AS $$
    SELECT i.valor_diario FROM nomina.tb_bi_indicadores i
      JOIN nomina.tb_bi_tipos_indicador t ON t.id_tipo_indicador = i.id_tipo_indicador
     WHERE t.clave = p_clave AND i.activo AND i.fecha_inicio <= p_fecha AND (i.fecha_fin IS NULL OR i.fecha_fin >= p_fecha)
     ORDER BY i.fecha_inicio DESC LIMIT 1;
$$;

CREATE FUNCTION nomina.fn_isr_art174(p_gravado numeric, p_sueldo_mensual numeric, p_fecha date) RETURNS numeric
    LANGUAGE plpgsql STABLE
    AS $$
DECLARE
    v_x      NUMERIC;
    v_tasa   NUMERIC;
BEGIN
    IF p_gravado <= 0 THEN RETURN 0; END IF;
    v_x := p_gravado / 365 * 30.4;
    v_tasa := (nomina.fn_isr_tarifa(p_sueldo_mensual + v_x, 'mensual', p_fecha)
               - nomina.fn_isr_tarifa(p_sueldo_mensual, 'mensual', p_fecha)) / v_x;
    RETURN round(p_gravado * v_tasa, 2);
END;
$$;

CREATE FUNCTION nomina.fn_isr_periodo(p_gravado numeric, p_periodicidad text, p_dias numeric, p_fecha date) RETURNS numeric
    LANGUAGE plpgsql STABLE
    AS $$
BEGIN
    IF p_gravado <= 0 OR p_dias <= 0 THEN RETURN 0; END IF;
    IF p_periodicidad IN ('semanal','quincenal','mensual') THEN
        RETURN nomina.fn_isr_tarifa(p_gravado, p_periodicidad, p_fecha);
    END IF;
    RETURN round(nomina.fn_isr_tarifa(p_gravado / p_dias, 'diaria', p_fecha) * p_dias, 2);
END;
$$;

CREATE FUNCTION nomina.fn_isr_tarifa(p_base numeric, p_periodicidad text, p_fecha date) RETURNS numeric
    LANGUAGE plpgsql STABLE
    AS $$
DECLARE
    v_anio INT;
    r      RECORD;
BEGIN
    IF p_base IS NULL OR p_base <= 0 THEN RETURN 0; END IF;
    SELECT MAX(anio) INTO v_anio FROM nomina.tb_bi_tarifas_isr
     WHERE periodicidad = p_periodicidad AND anio <= EXTRACT(YEAR FROM p_fecha) AND activo;
    IF v_anio IS NULL THEN
        RAISE EXCEPTION 'No hay tarifa ISR % para %', p_periodicidad, p_fecha;
    END IF;
    SELECT * INTO r FROM nomina.tb_bi_tarifas_isr
     WHERE anio = v_anio AND periodicidad = p_periodicidad AND activo AND limite_inferior <= p_base
     ORDER BY limite_inferior DESC LIMIT 1;
    RETURN round(r.cuota_fija + (p_base - r.limite_inferior) * r.porcentaje_excedente / 100, 2);
END;
$$;

CREATE FUNCTION nomina.fn_sbc(p_sdi numeric, p_fecha date, p_area text DEFAULT 'general'::text) RETURNS numeric
    LANGUAGE sql STABLE
    AS $$
    SELECT round(LEAST(GREATEST(p_sdi,
                 nomina.fn_indicador(CASE WHEN p_area = 'frontera' THEN 'salario_minimo_frontera'
                                          ELSE 'salario_minimo_general' END, p_fecha)),
                 25 * nomina.fn_indicador('uma', p_fecha)), 2);
$$;

CREATE FUNCTION nomina.fn_sdi_sugerido(p_id_empleado integer, p_salario_diario numeric, p_fecha date) RETURNS TABLE(factor numeric, sdi numeric, sbc numeric)
    LANGUAGE sql STABLE
    AS $$
    SELECT f.factor, round(p_salario_diario * f.factor, 2),
           nomina.fn_sbc(round(p_salario_diario * f.factor, 2), p_fecha, rp.area_salario_minimo)
      FROM nomina.tb_bi_empleados_nomina en
      JOIN recursos_humanos.tb_bi_empleados e ON e.id_empleado = en.id_empleado
      JOIN nomina.vw_bi_registros_patronales rp ON rp.id_registro_patronal = en.id_registro_patronal
      CROSS JOIN LATERAL (SELECT nomina.fn_factor_integracion(en.id_esquema_prestaciones,
                              EXTRACT(YEAR FROM age(p_fecha, e.fecha_ingreso))::int) AS factor) f
     WHERE en.id_empleado = p_id_empleado;
$$;

CREATE FUNCTION nomina.fn_subsidio_empleo(p_gravado numeric, p_dias numeric, p_fecha date) RETURNS numeric
    LANGUAGE plpgsql STABLE
    AS $$
DECLARE r RECORD;
BEGIN
    IF p_dias <= 0 THEN RETURN 0; END IF;
    SELECT * INTO r FROM nomina.tb_bi_subsidio_empleo
     WHERE activo AND fecha_inicio <= p_fecha AND (fecha_fin IS NULL OR fecha_fin >= p_fecha)
     ORDER BY fecha_inicio DESC LIMIT 1;
    IF r IS NULL OR p_gravado / p_dias * 30.4 > r.ingreso_tope_mensual THEN RETURN 0; END IF;
    RETURN round(r.cuota_mensual / 30.4 * p_dias, 2);
END;
$$;

CREATE FUNCTION nomina.fn_totalizar_nomina(p_id_nomina integer) RETURNS void
    LANGUAGE sql
    AS $$
    UPDATE nomina.tb_bi_nominas n SET
        total_percepciones = t.percepciones,
        total_gravado      = t.gravado,
        total_exento       = t.exento,
        total_deducciones  = t.deducciones,
        total_otros_pagos  = t.otros,
        isr_retenido       = t.isr,
        imss_obrero        = t.imss,
        neto_a_pagar       = t.percepciones + t.otros - t.deducciones
      FROM (SELECT COALESCE(SUM(d.importe_total) FILTER (WHERE c.tipo_concepto = 'percepcion'), 0) AS percepciones,
                   COALESCE(SUM(d.importe_gravado) FILTER (WHERE c.tipo_concepto = 'percepcion'), 0) AS gravado,
                   COALESCE(SUM(d.importe_exento) FILTER (WHERE c.tipo_concepto = 'percepcion'), 0) AS exento,
                   COALESCE(SUM(d.importe_total) FILTER (WHERE c.tipo_concepto = 'deduccion'), 0) AS deducciones,
                   COALESCE(SUM(d.importe_total) FILTER (WHERE c.tipo_concepto = 'otro_pago'), 0) AS otros,
                   COALESCE(SUM(d.importe_total) FILTER (WHERE c.clave_concepto = 'D002'), 0) AS isr,
                   COALESCE(SUM(d.importe_total) FILTER (WHERE c.clave_concepto = 'D001'), 0) AS imss
              FROM nomina.tb_bi_nomina_detalle d
              JOIN nomina.vw_bi_conceptos_nomina c ON c.id_concepto = d.id_concepto
             WHERE d.id_nomina = p_id_nomina) t
     WHERE n.id_nomina = p_id_nomina;
$$;

CREATE FUNCTION recursos_humanos.fn_auditar() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_ocultas  TEXT[] := ARRAY['password_hash', 'id_usuario_modificacion', 'fecha_modificacion'];
    v_previos  JSONB;
    v_nuevos   JSONB;
    v_registro JSONB;
    v_usuario  INT;
BEGIN
    IF TG_OP = 'UPDATE' THEN
        SELECT jsonb_object_agg(n.key, o.value), jsonb_object_agg(n.key, n.value)
          INTO v_previos, v_nuevos
          FROM jsonb_each(to_jsonb(NEW)) n
          JOIN jsonb_each(to_jsonb(OLD)) o ON o.key = n.key
         WHERE n.value IS DISTINCT FROM o.value AND NOT n.key = ANY(v_ocultas);
        IF v_nuevos IS NULL THEN RETURN NEW; END IF;
        v_registro := to_jsonb(NEW);
    ELSIF TG_OP = 'INSERT' THEN
        v_nuevos := to_jsonb(NEW) - v_ocultas;
        v_registro := to_jsonb(NEW);
    ELSE
        v_previos := to_jsonb(OLD) - v_ocultas;
        v_registro := to_jsonb(OLD);
    END IF;

    v_usuario := COALESCE(NULLIF(current_setting('app.id_usuario', TRUE), '')::INT,
                          (v_registro ->> 'id_usuario_modificacion')::INT,
                          (v_registro ->> 'id_usuario_creacion')::INT,
                          1);

    INSERT INTO recursos_humanos.tb_bi_auditoria_logs
           (id_usuario, tabla_afectada, id_registro, accion, json_datos_previos, json_datos_nuevos, ip_origen)
    VALUES (v_usuario, TG_TABLE_SCHEMA || '.' || TG_TABLE_NAME, v_registro ->> TG_ARGV[0], TG_OP,
            v_previos, v_nuevos, NULLIF(current_setting('app.ip_origen', TRUE), ''));
    RETURN COALESCE(NEW, OLD);
END;
$$;

CREATE FUNCTION recursos_humanos.fn_dias_vacaciones_ley(p_anios integer) RETURNS integer
    LANGUAGE sql STABLE
    AS $$
    SELECT dias_de_ley
      FROM recursos_humanos.tb_bi_lft_vacaciones
     WHERE activo
       AND anio_antiguedad = LEAST(GREATEST(p_anios, 1),
                                   (SELECT MAX(anio_antiguedad) FROM recursos_humanos.tb_bi_lft_vacaciones WHERE activo));
$$;

CREATE FUNCTION recursos_humanos.fn_generar_asistencia_diaria(p_fecha date) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_dia        SMALLINT := EXTRACT(ISODOW FROM p_fecha);
    v_total      INT := 0;
    r            RECORD;
    v_entrada    TIMESTAMP;
    v_salida     TIMESTAMP;
    v_ini        TIMESTAMP;
    v_fin        TIMESTAMP;
    v_retardo    INT;
    v_estatus    VARCHAR(30);
    v_horas      NUMERIC(5,2);
    v_prog       NUMERIC(5,2);
    v_id_sol     INT;
    v_est_sol    VARCHAR(30);
BEGIN
    IF EXISTS (SELECT 1 FROM recursos_humanos.tb_bi_dias_festivos
                WHERE fecha = p_fecha AND NOT es_laborable AND activo) THEN
        DELETE FROM recursos_humanos.tb_bi_asistencia_diaria d
         USING recursos_humanos.tb_bi_empleados e
         WHERE e.id_empleado = d.id_empleado AND d.fecha_dia = p_fecha
           AND NOT EXISTS (SELECT 1 FROM nomina.vw_bi_periodos_nomina pn
                            WHERE pn.id_empresa = e.id_empresa AND pn.tipo_nomina = 'ordinaria' AND pn.activo
                              AND p_fecha BETWEEN pn.fecha_inicio AND pn.fecha_fin
                              AND pn.estatus_cierre IN ('autorizado','timbrado','cerrado'));
        RETURN 0;
    END IF;

    FOR r IN
        SELECT h.*, rg.minutos_tolerancia, rg.minutos_limite_falta, mc.clave AS modo_checada
          FROM recursos_humanos.tb_bi_historial_horarios h
          JOIN recursos_humanos.tb_bi_empleados e         ON e.id_empleado = h.id_empleado
          JOIN recursos_humanos.tb_bi_estatus_laborales el ON el.id_estatus_laboral = e.id_estatus_laboral
          JOIN recursos_humanos.tb_bi_reglas_asistencia rg ON rg.id_regla_asistencia = h.id_regla_asistencia
          JOIN recursos_humanos.tb_bi_modos_checada mc     ON mc.id_modo_checada = rg.id_modo_checada
         WHERE h.activo
           AND h.dia_semana = v_dia
           AND p_fecha >= h.fecha_inicio_vigencia
           AND (h.fecha_fin_vigencia IS NULL OR p_fecha <= h.fecha_fin_vigencia)
           AND el.clave = 'activo' AND e.activo
           AND e.fecha_ingreso <= p_fecha
           AND NOT EXISTS (SELECT 1 FROM nomina.vw_bi_periodos_nomina pn
                            WHERE pn.id_empresa = e.id_empresa AND pn.tipo_nomina = 'ordinaria' AND pn.activo
                              AND p_fecha BETWEEN pn.fecha_inicio AND pn.fecha_fin
                              AND pn.estatus_cierre IN ('autorizado','timbrado','cerrado'))
    LOOP
        v_ini := p_fecha + r.hora_entrada;
        v_fin := p_fecha + r.hora_salida;
        v_prog := (EXTRACT(EPOCH FROM (r.hora_salida - r.hora_entrada))
                   - COALESCE(EXTRACT(EPOCH FROM (r.hora_comida_fin - r.hora_comida_inicio)), 0)) / 3600.0;

        SELECT MIN(a.fecha_hora_registro) INTO v_entrada
          FROM recursos_humanos.tb_bi_asistencias a
         WHERE a.id_empleado = r.id_empleado AND a.activo
           AND a.fecha_hora_registro >= CASE WHEN r.modo_checada = 'por_clase'
                                             THEN v_ini - INTERVAL '60 minutes'
                                             ELSE p_fecha::timestamp END
           AND a.fecha_hora_registro < v_fin;

        SELECT MAX(a.fecha_hora_registro) INTO v_salida
          FROM recursos_humanos.tb_bi_asistencias a
         WHERE a.id_empleado = r.id_empleado AND a.activo
           AND a.fecha_hora_registro > COALESCE(v_entrada, v_ini)
           AND a.fecha_hora_registro <= v_fin + INTERVAL '3 hours';

        v_retardo := CASE WHEN v_entrada IS NULL THEN 0
                          ELSE GREATEST(0, FLOOR(EXTRACT(EPOCH FROM (v_entrada - v_ini)) / 60))::int END;

        IF v_entrada IS NULL OR v_retardo > r.minutos_limite_falta THEN
            v_estatus := 'falta_injustificada';
        ELSIF v_retardo > r.minutos_tolerancia THEN
            v_estatus := 'retardo';
        ELSE
            v_estatus := 'a_tiempo';
        END IF;

        v_horas := CASE
            WHEN v_estatus = 'falta_injustificada' THEN 0
            WHEN v_salida IS NULL THEN v_prog
            ELSE LEAST(v_prog, GREATEST(0, EXTRACT(EPOCH FROM (LEAST(v_salida, v_fin)
                                                                - GREATEST(v_entrada, v_ini))) / 3600.0))
        END;

        v_id_sol := NULL; v_est_sol := NULL;
        IF EXISTS (SELECT 1 FROM recursos_humanos.tb_bi_incapacidades_imss i
                    WHERE i.id_empleado = r.id_empleado AND i.activo
                      AND p_fecha BETWEEN i.fecha_inicio AND i.fecha_fin) THEN
            v_estatus := 'incapacidad';  v_horas := 0;
        ELSIF EXISTS (SELECT 1 FROM recursos_humanos.tb_bi_suplencias s
                       WHERE s.id_historial_horario = r.id_historial_horario AND s.activo
                         AND s.fecha_suplencia = p_fecha AND s.estatus_aprobacion = 'aprobada') THEN
            v_estatus := 'suplencia';    v_horas := 0;
        ELSE
            SELECT s.id_solicitud, ea.clave INTO v_id_sol, v_est_sol
              FROM recursos_humanos.tb_bi_solicitudes s
              JOIN recursos_humanos.tb_bi_tipos_permiso t       ON t.id_tipo_permiso = s.id_tipo_permiso
              JOIN recursos_humanos.tb_bi_estatus_asistencia ea ON ea.id_estatus_asistencia = t.id_estatus_asistencia
             WHERE s.id_empleado = r.id_empleado AND s.activo
               AND s.estatus_aprobacion = 'aprobada'
               AND p_fecha BETWEEN s.fecha_inicio AND s.fecha_fin
               AND (s.hora_inicio_permiso IS NULL
                    OR (s.hora_inicio_permiso < r.hora_salida AND s.hora_fin_permiso > r.hora_entrada))
             ORDER BY s.fecha_creacion DESC
             LIMIT 1;

            IF v_id_sol IS NOT NULL
               AND (v_estatus IN ('falta_injustificada','retardo') OR v_est_sol = 'vacaciones') THEN
                v_estatus := v_est_sol;
                IF v_estatus <> 'falta_justificada' OR v_horas = 0 THEN
                    v_horas := CASE WHEN v_estatus = 'falta_justificada' THEN v_prog ELSE 0 END;
                END IF;
            ELSE
                v_id_sol := NULL;
            END IF;
        END IF;

        INSERT INTO recursos_humanos.tb_bi_asistencia_diaria AS d
               (id_empleado, id_historial_horario, id_solicitud, fecha_dia,
                id_estatus_asistencia, minutos_retardo, horas_cubiertas)
        VALUES (r.id_empleado, r.id_historial_horario, v_id_sol, p_fecha,
                (SELECT id_estatus_asistencia FROM recursos_humanos.tb_bi_estatus_asistencia WHERE clave = v_estatus),
                CASE WHEN v_estatus = 'retardo' THEN v_retardo ELSE 0 END, round(v_horas, 2))
        ON CONFLICT (id_historial_horario, fecha_dia) DO UPDATE SET
                id_solicitud            = EXCLUDED.id_solicitud,
                id_estatus_asistencia   = EXCLUDED.id_estatus_asistencia,
                minutos_retardo         = EXCLUDED.minutos_retardo,
                horas_cubiertas         = EXCLUDED.horas_cubiertas,
                id_usuario_modificacion = 1,
                fecha_modificacion      = now()
          WHERE d.id_estatus_asistencia IS DISTINCT FROM EXCLUDED.id_estatus_asistencia
             OR d.minutos_retardo    IS DISTINCT FROM EXCLUDED.minutos_retardo
             OR d.horas_cubiertas    IS DISTINCT FROM EXCLUDED.horas_cubiertas
             OR d.id_solicitud       IS DISTINCT FROM EXCLUDED.id_solicitud;

        v_total := v_total + 1;
    END LOOP;

    RETURN v_total;
END;
$$;

CREATE FUNCTION recursos_humanos.fn_generar_asistencia_rango(p_inicio date, p_fin date) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE d DATE; v INT := 0;
BEGIN
    FOR d IN SELECT generate_series(p_inicio, p_fin, INTERVAL '1 day')::date LOOP
        v := v + recursos_humanos.fn_generar_asistencia_diaria(d);
    END LOOP;
    RETURN v;
END;
$$;

CREATE FUNCTION recursos_humanos.fn_generar_saldos_vacaciones(p_fecha date DEFAULT CURRENT_DATE) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE v_insertados INT;
BEGIN
    INSERT INTO recursos_humanos.tb_bi_saldos_vacaciones
           (id_empleado, id_lft_vacacion, anio_servicio, dias_otorgados, fecha_caducidad)
    SELECT e.id_empleado, l.id_lft_vacacion, n.anio, l.dias_de_ley,
           (e.fecha_ingreso + make_interval(years => n.anio, months => 18))::date
      FROM recursos_humanos.tb_bi_empleados e
      CROSS JOIN LATERAL generate_series(1, EXTRACT(YEAR FROM age(p_fecha, e.fecha_ingreso))::int) AS n(anio)
      JOIN recursos_humanos.tb_bi_lft_vacaciones l
        ON l.activo
       AND l.anio_antiguedad = LEAST(n.anio, (SELECT MAX(anio_antiguedad)
                                                FROM recursos_humanos.tb_bi_lft_vacaciones WHERE activo))
     WHERE e.id_estatus_laboral = (SELECT id_estatus_laboral FROM recursos_humanos.tb_bi_estatus_laborales WHERE clave = 'activo')
       AND e.activo
       AND (e.fecha_ingreso + make_interval(years => n.anio, months => 18))::date >= p_fecha
    ON CONFLICT (id_empleado, anio_servicio) DO NOTHING;
    GET DIAGNOSTICS v_insertados = ROW_COUNT;
    RETURN v_insertados;
END;
$$;

CREATE FUNCTION recursos_humanos.fn_preparar_cuentas_directorio() RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    r       RECORD;
    v_sam   VARCHAR;
    v_upn   VARCHAR;
    v_m365  BOOLEAN;
    v_total INT := 0;
BEGIN
    FOR r IN
        SELECT e.id_empleado, p.nombre, p.apellido_paterno, p.apellido_materno,
               lower(p.correo_electronico) AS correo, em.dominio_correo
          FROM recursos_humanos.tb_bi_empleados e
          JOIN recursos_humanos.tb_bi_personas p  ON p.id_persona = e.id_persona
          JOIN recursos_humanos.tb_bi_empresas em ON em.id_empresa = e.id_empresa
          JOIN recursos_humanos.tb_bi_estatus_laborales el ON el.id_estatus_laboral = e.id_estatus_laboral
         WHERE el.clave = 'activo' AND e.activo
           AND em.dominio_correo IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM recursos_humanos.tb_bi_cuentas_directorio c WHERE c.id_empleado = e.id_empleado)
         ORDER BY e.fecha_ingreso, e.id_empleado
    LOOP
        v_m365 := COALESCE(r.correo LIKE '%@' || lower(r.dominio_correo), FALSE);
        IF v_m365 THEN
            v_upn := r.correo;
            v_sam := left(regexp_replace(split_part(r.correo, '@', 1), '[^a-z0-9._-]', '', 'g'), 20);
            IF v_sam = '' OR EXISTS (SELECT 1 FROM recursos_humanos.tb_bi_cuentas_directorio WHERE sam_account_name = v_sam) THEN
                v_sam := recursos_humanos.fn_proponer_sam_account_name(r.nombre, r.apellido_paterno, r.apellido_materno);
            END IF;
        ELSE
            v_sam := recursos_humanos.fn_proponer_sam_account_name(r.nombre, r.apellido_paterno, r.apellido_materno);
            v_upn := v_sam || '@' || lower(r.dominio_correo);
        END IF;

        INSERT INTO recursos_humanos.tb_bi_cuentas_directorio
               (id_empleado, sam_account_name, user_principal_name, correo_institucional, cuenta_existente_m365)
        VALUES (r.id_empleado, v_sam, v_upn, v_upn, v_m365);
        v_total := v_total + 1;
    END LOOP;
    RETURN v_total;
END;
$$;

CREATE FUNCTION recursos_humanos.fn_proponer_sam_account_name(p_nombre text, p_apellido_paterno text, p_apellido_materno text DEFAULT NULL::text) RETURNS character varying
    LANGUAGE plpgsql STABLE
    AS $$
DECLARE
    v_base TEXT := left(left(recursos_humanos.fn_texto_cuenta(p_nombre), 1)
                        || recursos_humanos.fn_texto_cuenta(p_apellido_paterno), 18);
    v_sam  TEXT;
    v_n    INT := 1;
BEGIN
    IF v_base = '' THEN RAISE EXCEPTION 'No se puede generar la cuenta: nombre vacío'; END IF;
    v_sam := v_base;
    IF EXISTS (SELECT 1 FROM recursos_humanos.tb_bi_cuentas_directorio WHERE sam_account_name = v_sam)
       AND p_apellido_materno IS NOT NULL THEN
        v_sam := left(v_base, 17) || left(recursos_humanos.fn_texto_cuenta(p_apellido_materno), 1);
    END IF;
    WHILE EXISTS (SELECT 1 FROM recursos_humanos.tb_bi_cuentas_directorio WHERE sam_account_name = v_sam) LOOP
        v_n := v_n + 1;
        v_sam := left(v_base, 20 - length(v_n::text)) || v_n;
    END LOOP;
    RETURN v_sam;
END;
$$;

CREATE FUNCTION recursos_humanos.fn_registrar_sincronizacion(p_id_empleado integer, p_accion text, p_exitoso boolean, p_mensaje text DEFAULT NULL::text, p_object_guid uuid DEFAULT NULL::uuid, p_distinguished_name text DEFAULT NULL::text) RETURNS void
    LANGUAGE plpgsql SECURITY DEFINER
    SET search_path TO 'recursos_humanos', 'pg_temp'
    AS $$
DECLARE v_hash CHAR(32); v_atributos JSONB;
BEGIN
    SELECT hash_datos, to_jsonb(v) - 'hash_datos' - 'hash_sincronizado' - 'requiere_sincronizacion'
      INTO v_hash, v_atributos
      FROM recursos_humanos.vw_bi_directorio_empleados v WHERE v.id_empleado = p_id_empleado;

    INSERT INTO recursos_humanos.tb_bi_sincronizacion_directorio_logs
           (id_empleado, id_accion_directorio, exitoso, atributos_enviados, mensaje)
    VALUES (p_id_empleado,
            (SELECT id_accion_directorio FROM recursos_humanos.tb_bi_acciones_directorio WHERE clave = p_accion),
            p_exitoso, v_atributos, left(p_mensaje, 1000));

    UPDATE recursos_humanos.tb_bi_cuentas_directorio SET
           hash_sincronizado           = CASE WHEN p_exitoso THEN v_hash ELSE hash_sincronizado END,
           fecha_ultima_sincronizacion = CASE WHEN p_exitoso THEN now() ELSE fecha_ultima_sincronizacion END,
           ultimo_error                = CASE WHEN p_exitoso THEN NULL ELSE left(p_mensaje, 1000) END,
           object_guid                 = COALESCE(p_object_guid, object_guid),
           distinguished_name          = COALESCE(p_distinguished_name, distinguished_name),
           fecha_modificacion          = now()
     WHERE id_empleado = p_id_empleado;
END;
$$;

CREATE FUNCTION recursos_humanos.fn_texto_cuenta(p_texto text) RETURNS text
    LANGUAGE sql IMMUTABLE
    AS $$
    SELECT regexp_replace(translate(lower(COALESCE(p_texto, '')),
                                    'áàäâéèëêíìïîóòöôúùüûñç', 'aaaaeeeeiiiioooouuuunc'),
                          '[^a-z0-9]', '', 'g');
$$;

SET default_tablespace = '';

SET default_table_access_method = heap;

CREATE TABLE nomina.tb_bi_bancos (
    id_banco integer NOT NULL,
    clave character(3) NOT NULL,
    nombre_corto character varying(50) NOT NULL,
    razon_social character varying(200),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_bancos ALTER COLUMN id_banco ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_bancos_id_banco_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_cesantia_vejez_patron (
    id_cesantia_vejez integer NOT NULL,
    anio integer NOT NULL,
    veces_uma_desde numeric(6,2) NOT NULL,
    porcentaje numeric(7,4) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_cesantia_vejez_patron ALTER COLUMN id_cesantia_vejez ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_cesantia_vejez_patron_id_cesantia_vejez_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_clases_riesgo (
    id_clase_riesgo integer NOT NULL,
    clave character varying(2) NOT NULL,
    descripcion character varying(100) NOT NULL,
    prima_media numeric(8,5),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_clases_riesgo ALTER COLUMN id_clase_riesgo ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_clases_riesgo_id_clase_riesgo_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_claves_sat_concepto (
    id_clave_sat_concepto integer NOT NULL,
    id_tipo_concepto integer NOT NULL,
    clave character varying(5) NOT NULL,
    descripcion character varying(200) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_claves_sat_concepto ALTER COLUMN id_clave_sat_concepto ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_claves_sat_concepto_id_clave_sat_concepto_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_conceptos_hist (
    id_hist_concepto integer NOT NULL,
    id_empleado integer NOT NULL,
    id_concepto integer NOT NULL,
    monto_total_deuda numeric(12,2) NOT NULL,
    total_parcialidades integer NOT NULL,
    parcialidades_pagadas integer DEFAULT 0 NOT NULL,
    saldo_restante numeric(12,2) NOT NULL,
    fecha_inicio_descuento date DEFAULT CURRENT_DATE NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_conceptos_hist_parcialidades CHECK (((parcialidades_pagadas >= 0) AND (parcialidades_pagadas <= total_parcialidades))),
    CONSTRAINT ck_conceptos_hist_saldo CHECK (((saldo_restante >= (0)::numeric) AND (saldo_restante <= monto_total_deuda))),
    CONSTRAINT tb_bi_conceptos_hist_monto_total_deuda_check CHECK ((monto_total_deuda > (0)::numeric)),
    CONSTRAINT tb_bi_conceptos_hist_total_parcialidades_check CHECK ((total_parcialidades > 0))
);

ALTER TABLE nomina.tb_bi_conceptos_hist ALTER COLUMN id_hist_concepto ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_conceptos_hist_id_hist_concepto_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_conceptos_nomina (
    id_concepto integer NOT NULL,
    clave_concepto character varying(10) NOT NULL,
    nombre_concepto character varying(100) NOT NULL,
    id_tipo_concepto integer NOT NULL,
    id_clave_sat_concepto integer,
    id_tipo_exencion integer DEFAULT 1 NOT NULL,
    integra_sdi boolean DEFAULT false NOT NULL,
    es_calculado boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_conceptos_nomina ALTER COLUMN id_concepto ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_conceptos_nomina_id_concepto_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_conceptos_recurrentes (
    id_concepto_recurrente integer NOT NULL,
    id_empleado integer NOT NULL,
    id_concepto integer NOT NULL,
    importe numeric(12,2) NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_fin date,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT tb_bi_conceptos_recurrentes_importe_check CHECK ((importe > (0)::numeric))
);

ALTER TABLE nomina.tb_bi_conceptos_recurrentes ALTER COLUMN id_concepto_recurrente ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_conceptos_recurrentes_id_concepto_recurrente_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_creditos_fonacot (
    id_credito_fonacot integer NOT NULL,
    id_empleado integer NOT NULL,
    numero_credito character varying(20) NOT NULL,
    importe_total numeric(12,2) NOT NULL,
    retencion_mensual numeric(12,2) NOT NULL,
    saldo numeric(12,2) NOT NULL,
    fecha_inicio date NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_fonacot_saldo CHECK (((saldo >= (0)::numeric) AND (saldo <= importe_total))),
    CONSTRAINT tb_bi_creditos_fonacot_importe_total_check CHECK ((importe_total > (0)::numeric)),
    CONSTRAINT tb_bi_creditos_fonacot_retencion_mensual_check CHECK ((retencion_mensual > (0)::numeric))
);

ALTER TABLE nomina.tb_bi_creditos_fonacot ALTER COLUMN id_credito_fonacot ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_creditos_fonacot_id_credito_fonacot_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_creditos_infonavit (
    id_credito_infonavit integer NOT NULL,
    id_empleado integer NOT NULL,
    numero_credito character varying(10) NOT NULL,
    id_tipo_descuento integer NOT NULL,
    valor_descuento numeric(10,4) NOT NULL,
    incluye_seguro_vivienda boolean DEFAULT true NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_suspension date,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT tb_bi_creditos_infonavit_valor_descuento_check CHECK ((valor_descuento > (0)::numeric))
);

ALTER TABLE nomina.tb_bi_creditos_infonavit ALTER COLUMN id_credito_infonavit ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_creditos_infonavit_id_credito_infonavit_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_cuotas_imss (
    id_cuota_imss integer NOT NULL,
    clave_ramo character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    porcentaje_patron numeric(7,4),
    porcentaje_obrero numeric(7,4) DEFAULT 0 NOT NULL,
    base_calculo character varying(20) NOT NULL,
    tipo_dias character varying(10) NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_fin date,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_cuotas_base CHECK (((base_calculo)::text = ANY ((ARRAY['sbc'::character varying, 'uma'::character varying, 'excedente_3_uma'::character varying])::text[]))),
    CONSTRAINT ck_cuotas_dias CHECK (((tipo_dias)::text = ANY ((ARRAY['eym'::character varying, 'otros'::character varying, 'retiro'::character varying])::text[])))
);

ALTER TABLE nomina.tb_bi_cuotas_imss ALTER COLUMN id_cuota_imss ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_cuotas_imss_id_cuota_imss_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_empleados_nomina (
    id_empleado_nomina integer NOT NULL,
    id_empleado integer NOT NULL,
    id_registro_patronal integer NOT NULL,
    id_esquema_prestaciones integer NOT NULL,
    nss character(11),
    fecha_alta_imss date,
    id_periodicidad_pago integer DEFAULT 3 NOT NULL,
    id_tipo_contrato integer DEFAULT 1 NOT NULL,
    id_tipo_regimen integer DEFAULT 1 NOT NULL,
    id_tipo_jornada integer DEFAULT 1 NOT NULL,
    id_clase_riesgo_puesto integer DEFAULT 1 NOT NULL,
    sindicalizado boolean DEFAULT false NOT NULL,
    excluido_ptu boolean DEFAULT false NOT NULL,
    id_metodo_pago integer DEFAULT 1 NOT NULL,
    id_banco integer,
    clabe character(18),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_empleados_nomina_clabe CHECK (((clabe IS NULL) OR (clabe ~ '^[0-9]{18}$'::text))),
    CONSTRAINT ck_empleados_nomina_nss CHECK (((nss IS NULL) OR (nss ~ '^[0-9]{11}$'::text)))
);

ALTER TABLE nomina.tb_bi_empleados_nomina ALTER COLUMN id_empleado_nomina ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_empleados_nomina_id_empleado_nomina_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_esquemas_prestaciones (
    id_esquema_prestaciones integer NOT NULL,
    id_empresa integer NOT NULL,
    nombre_esquema character varying(100) NOT NULL,
    dias_aguinaldo integer DEFAULT 15 NOT NULL,
    porcentaje_prima_vacacional numeric(6,2) DEFAULT 25 CONSTRAINT tb_bi_esquemas_prestaciones_porcentaje_prima_vacaciona_not_null NOT NULL,
    porcentaje_prima_dominical numeric(6,2) DEFAULT 25 NOT NULL,
    descontar_faltas_aguinaldo boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT tb_bi_esquemas_prestaciones_dias_aguinaldo_check CHECK ((dias_aguinaldo >= 15)),
    CONSTRAINT tb_bi_esquemas_prestaciones_porcentaje_prima_dominical_check CHECK ((porcentaje_prima_dominical >= (25)::numeric)),
    CONSTRAINT tb_bi_esquemas_prestaciones_porcentaje_prima_vacacional_check CHECK ((porcentaje_prima_vacacional >= (25)::numeric))
);

ALTER TABLE nomina.tb_bi_esquemas_prestaciones ALTER COLUMN id_esquema_prestaciones ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_esquemas_prestaciones_id_esquema_prestaciones_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_estados (
    id_estado integer NOT NULL,
    clave character(3) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_estados ALTER COLUMN id_estado ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_estados_id_estado_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_incidencias_nomina (
    id_incidencia_nomina integer NOT NULL,
    id_periodo_nomina integer NOT NULL,
    id_empleado integer NOT NULL,
    id_concepto integer NOT NULL,
    cantidad numeric(10,2) DEFAULT 1 NOT NULL,
    importe numeric(12,2) NOT NULL,
    observaciones character varying(300),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT tb_bi_incidencias_nomina_importe_check CHECK ((importe > (0)::numeric))
);

ALTER TABLE nomina.tb_bi_incidencias_nomina ALTER COLUMN id_incidencia_nomina ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_incidencias_nomina_id_incidencia_nomina_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_indicadores (
    id_indicador integer NOT NULL,
    id_tipo_indicador integer NOT NULL,
    valor_diario numeric(12,4) NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_fin date,
    fuente character varying(200),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT tb_bi_indicadores_valor_diario_check CHECK ((valor_diario > (0)::numeric))
);

ALTER TABLE nomina.tb_bi_indicadores ALTER COLUMN id_indicador ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_indicadores_id_indicador_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_metodos_pago (
    id_metodo_pago integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    requiere_clabe boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_metodos_pago ALTER COLUMN id_metodo_pago ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_metodos_pago_id_metodo_pago_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_movimientos_imss (
    id_movimiento_imss integer NOT NULL,
    id_empleado integer NOT NULL,
    id_tipo_movimiento_imss integer NOT NULL,
    fecha_movimiento date NOT NULL,
    salario_diario numeric(12,2) NOT NULL,
    salario_diario_integrado numeric(12,2) NOT NULL,
    salario_base_cotizacion numeric(12,2) NOT NULL,
    id_causa_baja_imss integer,
    enviado_idse boolean DEFAULT false NOT NULL,
    fecha_envio_idse timestamp with time zone,
    acuse_idse character varying(100),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_movimientos_sdi CHECK ((salario_diario_integrado >= salario_diario))
);

ALTER TABLE nomina.tb_bi_movimientos_imss ALTER COLUMN id_movimiento_imss ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_movimientos_imss_id_movimiento_imss_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_nomina_cuotas_imss (
    id_nomina_cuota integer NOT NULL,
    id_nomina integer NOT NULL,
    clave_ramo character varying(20) NOT NULL,
    dias_cotizados numeric(5,2) NOT NULL,
    base numeric(12,2) NOT NULL,
    cuota_patron numeric(12,2) DEFAULT 0 NOT NULL,
    cuota_obrero numeric(12,2) DEFAULT 0 NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_nomina_cuotas_imss ALTER COLUMN id_nomina_cuota ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_nomina_cuotas_imss_id_nomina_cuota_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_nomina_detalle (
    id_nomina_detalle integer NOT NULL,
    id_nomina integer NOT NULL,
    id_concepto integer NOT NULL,
    cantidad numeric(10,2) DEFAULT 0 NOT NULL,
    importe_gravado numeric(14,2) DEFAULT 0 NOT NULL,
    importe_exento numeric(14,2) DEFAULT 0 NOT NULL,
    importe_total numeric(14,2) GENERATED ALWAYS AS ((importe_gravado + importe_exento)) STORED,
    id_hist_concepto integer,
    id_credito_infonavit integer,
    id_credito_fonacot integer,
    id_pension integer,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_detalle_importes CHECK (((importe_gravado >= (0)::numeric) AND (importe_exento >= (0)::numeric)))
);

ALTER TABLE nomina.tb_bi_nomina_detalle ALTER COLUMN id_nomina_detalle ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_nomina_detalle_id_nomina_detalle_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_nominas (
    id_nomina integer NOT NULL,
    id_periodo_nomina integer NOT NULL,
    id_empleado integer NOT NULL,
    dias_pagados numeric(6,2) DEFAULT 0 NOT NULL,
    dias_falta numeric(6,2) DEFAULT 0 NOT NULL,
    dias_incapacidad numeric(6,2) DEFAULT 0 NOT NULL,
    salario_diario numeric(12,2) DEFAULT 0 NOT NULL,
    salario_diario_integrado numeric(12,2) DEFAULT 0 NOT NULL,
    salario_base_cotizacion numeric(12,2) DEFAULT 0 NOT NULL,
    total_percepciones numeric(14,2) DEFAULT 0 NOT NULL,
    total_gravado numeric(14,2) DEFAULT 0 NOT NULL,
    total_exento numeric(14,2) DEFAULT 0 NOT NULL,
    total_deducciones numeric(14,2) DEFAULT 0 NOT NULL,
    total_otros_pagos numeric(14,2) DEFAULT 0 NOT NULL,
    isr_causado numeric(12,2) DEFAULT 0 NOT NULL,
    subsidio_causado numeric(12,2) DEFAULT 0 NOT NULL,
    isr_retenido numeric(12,2) DEFAULT 0 NOT NULL,
    imss_obrero numeric(12,2) DEFAULT 0 NOT NULL,
    neto_a_pagar numeric(14,2) DEFAULT 0 NOT NULL,
    imss_patronal numeric(12,2) DEFAULT 0 NOT NULL,
    observaciones character varying(500),
    estatus_recibo character varying(15) DEFAULT 'calculado'::character varying NOT NULL,
    serie character varying(10),
    folio integer,
    uuid uuid,
    fecha_timbrado timestamp with time zone,
    ruta_xml character varying(300),
    ruta_pdf character varying(300),
    uuid_sustituido uuid,
    mensaje_timbrado character varying(500),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_nominas_estatus CHECK (((estatus_recibo)::text = ANY ((ARRAY['calculado'::character varying, 'autorizado'::character varying, 'timbrado'::character varying, 'cancelado'::character varying, 'error_timbrado'::character varying])::text[]))),
    CONSTRAINT ck_nominas_timbrado CHECK ((((estatus_recibo)::text <> 'timbrado'::text) OR (uuid IS NOT NULL)))
);

ALTER TABLE nomina.tb_bi_nominas ALTER COLUMN id_nomina ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_nominas_id_nomina_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_pensiones_alimenticias (
    id_pension integer NOT NULL,
    id_empleado integer NOT NULL,
    beneficiario character varying(200) NOT NULL,
    oficio_juzgado character varying(100),
    id_tipo_descuento integer NOT NULL,
    valor numeric(10,4) NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_fin date,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT tb_bi_pensiones_alimenticias_valor_check CHECK ((valor > (0)::numeric))
);

ALTER TABLE nomina.tb_bi_pensiones_alimenticias ALTER COLUMN id_pension ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_pensiones_alimenticias_id_pension_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_periodicidades_pago (
    id_periodicidad_pago integer NOT NULL,
    clave character varying(15) NOT NULL,
    clave_sat character(2) NOT NULL,
    descripcion character varying(100) NOT NULL,
    dias numeric(5,2) DEFAULT 0 NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_periodicidades_pago ALTER COLUMN id_periodicidad_pago ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_periodicidades_pago_id_periodicidad_pago_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_periodos_nomina (
    id_periodo_nomina integer NOT NULL,
    id_empresa integer NOT NULL,
    id_tipo_nomina integer DEFAULT 1 NOT NULL,
    id_periodicidad_pago integer DEFAULT 3 NOT NULL,
    ejercicio integer NOT NULL,
    numero_periodo integer NOT NULL,
    descripcion character varying(100) NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_fin date NOT NULL,
    fecha_pago date NOT NULL,
    dias_pago numeric(5,2) DEFAULT 15 NOT NULL,
    estatus_cierre character varying(15) DEFAULT 'abierto'::character varying NOT NULL,
    id_autorizador_nomina integer,
    fecha_autorizacion timestamp with time zone,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_periodos_autorizado CHECK ((((estatus_cierre)::text = ANY ((ARRAY['abierto'::character varying, 'calculado'::character varying])::text[])) OR ((id_autorizador_nomina IS NOT NULL) AND (fecha_autorizacion IS NOT NULL)))),
    CONSTRAINT ck_periodos_estatus CHECK (((estatus_cierre)::text = ANY ((ARRAY['abierto'::character varying, 'calculado'::character varying, 'autorizado'::character varying, 'timbrado'::character varying, 'cerrado'::character varying])::text[]))),
    CONSTRAINT ck_periodos_fechas CHECK ((fecha_fin >= fecha_inicio))
);

ALTER TABLE nomina.tb_bi_periodos_nomina ALTER COLUMN id_periodo_nomina ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_periodos_nomina_id_periodo_nomina_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_primas_riesgo_trabajo (
    id_prima_riesgo integer NOT NULL,
    id_registro_patronal integer NOT NULL,
    prima numeric(8,5) NOT NULL,
    fecha_inicio date NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT tb_bi_primas_riesgo_trabajo_prima_check CHECK (((prima >= 0.5) AND (prima <= (15)::numeric)))
);

ALTER TABLE nomina.tb_bi_primas_riesgo_trabajo ALTER COLUMN id_prima_riesgo ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_primas_riesgo_trabajo_id_prima_riesgo_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_ptu (
    id_ptu integer NOT NULL,
    id_empresa integer NOT NULL,
    ejercicio integer NOT NULL,
    renta_gravable numeric(16,2) NOT NULL,
    monto_repartir numeric(16,2) GENERATED ALWAYS AS (round((renta_gravable * 0.10), 2)) STORED,
    tope_salario_confianza numeric(12,2),
    id_periodo_nomina integer,
    estatus character varying(15) DEFAULT 'borrador'::character varying NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_ptu_estatus CHECK (((estatus)::text = ANY ((ARRAY['borrador'::character varying, 'calculado'::character varying, 'pagado'::character varying])::text[])))
);

CREATE TABLE nomina.tb_bi_ptu_empleados (
    id_ptu_empleado integer NOT NULL,
    id_ptu integer NOT NULL,
    id_empleado integer NOT NULL,
    dias_trabajados integer NOT NULL,
    salario_anual numeric(14,2) NOT NULL,
    ptu_por_dias numeric(14,2) NOT NULL,
    ptu_por_salarios numeric(14,2) NOT NULL,
    ptu_calculada numeric(14,2) NOT NULL,
    tope_tres_meses numeric(14,2) NOT NULL,
    promedio_tres_anios numeric(14,2) DEFAULT 0 NOT NULL,
    ptu_final numeric(14,2) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_ptu_empleados ALTER COLUMN id_ptu_empleado ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_ptu_empleados_id_ptu_empleado_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE nomina.tb_bi_ptu ALTER COLUMN id_ptu ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_ptu_id_ptu_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_registros_patronales (
    id_registro_patronal integer NOT NULL,
    id_empresa integer NOT NULL,
    registro_patronal character varying(11) NOT NULL,
    id_clase_riesgo integer NOT NULL,
    id_zona_salario_minimo integer DEFAULT 1 NOT NULL,
    id_estado integer NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_registros_patronales ALTER COLUMN id_registro_patronal ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_registros_patronales_id_registro_patronal_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_subsidio_empleo (
    id_subsidio integer NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_fin date,
    porcentaje_uma numeric(6,3) NOT NULL,
    uma_mensual_base numeric(12,2) NOT NULL,
    ingreso_tope_mensual numeric(12,2) NOT NULL,
    cuota_mensual numeric(12,2) GENERATED ALWAYS AS (round(((uma_mensual_base * porcentaje_uma) / (100)::numeric), 2)) STORED,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_subsidio_empleo ALTER COLUMN id_subsidio ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_subsidio_empleo_id_subsidio_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tarifas_isr (
    id_tarifa_isr integer NOT NULL,
    anio integer NOT NULL,
    periodicidad character varying(15) NOT NULL,
    limite_inferior numeric(14,2) NOT NULL,
    limite_superior numeric(14,2),
    cuota_fija numeric(14,2) NOT NULL,
    porcentaje_excedente numeric(6,2) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_tarifas_periodicidad CHECK (((periodicidad)::text = ANY ((ARRAY['diaria'::character varying, 'semanal'::character varying, 'decenal'::character varying, 'quincenal'::character varying, 'mensual'::character varying, 'anual'::character varying])::text[])))
);

ALTER TABLE nomina.tb_bi_tarifas_isr ALTER COLUMN id_tarifa_isr ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tarifas_isr_id_tarifa_isr_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_concepto (
    id_tipo_concepto integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    catalogo_sat character varying(30),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_tipos_concepto ALTER COLUMN id_tipo_concepto ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_concepto_id_tipo_concepto_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_contrato (
    id_tipo_contrato integer NOT NULL,
    clave character(2) NOT NULL,
    descripcion character varying(150) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_tipos_contrato ALTER COLUMN id_tipo_contrato ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_contrato_id_tipo_contrato_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_descuento (
    id_tipo_descuento integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(150) NOT NULL,
    aplica_infonavit boolean DEFAULT false NOT NULL,
    aplica_pension boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_tipos_descuento ALTER COLUMN id_tipo_descuento ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_descuento_id_tipo_descuento_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_exencion (
    id_tipo_exencion integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(150) NOT NULL,
    porcentaje_exento numeric(6,2) DEFAULT 0 NOT NULL,
    tope_veces_uma numeric(8,2),
    multiplicador character varying(20) DEFAULT 'ninguno'::character varying NOT NULL,
    acumula_anual boolean DEFAULT false NOT NULL,
    fundamento character varying(100),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_tipos_exencion_mult CHECK (((multiplicador)::text = ANY ((ARRAY['ninguno'::character varying, 'anios_servicio'::character varying, 'domingos'::character varying, 'semanas'::character varying])::text[])))
);

ALTER TABLE nomina.tb_bi_tipos_exencion ALTER COLUMN id_tipo_exencion ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_exencion_id_tipo_exencion_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_indicador (
    id_tipo_indicador integer NOT NULL,
    clave character varying(30) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_tipos_indicador ALTER COLUMN id_tipo_indicador ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_indicador_id_tipo_indicador_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_jornada (
    id_tipo_jornada integer NOT NULL,
    clave character(2) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_tipos_jornada ALTER COLUMN id_tipo_jornada ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_jornada_id_tipo_jornada_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_movimiento_imss (
    id_tipo_movimiento_imss integer NOT NULL,
    clave character varying(20) NOT NULL,
    clave_idse character(2) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_tipos_movimiento_imss ALTER COLUMN id_tipo_movimiento_imss ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_movimiento_imss_id_tipo_movimiento_imss_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_nomina (
    id_tipo_nomina integer NOT NULL,
    clave character varying(20) NOT NULL,
    clave_sat character(1) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_tipos_nomina ALTER COLUMN id_tipo_nomina ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_nomina_id_tipo_nomina_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_tipos_regimen (
    id_tipo_regimen integer NOT NULL,
    clave character(2) NOT NULL,
    descripcion character varying(150) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_tipos_regimen ALTER COLUMN id_tipo_regimen ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_tipos_regimen_id_tipo_regimen_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE nomina.tb_bi_zonas_salario_minimo (
    id_zona_salario_minimo integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_tipo_indicador integer NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE nomina.tb_bi_zonas_salario_minimo ALTER COLUMN id_zona_salario_minimo ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME nomina.tb_bi_zonas_salario_minimo_id_zona_salario_minimo_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE VIEW nomina.vw_bi_conceptos_nomina AS
 SELECT c.id_concepto,
    c.clave_concepto,
    c.nombre_concepto,
    c.id_tipo_concepto,
    c.id_clave_sat_concepto,
    c.id_tipo_exencion,
    c.integra_sdi,
    c.es_calculado,
    c.id_usuario_creacion,
    c.fecha_creacion,
    c.id_usuario_modificacion,
    c.fecha_modificacion,
    c.activo,
    tc.clave AS tipo_concepto,
    te.clave AS tipo_exencion,
    cs.clave AS clave_sat,
    te.porcentaje_exento,
    te.tope_veces_uma,
    te.multiplicador,
    te.acumula_anual
   FROM (((nomina.tb_bi_conceptos_nomina c
     JOIN nomina.tb_bi_tipos_concepto tc ON ((tc.id_tipo_concepto = c.id_tipo_concepto)))
     JOIN nomina.tb_bi_tipos_exencion te ON ((te.id_tipo_exencion = c.id_tipo_exencion)))
     LEFT JOIN nomina.tb_bi_claves_sat_concepto cs ON ((cs.id_clave_sat_concepto = c.id_clave_sat_concepto)));

CREATE VIEW nomina.vw_bi_periodos_nomina AS
 SELECT p.id_periodo_nomina,
    p.id_empresa,
    p.id_tipo_nomina,
    p.id_periodicidad_pago,
    p.ejercicio,
    p.numero_periodo,
    p.descripcion,
    p.fecha_inicio,
    p.fecha_fin,
    p.fecha_pago,
    p.dias_pago,
    p.estatus_cierre,
    p.id_autorizador_nomina,
    p.fecha_autorizacion,
    p.id_usuario_creacion,
    p.fecha_creacion,
    p.id_usuario_modificacion,
    p.fecha_modificacion,
    p.activo,
    tn.clave AS tipo_nomina,
    tn.clave_sat AS tipo_nomina_sat,
    pp.clave AS periodicidad,
    pp.clave_sat AS periodicidad_sat
   FROM ((nomina.tb_bi_periodos_nomina p
     JOIN nomina.tb_bi_tipos_nomina tn ON ((tn.id_tipo_nomina = p.id_tipo_nomina)))
     JOIN nomina.tb_bi_periodicidades_pago pp ON ((pp.id_periodicidad_pago = p.id_periodicidad_pago)));

CREATE VIEW nomina.vw_bi_acumulados_anuales AS
 SELECT (EXTRACT(year FROM p.fecha_pago))::integer AS ejercicio,
    p.id_empresa,
    n.id_empleado,
    c.clave_concepto,
    c.nombre_concepto,
    c.tipo_concepto,
    c.clave_sat,
    sum(d.importe_gravado) AS gravado,
    sum(d.importe_exento) AS exento,
    sum(d.importe_total) AS total
   FROM (((nomina.tb_bi_nomina_detalle d
     JOIN nomina.tb_bi_nominas n ON (((n.id_nomina = d.id_nomina) AND ((n.estatus_recibo)::text <> 'cancelado'::text))))
     JOIN nomina.vw_bi_periodos_nomina p ON ((p.id_periodo_nomina = n.id_periodo_nomina)))
     JOIN nomina.vw_bi_conceptos_nomina c ON ((c.id_concepto = d.id_concepto)))
  GROUP BY ((EXTRACT(year FROM p.fecha_pago))::integer), p.id_empresa, n.id_empleado, c.clave_concepto, c.nombre_concepto, c.tipo_concepto, c.clave_sat;

CREATE VIEW nomina.vw_bi_costo_patronal AS
 SELECT n.id_periodo_nomina,
    n.id_empleado,
    n.total_percepciones,
    COALESCE(sum(q.cuota_patron) FILTER (WHERE ((q.clave_ramo)::text <> 'infonavit'::text)), (0)::numeric) AS imss_patronal,
    COALESCE(sum(q.cuota_patron) FILTER (WHERE ((q.clave_ramo)::text = 'infonavit'::text)), (0)::numeric) AS infonavit_patronal,
    (n.total_percepciones + COALESCE(sum(q.cuota_patron), (0)::numeric)) AS costo_total
   FROM (nomina.tb_bi_nominas n
     LEFT JOIN nomina.tb_bi_nomina_cuotas_imss q ON ((q.id_nomina = n.id_nomina)))
  WHERE ((n.estatus_recibo)::text <> 'cancelado'::text)
  GROUP BY n.id_periodo_nomina, n.id_empleado, n.total_percepciones;

CREATE VIEW nomina.vw_bi_movimientos_imss AS
 SELECT m.id_movimiento_imss,
    m.id_empleado,
    m.id_tipo_movimiento_imss,
    m.fecha_movimiento,
    m.salario_diario,
    m.salario_diario_integrado,
    m.salario_base_cotizacion,
    m.id_causa_baja_imss,
    m.enviado_idse,
    m.fecha_envio_idse,
    m.acuse_idse,
    m.id_usuario_creacion,
    m.fecha_creacion,
    m.id_usuario_modificacion,
    m.fecha_modificacion,
    m.activo,
    tm.clave AS tipo_movimiento,
    tm.clave_idse
   FROM (nomina.tb_bi_movimientos_imss m
     JOIN nomina.tb_bi_tipos_movimiento_imss tm ON ((tm.id_tipo_movimiento_imss = m.id_tipo_movimiento_imss)));

CREATE TABLE recursos_humanos.tb_bi_departamentos (
    id_departamento integer NOT NULL,
    nombre_departamento character varying(100) NOT NULL,
    id_responsable integer,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    grupo_directorio character varying(100)
);

CREATE TABLE recursos_humanos.tb_bi_empleados (
    id_empleado integer NOT NULL,
    id_persona integer NOT NULL,
    id_empresa integer NOT NULL,
    id_departamento integer NOT NULL,
    id_perfil integer NOT NULL,
    id_jefe_directo integer,
    numero_checador integer,
    matricula character varying(30),
    fecha_ingreso date NOT NULL,
    id_estatus_laboral integer DEFAULT 1 NOT NULL,
    aviso_privacidad_biometricos boolean DEFAULT false NOT NULL,
    fecha_firma_aviso timestamp with time zone,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    id_puesto integer,
    id_campus integer,
    extension_telefono character varying(10),
    CONSTRAINT ck_empleados_biometricos CHECK (((numero_checador IS NULL) OR aviso_privacidad_biometricos)),
    CONSTRAINT ck_empleados_jefe CHECK (((id_jefe_directo IS NULL) OR (id_jefe_directo <> id_empleado)))
);

CREATE TABLE recursos_humanos.tb_bi_empresas (
    id_empresa integer NOT NULL,
    razon_social character varying(200) NOT NULL,
    nombre_comercial character varying(100) NOT NULL,
    rfc character varying(13) NOT NULL,
    id_regimen_fiscal integer NOT NULL,
    codigo_postal_expedicion character(5) NOT NULL,
    numero_certificado_csd character varying(20),
    ruta_certificado_csd character varying(300),
    vigencia_csd date,
    proveedor_pac character varying(50),
    serie_recibos character varying(10) DEFAULT 'NOM'::character varying,
    folio_siguiente integer DEFAULT 1 NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    dominio_correo character varying(100),
    ou_directorio character varying(300)
);

CREATE TABLE recursos_humanos.tb_bi_personas (
    id_persona integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido_paterno character varying(100) NOT NULL,
    apellido_materno character varying(100),
    curp character(18),
    rfc character varying(13),
    correo_electronico character varying(150),
    telefono character varying(20),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

CREATE VIEW nomina.vw_bi_recibos AS
 SELECT n.id_nomina,
    p.id_periodo_nomina,
    p.id_empresa,
    em.nombre_comercial AS empresa,
    p.tipo_nomina,
    p.descripcion AS periodo,
    p.fecha_pago,
    n.id_empleado,
    e.matricula,
    ((((pe.nombre)::text || ' '::text) || (pe.apellido_paterno)::text) || COALESCE((' '::text || (pe.apellido_materno)::text), ''::text)) AS empleado,
    pe.rfc,
    pe.curp,
    en.nss,
    d.nombre_departamento AS departamento,
    n.dias_pagados,
    n.salario_diario,
    n.salario_diario_integrado,
    n.total_percepciones,
    n.total_gravado,
    n.total_exento,
    n.total_deducciones,
    n.total_otros_pagos,
    n.isr_causado,
    n.subsidio_causado,
    n.isr_retenido,
    n.imss_obrero,
    n.neto_a_pagar,
    n.imss_patronal,
    n.estatus_recibo,
    n.uuid,
    n.observaciones
   FROM ((((((nomina.tb_bi_nominas n
     JOIN nomina.vw_bi_periodos_nomina p ON ((p.id_periodo_nomina = n.id_periodo_nomina)))
     JOIN recursos_humanos.tb_bi_empresas em ON ((em.id_empresa = p.id_empresa)))
     JOIN recursos_humanos.tb_bi_empleados e ON ((e.id_empleado = n.id_empleado)))
     JOIN recursos_humanos.tb_bi_personas pe ON ((pe.id_persona = e.id_persona)))
     JOIN recursos_humanos.tb_bi_departamentos d ON ((d.id_departamento = e.id_departamento)))
     LEFT JOIN nomina.tb_bi_empleados_nomina en ON ((en.id_empleado = n.id_empleado)));

CREATE VIEW nomina.vw_bi_registros_patronales AS
 SELECT rp.id_registro_patronal,
    rp.id_empresa,
    rp.registro_patronal,
    rp.id_clase_riesgo,
    rp.id_zona_salario_minimo,
    rp.id_estado,
    rp.id_usuario_creacion,
    rp.fecha_creacion,
    rp.id_usuario_modificacion,
    rp.fecha_modificacion,
    rp.activo,
    z.clave AS area_salario_minimo,
    cr.clave AS clase_riesgo,
    es.clave AS entidad_federativa
   FROM (((nomina.tb_bi_registros_patronales rp
     JOIN nomina.tb_bi_zonas_salario_minimo z ON ((z.id_zona_salario_minimo = rp.id_zona_salario_minimo)))
     JOIN nomina.tb_bi_clases_riesgo cr ON ((cr.id_clase_riesgo = rp.id_clase_riesgo)))
     JOIN nomina.tb_bi_estados es ON ((es.id_estado = rp.id_estado)));

CREATE TABLE recursos_humanos.tb_bi_asistencia_diaria (
    id_asistencia_diaria bigint NOT NULL,
    id_empleado integer NOT NULL,
    id_historial_horario integer NOT NULL,
    id_solicitud integer,
    fecha_dia date NOT NULL,
    id_estatus_asistencia integer NOT NULL,
    minutos_retardo integer DEFAULT 0 NOT NULL,
    horas_cubiertas numeric(5,2) DEFAULT 0 NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

CREATE TABLE recursos_humanos.tb_bi_estatus_asistencia (
    id_estatus_asistencia integer NOT NULL,
    clave character varying(30) NOT NULL,
    descripcion character varying(100) NOT NULL,
    color_hex character(7),
    es_falta boolean DEFAULT false NOT NULL,
    goce_sueldo boolean DEFAULT true NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

CREATE VIEW recursos_humanos.vw_bi_asistencia_diaria AS
 SELECT d.id_asistencia_diaria,
    d.id_empleado,
    d.id_historial_horario,
    d.id_solicitud,
    d.fecha_dia,
    d.id_estatus_asistencia,
    d.minutos_retardo,
    d.horas_cubiertas,
    d.id_usuario_creacion,
    d.fecha_creacion,
    d.id_usuario_modificacion,
    d.fecha_modificacion,
    d.activo,
    ea.clave AS estatus_asistencia,
    ea.descripcion AS descripcion_estatus,
    ea.color_hex
   FROM (recursos_humanos.tb_bi_asistencia_diaria d
     JOIN recursos_humanos.tb_bi_estatus_asistencia ea ON ((ea.id_estatus_asistencia = d.id_estatus_asistencia)));

CREATE VIEW nomina.vw_bi_resumen_asistencia_periodo AS
 SELECT p.id_periodo_nomina,
    d.id_empleado,
    count(*) AS jornadas_programadas,
    count(*) FILTER (WHERE ((d.estatus_asistencia)::text = 'a_tiempo'::text)) AS a_tiempo,
    count(*) FILTER (WHERE ((d.estatus_asistencia)::text = 'retardo'::text)) AS retardos,
    count(*) FILTER (WHERE ((d.estatus_asistencia)::text = 'falta_injustificada'::text)) AS faltas_injustificadas,
    count(*) FILTER (WHERE ((d.estatus_asistencia)::text = 'falta_justificada'::text)) AS faltas_justificadas,
    count(*) FILTER (WHERE ((d.estatus_asistencia)::text = 'permiso_sin_goce'::text)) AS permisos_sin_goce,
    count(*) FILTER (WHERE ((d.estatus_asistencia)::text = 'vacaciones'::text)) AS vacaciones,
    count(*) FILTER (WHERE ((d.estatus_asistencia)::text = 'incapacidad'::text)) AS incapacidad,
    count(*) FILTER (WHERE ((d.estatus_asistencia)::text = 'suplencia'::text)) AS suplidas,
    COALESCE(sum(d.horas_cubiertas), (0)::numeric) AS horas_cubiertas
   FROM ((recursos_humanos.vw_bi_asistencia_diaria d
     JOIN recursos_humanos.tb_bi_empleados e ON ((e.id_empleado = d.id_empleado)))
     JOIN nomina.vw_bi_periodos_nomina p ON (((p.id_empresa = e.id_empresa) AND ((p.tipo_nomina)::text = 'ordinaria'::text) AND ((d.fecha_dia >= p.fecha_inicio) AND (d.fecha_dia <= p.fecha_fin)))))
  WHERE d.activo
  GROUP BY p.id_periodo_nomina, d.id_empleado;

CREATE TABLE recursos_humanos.tb_bi_acciones_directorio (
    id_accion_directorio integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_acciones_directorio ALTER COLUMN id_accion_directorio ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_acciones_directorio_id_accion_directorio_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE recursos_humanos.tb_bi_asistencia_diaria ALTER COLUMN id_asistencia_diaria ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_asistencia_diaria_id_asistencia_diaria_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_asistencias (
    id_asistencia bigint NOT NULL,
    id_empleado integer NOT NULL,
    id_checador integer,
    fecha_hora_registro timestamp without time zone NOT NULL,
    tipo_registro character varying(10) DEFAULT 'checada'::character varying NOT NULL,
    id_origen_checada integer DEFAULT 1 NOT NULL,
    numero_evento_hikvision bigint,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_asistencias_tipo CHECK (((tipo_registro)::text = ANY ((ARRAY['entrada'::character varying, 'salida'::character varying, 'checada'::character varying])::text[])))
);

ALTER TABLE recursos_humanos.tb_bi_asistencias ALTER COLUMN id_asistencia ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_asistencias_id_asistencia_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_auditoria_logs (
    id_log bigint NOT NULL,
    id_usuario integer NOT NULL,
    tabla_afectada character varying(100) NOT NULL,
    id_registro character varying(50) NOT NULL,
    accion character varying(10) NOT NULL,
    json_datos_previos jsonb,
    json_datos_nuevos jsonb,
    fecha_accion timestamp with time zone DEFAULT now() NOT NULL,
    ip_origen character varying(45),
    CONSTRAINT ck_auditoria_accion CHECK (((accion)::text = ANY ((ARRAY['INSERT'::character varying, 'UPDATE'::character varying, 'DELETE'::character varying])::text[])))
);

ALTER TABLE recursos_humanos.tb_bi_auditoria_logs ALTER COLUMN id_log ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_auditoria_logs_id_log_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_bajas_laborales (
    id_baja integer NOT NULL,
    id_empleado integer NOT NULL,
    id_motivo_baja integer NOT NULL,
    fecha_baja date NOT NULL,
    monto_finiquito_calculado numeric(12,2),
    estatus_pago character varying(20) DEFAULT 'pendiente'::character varying NOT NULL,
    id_nomina_finiquito integer,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_bajas_estatus CHECK (((estatus_pago)::text = ANY ((ARRAY['pendiente'::character varying, 'pagado'::character varying])::text[])))
);

ALTER TABLE recursos_humanos.tb_bi_bajas_laborales ALTER COLUMN id_baja ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_bajas_laborales_id_baja_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_campus (
    id_campus integer NOT NULL,
    id_empresa integer NOT NULL,
    clave character varying(20) NOT NULL,
    nombre_campus character varying(100) NOT NULL,
    calle_numero character varying(200),
    ciudad character varying(100),
    id_estado integer,
    codigo_postal character(5),
    telefono character varying(20),
    ou_directorio character varying(300),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_campus ALTER COLUMN id_campus ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_campus_id_campus_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_causas_baja_imss (
    id_causa_baja_imss integer NOT NULL,
    clave character(1) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_causas_baja_imss ALTER COLUMN id_causa_baja_imss ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_causas_baja_imss_id_causa_baja_imss_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_checadores (
    id_checador integer NOT NULL,
    nombre_checador character varying(100) NOT NULL,
    numero_serie character varying(60) NOT NULL,
    ip_dispositivo character varying(45),
    ubicacion character varying(150),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_checadores ALTER COLUMN id_checador ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_checadores_id_checador_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_cuentas_directorio (
    id_cuenta_directorio integer NOT NULL,
    id_empleado integer NOT NULL,
    sam_account_name character varying(20) NOT NULL,
    user_principal_name character varying(150) NOT NULL,
    correo_institucional character varying(150) NOT NULL,
    object_guid uuid,
    entra_object_id uuid,
    distinguished_name character varying(400),
    cuenta_existente_m365 boolean DEFAULT false NOT NULL,
    hash_sincronizado character(32),
    fecha_ultima_sincronizacion timestamp with time zone,
    ultimo_error character varying(1000),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_cuentas_sam CHECK (((sam_account_name)::text ~ '^[a-z0-9._-]{1,20}$'::text)),
    CONSTRAINT ck_cuentas_upn CHECK (((user_principal_name)::text ~ '^[^@\s]+@[^@\s]+\.[^@\s]+$'::text))
);

ALTER TABLE recursos_humanos.tb_bi_cuentas_directorio ALTER COLUMN id_cuenta_directorio ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_cuentas_directorio_id_cuenta_directorio_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE recursos_humanos.tb_bi_departamentos ALTER COLUMN id_departamento ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_departamentos_id_departamento_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_dias_festivos (
    id_dia_festivo integer NOT NULL,
    fecha date NOT NULL,
    descripcion_festividad character varying(150) NOT NULL,
    es_laborable boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_dias_festivos ALTER COLUMN id_dia_festivo ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_dias_festivos_id_dia_festivo_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE recursos_humanos.tb_bi_empleados ALTER COLUMN id_empleado ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_empleados_id_empleado_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE recursos_humanos.tb_bi_empresas ALTER COLUMN id_empresa ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_empresas_id_empresa_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE recursos_humanos.tb_bi_estatus_asistencia ALTER COLUMN id_estatus_asistencia ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_estatus_asistencia_id_estatus_asistencia_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_estatus_laborales (
    id_estatus_laboral integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_estatus_laborales ALTER COLUMN id_estatus_laboral ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_estatus_laborales_id_estatus_laboral_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_historial_horarios (
    id_historial_horario integer NOT NULL,
    id_empleado integer NOT NULL,
    id_materia integer,
    id_periodo integer,
    id_regla_asistencia integer NOT NULL,
    dia_semana smallint NOT NULL,
    hora_entrada time without time zone NOT NULL,
    hora_salida time without time zone NOT NULL,
    hora_comida_inicio time without time zone,
    hora_comida_fin time without time zone,
    fecha_inicio_vigencia date NOT NULL,
    fecha_fin_vigencia date,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_horarios_comida CHECK ((((hora_comida_inicio IS NULL) = (hora_comida_fin IS NULL)) AND ((hora_comida_fin IS NULL) OR (hora_comida_fin > hora_comida_inicio)))),
    CONSTRAINT ck_horarios_dia CHECK (((dia_semana >= 1) AND (dia_semana <= 7))),
    CONSTRAINT ck_horarios_docente CHECK (((id_materia IS NULL) = (id_periodo IS NULL))),
    CONSTRAINT ck_horarios_horas CHECK ((hora_salida > hora_entrada)),
    CONSTRAINT ck_horarios_vigencia CHECK (((fecha_fin_vigencia IS NULL) OR (fecha_fin_vigencia >= fecha_inicio_vigencia)))
);

ALTER TABLE recursos_humanos.tb_bi_historial_horarios ALTER COLUMN id_historial_horario ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_historial_horarios_id_historial_horario_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_historial_sueldos (
    id_historial_sueldo integer NOT NULL,
    id_empleado integer NOT NULL,
    salario_base numeric(12,2),
    tarifa_por_hora numeric(10,2),
    fecha_inicio_vigencia date NOT NULL,
    fecha_fin_vigencia date,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_sueldos_monto CHECK (((salario_base IS NOT NULL) OR (tarifa_por_hora IS NOT NULL))),
    CONSTRAINT ck_sueldos_vigencia CHECK (((fecha_fin_vigencia IS NULL) OR (fecha_fin_vigencia >= fecha_inicio_vigencia)))
);

ALTER TABLE recursos_humanos.tb_bi_historial_sueldos ALTER COLUMN id_historial_sueldo ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_historial_sueldos_id_historial_sueldo_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_incapacidades_imss (
    id_incapacidad integer NOT NULL,
    id_empleado integer NOT NULL,
    folio_imss character varying(30) NOT NULL,
    dias_autorizados integer NOT NULL,
    id_tipo_incapacidad integer NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_fin date NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_incapacidades_fechas CHECK ((fecha_fin >= fecha_inicio)),
    CONSTRAINT tb_bi_incapacidades_imss_dias_autorizados_check CHECK ((dias_autorizados > 0))
);

ALTER TABLE recursos_humanos.tb_bi_incapacidades_imss ALTER COLUMN id_incapacidad ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_incapacidades_imss_id_incapacidad_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_lft_vacaciones (
    id_lft_vacacion integer NOT NULL,
    anio_antiguedad integer NOT NULL,
    dias_de_ley integer NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT tb_bi_lft_vacaciones_anio_antiguedad_check CHECK ((anio_antiguedad >= 1)),
    CONSTRAINT tb_bi_lft_vacaciones_dias_de_ley_check CHECK ((dias_de_ley > 0))
);

ALTER TABLE recursos_humanos.tb_bi_lft_vacaciones ALTER COLUMN id_lft_vacacion ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_lft_vacaciones_id_lft_vacacion_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_materias (
    id_materia integer NOT NULL,
    nombre_materia character varying(150) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_materias ALTER COLUMN id_materia ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_materias_id_materia_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_modos_checada (
    id_modo_checada integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(150) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_modos_checada ALTER COLUMN id_modo_checada ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_modos_checada_id_modo_checada_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_motivos_baja (
    id_motivo_baja integer NOT NULL,
    descripcion_motivo character varying(100) NOT NULL,
    aplica_finiquito boolean DEFAULT true NOT NULL,
    id_regla_prima_antiguedad integer DEFAULT 1 NOT NULL,
    aplica_indemnizacion_90_dias boolean DEFAULT false NOT NULL,
    aplica_20_dias_por_anio boolean DEFAULT false NOT NULL,
    id_causa_baja_imss integer,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_motivos_baja ALTER COLUMN id_motivo_baja ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_motivos_baja_id_motivo_baja_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_origenes_checada (
    id_origen_checada integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    requiere_checador boolean DEFAULT true NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_origenes_checada ALTER COLUMN id_origen_checada ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_origenes_checada_id_origen_checada_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_perfiles (
    id_perfil integer NOT NULL,
    nombre_perfil character varying(60) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_perfiles ALTER COLUMN id_perfil ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_perfiles_id_perfil_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_periodos_acumulacion (
    id_periodo_acumulacion integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_periodos_acumulacion ALTER COLUMN id_periodo_acumulacion ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_periodos_acumulacion_id_periodo_acumulacion_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_periodos_escolares (
    id_periodo integer NOT NULL,
    nombre_periodo character varying(100) NOT NULL,
    fecha_inicio date,
    fecha_fin date,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_periodos_escolares_fechas CHECK (((fecha_fin IS NULL) OR (fecha_fin >= fecha_inicio)))
);

ALTER TABLE recursos_humanos.tb_bi_periodos_escolares ALTER COLUMN id_periodo ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_periodos_escolares_id_periodo_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_permisos (
    id_permiso integer NOT NULL,
    clave character varying(50) NOT NULL,
    modulo character varying(50) NOT NULL,
    descripcion character varying(200) NOT NULL,
    usa_alcance boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_permisos ALTER COLUMN id_permiso ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_permisos_id_permiso_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE recursos_humanos.tb_bi_personas ALTER COLUMN id_persona ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_personas_id_persona_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_proveedores_autenticacion (
    id_proveedor_autenticacion integer CONSTRAINT tb_bi_proveedores_autentica_id_proveedor_autenticacion_not_null NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    requiere_password boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_proveedores_autenticacion ALTER COLUMN id_proveedor_autenticacion ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_proveedores_autenticacion_id_proveedor_autenticacion_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_puestos (
    id_puesto integer NOT NULL,
    clave character varying(20) NOT NULL,
    nombre_puesto character varying(100) NOT NULL,
    descripcion character varying(300),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_puestos ALTER COLUMN id_puesto ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_puestos_id_puesto_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_regimenes_fiscales (
    id_regimen_fiscal integer NOT NULL,
    clave character(3) NOT NULL,
    descripcion character varying(200) NOT NULL,
    persona_fisica boolean DEFAULT false NOT NULL,
    persona_moral boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_regimenes_fiscales ALTER COLUMN id_regimen_fiscal ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_regimenes_fiscales_id_regimen_fiscal_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_reglas_asistencia (
    id_regla_asistencia integer NOT NULL,
    nombre_regla character varying(100) NOT NULL,
    minutos_tolerancia integer DEFAULT 10 NOT NULL,
    minutos_limite_falta integer DEFAULT 15 NOT NULL,
    maximo_retardos_permitidos integer DEFAULT 3 NOT NULL,
    aplica_sancion_falta boolean DEFAULT true NOT NULL,
    id_periodo_acumulacion integer DEFAULT 1 NOT NULL,
    id_modo_checada integer DEFAULT 1 NOT NULL,
    fecha_inicio_vigencia date NOT NULL,
    fecha_fin_vigencia date,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_reglas_minutos CHECK (((minutos_tolerancia >= 0) AND (minutos_limite_falta >= minutos_tolerancia))),
    CONSTRAINT ck_reglas_retardos CHECK ((maximo_retardos_permitidos > 0)),
    CONSTRAINT ck_reglas_vigencia CHECK (((fecha_fin_vigencia IS NULL) OR (fecha_fin_vigencia >= fecha_inicio_vigencia)))
);

ALTER TABLE recursos_humanos.tb_bi_reglas_asistencia ALTER COLUMN id_regla_asistencia ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_reglas_asistencia_id_regla_asistencia_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_reglas_prima_antiguedad (
    id_regla_prima_antiguedad integer CONSTRAINT tb_bi_reglas_prima_antigueda_id_regla_prima_antiguedad_not_null NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(150) NOT NULL,
    anios_minimos integer DEFAULT 0 NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_reglas_prima_antiguedad ALTER COLUMN id_regla_prima_antiguedad ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_reglas_prima_antiguedad_id_regla_prima_antiguedad_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_roles (
    id_rol integer NOT NULL,
    nombre_rol character varying(60) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    clave character varying(30) NOT NULL
);

CREATE TABLE recursos_humanos.tb_bi_roles_grupos_directorio (
    id_rol_grupo_directorio integer NOT NULL,
    id_rol integer NOT NULL,
    nombre_grupo character varying(100) NOT NULL,
    object_guid_grupo uuid,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_roles_grupos_directorio ALTER COLUMN id_rol_grupo_directorio ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_roles_grupos_directorio_id_rol_grupo_directorio_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE recursos_humanos.tb_bi_roles ALTER COLUMN id_rol ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_roles_id_rol_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_roles_permisos (
    id_rol_permiso integer NOT NULL,
    id_rol integer NOT NULL,
    id_permiso integer NOT NULL,
    alcance character varying(10),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_roles_permisos_alcance CHECK (((alcance IS NULL) OR ((alcance)::text = ANY ((ARRAY['todos'::character varying, 'equipo'::character varying, 'propio'::character varying])::text[]))))
);

ALTER TABLE recursos_humanos.tb_bi_roles_permisos ALTER COLUMN id_rol_permiso ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_roles_permisos_id_rol_permiso_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_saldos_vacaciones (
    id_saldo_vacacion integer NOT NULL,
    id_empleado integer NOT NULL,
    id_lft_vacacion integer NOT NULL,
    anio_servicio integer NOT NULL,
    dias_otorgados integer NOT NULL,
    dias_excepcion integer DEFAULT 0 NOT NULL,
    dias_disfrutados integer DEFAULT 0 NOT NULL,
    fecha_caducidad date NOT NULL,
    prima_pagada boolean DEFAULT false NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_saldos_disfrutados CHECK (((dias_disfrutados >= 0) AND (dias_disfrutados <= (dias_otorgados + dias_excepcion)))),
    CONSTRAINT tb_bi_saldos_vacaciones_anio_servicio_check CHECK ((anio_servicio >= 1))
);

ALTER TABLE recursos_humanos.tb_bi_saldos_vacaciones ALTER COLUMN id_saldo_vacacion ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_saldos_vacaciones_id_saldo_vacacion_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_sincronizacion_directorio_logs (
    id_sincronizacion_log bigint CONSTRAINT tb_bi_sincronizacion_directorio__id_sincronizacion_log_not_null NOT NULL,
    id_empleado integer NOT NULL,
    id_accion_directorio integer CONSTRAINT tb_bi_sincronizacion_directorio_l_id_accion_directorio_not_null NOT NULL,
    exitoso boolean NOT NULL,
    atributos_enviados jsonb,
    mensaje character varying(1000),
    fecha_sincronizacion timestamp with time zone DEFAULT now() CONSTRAINT tb_bi_sincronizacion_directorio_l_fecha_sincronizacion_not_null NOT NULL,
    id_usuario_creacion integer DEFAULT 1 CONSTRAINT tb_bi_sincronizacion_directorio_lo_id_usuario_creacion_not_null NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_sincronizacion_directorio_logs ALTER COLUMN id_sincronizacion_log ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_sincronizacion_directorio_logs_id_sincronizacion_log_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_solicitudes (
    id_solicitud integer NOT NULL,
    id_empleado integer NOT NULL,
    id_tipo_permiso integer NOT NULL,
    id_asistencia_omision bigint,
    fecha_inicio date NOT NULL,
    fecha_fin date NOT NULL,
    hora_inicio_permiso time without time zone,
    hora_fin_permiso time without time zone,
    dias_solicitados numeric(5,1),
    horas_solicitadas numeric(5,2),
    estatus_aprobacion character varying(20) DEFAULT 'pendiente'::character varying NOT NULL,
    motivo_rechazo character varying(500),
    fecha_aprobacion_jefe timestamp with time zone,
    fecha_aprobacion_rh timestamp with time zone,
    id_autorizador_jefe integer,
    id_autorizador_rh integer,
    ruta_comprobante_adjunto character varying(500),
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_solicitudes_estatus CHECK (((estatus_aprobacion)::text = ANY ((ARRAY['pendiente'::character varying, 'aprobada_jefe'::character varying, 'aprobada'::character varying, 'rechazada'::character varying, 'cancelada'::character varying])::text[]))),
    CONSTRAINT ck_solicitudes_fechas CHECK ((fecha_fin >= fecha_inicio)),
    CONSTRAINT ck_solicitudes_horas CHECK ((((hora_inicio_permiso IS NULL) = (hora_fin_permiso IS NULL)) AND ((hora_fin_permiso IS NULL) OR (hora_fin_permiso > hora_inicio_permiso)))),
    CONSTRAINT ck_solicitudes_rechazo CHECK ((((estatus_aprobacion)::text <> 'rechazada'::text) OR (motivo_rechazo IS NOT NULL)))
);

ALTER TABLE recursos_humanos.tb_bi_solicitudes ALTER COLUMN id_solicitud ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_solicitudes_id_solicitud_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_suplencias (
    id_suplencia integer NOT NULL,
    id_empleado_suplente integer NOT NULL,
    id_empleado_titular integer NOT NULL,
    id_historial_horario integer NOT NULL,
    id_autorizador integer,
    fecha_suplencia date NOT NULL,
    estatus_aprobacion character varying(20) DEFAULT 'pendiente'::character varying NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    CONSTRAINT ck_suplencias_estatus CHECK (((estatus_aprobacion)::text = ANY ((ARRAY['pendiente'::character varying, 'aprobada'::character varying, 'rechazada'::character varying, 'cancelada'::character varying])::text[]))),
    CONSTRAINT ck_suplencias_personas CHECK ((id_empleado_suplente <> id_empleado_titular))
);

ALTER TABLE recursos_humanos.tb_bi_suplencias ALTER COLUMN id_suplencia ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_suplencias_id_suplencia_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_tipos_duracion_permiso (
    id_tipo_duracion_permiso integer NOT NULL,
    clave character varying(20) NOT NULL,
    descripcion character varying(100) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_tipos_duracion_permiso ALTER COLUMN id_tipo_duracion_permiso ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_tipos_duracion_permiso_id_tipo_duracion_permiso_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_tipos_incapacidad (
    id_tipo_incapacidad integer NOT NULL,
    clave character(2) NOT NULL,
    descripcion character varying(150) NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL
);

ALTER TABLE recursos_humanos.tb_bi_tipos_incapacidad ALTER COLUMN id_tipo_incapacidad ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_tipos_incapacidad_id_tipo_incapacidad_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_tipos_permiso (
    id_tipo_permiso integer NOT NULL,
    descripcion character varying(100) NOT NULL,
    requiere_comprobante boolean DEFAULT false NOT NULL,
    es_con_goce boolean DEFAULT true NOT NULL,
    descuenta_saldo boolean DEFAULT false NOT NULL,
    id_tipo_duracion_permiso integer DEFAULT 1 NOT NULL,
    id_estatus_asistencia integer DEFAULT 4 NOT NULL,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    dias_por_anio integer,
    CONSTRAINT ck_tipos_permiso_dias_por_anio CHECK (((dias_por_anio IS NULL) OR (dias_por_anio > 0)))
);

ALTER TABLE recursos_humanos.tb_bi_tipos_permiso ALTER COLUMN id_tipo_permiso ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_tipos_permiso_id_tipo_permiso_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE recursos_humanos.tb_bi_usuarios (
    id_usuario integer NOT NULL,
    id_empleado integer,
    id_rol integer NOT NULL,
    nombre_usuario character varying(100) NOT NULL,
    password_hash character varying(255),
    estatus_usuario character varying(20) DEFAULT 'activo'::character varying NOT NULL,
    id_proveedor_autenticacion integer DEFAULT 1 NOT NULL,
    id_externo character varying(100),
    ultimo_acceso timestamp with time zone,
    id_usuario_creacion integer DEFAULT 1 NOT NULL,
    fecha_creacion timestamp with time zone DEFAULT now() NOT NULL,
    id_usuario_modificacion integer,
    fecha_modificacion timestamp with time zone,
    activo boolean DEFAULT true NOT NULL,
    intentos_fallidos integer DEFAULT 0 NOT NULL,
    bloqueado_hasta timestamp with time zone,
    debe_cambiar_contrasena boolean DEFAULT false NOT NULL,
    fecha_cambio_contrasena timestamp with time zone,
    CONSTRAINT ck_usuarios_estatus CHECK (((estatus_usuario)::text = ANY ((ARRAY['activo'::character varying, 'bloqueado'::character varying, 'inactivo'::character varying])::text[]))),
    CONSTRAINT ck_usuarios_password CHECK (((id_proveedor_autenticacion <> 1) OR (password_hash IS NOT NULL)))
);

ALTER TABLE recursos_humanos.tb_bi_usuarios ALTER COLUMN id_usuario ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME recursos_humanos.tb_bi_usuarios_id_usuario_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE VIEW recursos_humanos.vw_bi_directorio_empleados AS
 WITH datos AS (
         SELECT e.id_empleado,
            c.sam_account_name AS "sAMAccountName",
            c.user_principal_name AS "userPrincipalName",
            c.correo_institucional AS mail,
            p.nombre AS "givenName",
            TRIM(BOTH FROM (((p.apellido_paterno)::text || ' '::text) || (COALESCE(p.apellido_materno, ''::character varying))::text)) AS sn,
            TRIM(BOTH FROM (((((p.nombre)::text || ' '::text) || (p.apellido_paterno)::text) || ' '::text) || (COALESCE(p.apellido_materno, ''::character varying))::text)) AS "displayName",
            pu.nombre_puesto AS title,
            d_1.nombre_departamento AS department,
            em.nombre_comercial AS company,
            pl.nombre_campus AS "physicalDeliveryOfficeName",
            pl.calle_numero AS "streetAddress",
            pl.ciudad AS l,
            es.descripcion AS st,
            pl.codigo_postal AS "postalCode",
            e.matricula AS "employeeID",
                CASE
                    WHEN (e.extension_telefono IS NOT NULL) THEN (COALESCE(((pl.telefono)::text || ' ext. '::text), 'Ext. '::text) || (e.extension_telefono)::text)
                    ELSE NULL::text
                END AS "telephoneNumber",
            p.telefono AS mobile,
            cj.sam_account_name AS manager,
            COALESCE(pl.ou_directorio, em.ou_directorio) AS ou_destino,
            (((el.clave)::text = 'activo'::text) AND e.activo) AS habilitada,
            c.cuenta_existente_m365,
            c.object_guid,
            c.hash_sincronizado
           FROM (((((((((recursos_humanos.tb_bi_cuentas_directorio c
             JOIN recursos_humanos.tb_bi_empleados e ON ((e.id_empleado = c.id_empleado)))
             JOIN recursos_humanos.tb_bi_personas p ON ((p.id_persona = e.id_persona)))
             JOIN recursos_humanos.tb_bi_empresas em ON ((em.id_empresa = e.id_empresa)))
             JOIN recursos_humanos.tb_bi_departamentos d_1 ON ((d_1.id_departamento = e.id_departamento)))
             JOIN recursos_humanos.tb_bi_estatus_laborales el ON ((el.id_estatus_laboral = e.id_estatus_laboral)))
             LEFT JOIN recursos_humanos.tb_bi_puestos pu ON ((pu.id_puesto = e.id_puesto)))
             LEFT JOIN recursos_humanos.tb_bi_campus pl ON ((pl.id_campus = e.id_campus)))
             LEFT JOIN nomina.tb_bi_estados es ON ((es.id_estado = pl.id_estado)))
             LEFT JOIN recursos_humanos.tb_bi_cuentas_directorio cj ON ((cj.id_empleado = e.id_jefe_directo)))
          WHERE c.activo
        )
 SELECT id_empleado,
    "sAMAccountName",
    "userPrincipalName",
    mail,
    "givenName",
    sn,
    "displayName",
    title,
    department,
    company,
    "physicalDeliveryOfficeName",
    "streetAddress",
    l,
    st,
    "postalCode",
    "employeeID",
    "telephoneNumber",
    mobile,
    manager,
    ou_destino,
    habilitada,
    cuenta_existente_m365,
    object_guid,
    hash_sincronizado,
    (md5(concat_ws('|'::text, "sAMAccountName", "userPrincipalName", mail, "givenName", sn, "displayName", title, department, company, "physicalDeliveryOfficeName", "streetAddress", l, st, "postalCode", "employeeID", "telephoneNumber", mobile, manager, ou_destino, (habilitada)::text)))::character(32) AS hash_datos,
    (hash_sincronizado IS DISTINCT FROM (md5(concat_ws('|'::text, "sAMAccountName", "userPrincipalName", mail, "givenName", sn, "displayName", title, department, company, "physicalDeliveryOfficeName", "streetAddress", l, st, "postalCode", "employeeID", "telephoneNumber", mobile, manager, ou_destino, (habilitada)::text)))::character(32)) AS requiere_sincronizacion
   FROM datos d;

CREATE VIEW recursos_humanos.vw_bi_directorio_pendientes AS
 SELECT e.id_empleado,
    (((p.nombre)::text || ' '::text) || (p.apellido_paterno)::text) AS empleado,
    em.nombre_comercial AS empresa,
    concat_ws(', '::text,
        CASE
            WHEN (em.dominio_correo IS NULL) THEN 'la empresa no tiene dominio de correo'::text
            ELSE NULL::text
        END,
        CASE
            WHEN (COALESCE(ca.ou_directorio, em.ou_directorio) IS NULL) THEN 'sin OU (campus/empresa)'::text
            ELSE NULL::text
        END,
        CASE
            WHEN (e.id_puesto IS NULL) THEN 'sin puesto'::text
            ELSE NULL::text
        END,
        CASE
            WHEN (e.id_campus IS NULL) THEN 'sin campus'::text
            ELSE NULL::text
        END,
        CASE
            WHEN (e.id_jefe_directo IS NULL) THEN 'sin jefe directo'::text
            ELSE NULL::text
        END,
        CASE
            WHEN (c.id_cuenta_directorio IS NULL) THEN 'sin cuenta de directorio'::text
            ELSE NULL::text
        END,
        CASE
            WHEN (c.ultimo_error IS NOT NULL) THEN ('último error: '::text || (c.ultimo_error)::text)
            ELSE NULL::text
        END) AS faltantes
   FROM (((((recursos_humanos.tb_bi_empleados e
     JOIN recursos_humanos.tb_bi_personas p ON ((p.id_persona = e.id_persona)))
     JOIN recursos_humanos.tb_bi_empresas em ON ((em.id_empresa = e.id_empresa)))
     JOIN recursos_humanos.tb_bi_estatus_laborales el ON ((el.id_estatus_laboral = e.id_estatus_laboral)))
     LEFT JOIN recursos_humanos.tb_bi_campus ca ON ((ca.id_campus = e.id_campus)))
     LEFT JOIN recursos_humanos.tb_bi_cuentas_directorio c ON ((c.id_empleado = e.id_empleado)))
  WHERE (((el.clave)::text = 'activo'::text) AND e.activo AND ((em.dominio_correo IS NULL) OR (COALESCE(ca.ou_directorio, em.ou_directorio) IS NULL) OR (e.id_puesto IS NULL) OR (e.id_campus IS NULL) OR (e.id_jefe_directo IS NULL) OR (c.id_cuenta_directorio IS NULL) OR (c.ultimo_error IS NOT NULL)));

CREATE VIEW recursos_humanos.vw_bi_vacaciones_disponibles AS
 SELECT id_empleado,
    sum(((dias_otorgados + dias_excepcion) - dias_disfrutados)) AS dias_disponibles,
    min(fecha_caducidad) AS proxima_caducidad
   FROM recursos_humanos.tb_bi_saldos_vacaciones s
  WHERE (activo AND (fecha_caducidad >= CURRENT_DATE))
  GROUP BY id_empleado;

INSERT INTO nomina.tb_bi_bancos OVERRIDING SYSTEM VALUE VALUES
	(1, '002', 'BANAMEX', 'Banco Nacional de México', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, '006', 'BANCOMEXT', 'Banco Nacional de Comercio Exterior', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, '009', 'BANOBRAS', 'Banco Nacional de Obras y Servicios Públicos', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, '012', 'BBVA MEXICO', 'BBVA México', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, '014', 'SANTANDER', 'Banco Santander (México)', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, '019', 'BANJERCITO', 'Banco Nacional del Ejército, Fuerza Aérea y Armada', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, '021', 'HSBC', 'HSBC México', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, '030', 'BAJIO', 'Banco del Bajío', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, '036', 'INBURSA', 'Banco Inbursa', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, '042', 'MIFEL', 'Banca Mifel', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(11, '044', 'SCOTIABANK', 'Scotiabank Inverlat', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(12, '058', 'BANREGIO', 'Banco Regional de Monterrey', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(13, '059', 'INVEX', 'Banco Invex', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(14, '060', 'BANSI', 'Bansi', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(15, '062', 'AFIRME', 'Banca Afirme', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(16, '072', 'BANORTE', 'Banco Mercantil del Norte', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(17, '106', 'BANK OF AMERICA', 'Bank of America México', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(18, '110', 'JP MORGAN', 'Banco J.P. Morgan', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(19, '112', 'BMONEX', 'Banco Monex', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(20, '113', 'VE POR MAS', 'Banco Ve Por Más', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(21, '127', 'AZTECA', 'Banco Azteca', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(22, '128', 'AUTOFIN', 'Banco Autofin México', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(23, '130', 'COMPARTAMOS', 'Banco Compartamos', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(24, '132', 'MULTIVA', 'Banco Multiva', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(25, '133', 'ACTINVER', 'Banco Actinver', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(26, '136', 'INTERCAM BANCO', 'Intercam Banco', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(27, '137', 'BANCOPPEL', 'BanCoppel', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(28, '138', 'ABC CAPITAL', 'ABC Capital', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(29, '140', 'CONSUBANCO', 'Consubanco', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(30, '143', 'CIBANCO', 'CIBanco', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(31, '145', 'BBASE', 'Banco Base', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(32, '166', 'BANCO DEL BIENESTAR', 'Banco del Bienestar (antes BANSEFI)', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(33, '646', 'STP', 'Sistema de Transferencias y Pagos STP', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_cesantia_vejez_patron OVERRIDING SYSTEM VALUE VALUES
	(1, 2026, 0.00, 3.1500, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 2027, 0.00, 3.1500, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 2028, 0.00, 3.1500, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 2029, 0.00, 3.1500, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 2030, 0.00, 3.1500, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, 2026, 1.01, 3.6760, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, 2027, 1.01, 3.8070, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, 2028, 1.01, 3.9390, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, 2029, 1.01, 4.0700, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, 2030, 1.01, 4.2020, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(11, 2026, 1.51, 4.8510, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(12, 2027, 1.51, 5.2760, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(13, 2028, 1.51, 5.7010, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(14, 2029, 1.51, 6.1260, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(15, 2030, 1.51, 6.5520, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(16, 2026, 2.01, 5.5560, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(17, 2027, 2.01, 6.1570, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(18, 2028, 2.01, 6.7590, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(19, 2029, 2.01, 7.3600, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(20, 2030, 2.01, 7.9620, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(21, 2026, 2.51, 6.0260, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(22, 2027, 2.51, 6.7450, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(23, 2028, 2.51, 7.4640, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(24, 2029, 2.51, 8.1830, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(25, 2030, 2.51, 8.9020, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(26, 2026, 3.01, 6.3610, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(27, 2027, 3.01, 7.1640, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(28, 2028, 3.01, 7.9670, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(29, 2029, 3.01, 8.7700, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(30, 2030, 3.01, 9.5730, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(31, 2026, 3.51, 6.6130, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(32, 2027, 3.51, 7.4790, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(33, 2028, 3.51, 8.3450, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(34, 2029, 3.51, 9.2110, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(35, 2030, 3.51, 10.0770, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(36, 2026, 4.01, 7.5130, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(37, 2027, 4.01, 8.6030, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(38, 2028, 4.01, 9.6940, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(39, 2029, 4.01, 10.7840, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(40, 2030, 4.01, 11.8750, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_clases_riesgo OVERRIDING SYSTEM VALUE VALUES
	(1, '1', 'Clase I', 0.54355, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, '2', 'Clase II', 1.13065, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, '3', 'Clase III', 2.59840, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, '4', 'Clase IV', 4.65325, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, '5', 'Clase V', 7.58875, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, '99', 'No aplica', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_claves_sat_concepto OVERRIDING SYSTEM VALUE VALUES
	(1, 1, '049', 'Premios por asistencia', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 1, '038', 'Otros ingresos por salarios', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 1, '025', 'Indemnizaciones', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 1, '023', 'Pagos por separación', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 1, '022', 'Prima por antigüedad', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, 1, '021', 'Prima vacacional', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, 1, '020', 'Prima dominical', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, 1, '019', 'Horas extra', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, 1, '010', 'Premios por puntualidad', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, 1, '003', 'Participación de los Trabajadores en las Utilidades PTU', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(11, 1, '002', 'Gratificación Anual (Aguinaldo)', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(12, 1, '001', 'Sueldos, Salarios Rayas y Jornales', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(13, 2, '020', 'Ausencia (Ausentismo)', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(14, 2, '012', 'Anticipo de salarios', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(15, 2, '011', 'Pago de abonos INFONACOT', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(16, 2, '010', 'Pago por crédito de vivienda', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(17, 2, '007', 'Pensión alimenticia', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(18, 2, '006', 'Descuento por incapacidad', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(19, 2, '004', 'Otros', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(20, 2, '002', 'ISR', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(21, 2, '001', 'Seguridad social', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(22, 3, '002', 'Subsidio para el empleo (efectivamente entregado al trabajador)', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_conceptos_hist OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 4, 3000.00, 6, 1, 2500.00, '2026-10-01', 1, '2026-10-23 20:58:02.068688-06', 1, '2026-10-23 20:58:02.068688-06', true);

INSERT INTO nomina.tb_bi_conceptos_nomina OVERRIDING SYSTEM VALUE VALUES
	(1, 'D001', 'IMSS (cuota obrera)', 2, 21, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'D002', 'ISR', 2, 20, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'D003', 'Ausentismo (faltas por retardos)', 2, 13, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'D004', 'Préstamo', 2, 19, 1, false, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 'D005', 'Colegiatura de posgrado', 2, 19, 1, false, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, 'D006', 'Crédito INFONAVIT', 2, 16, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, 'D007', 'Crédito FONACOT', 2, 15, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, 'D008', 'Pensión alimenticia', 2, 17, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, 'D009', 'Anticipo de salario', 2, 14, 1, false, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, 'D010', 'Otros descuentos', 2, 19, 1, false, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(11, 'I001', 'Días de incapacidad', 4, NULL, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(12, 'I002', 'Días de falta y permisos sin goce', 4, NULL, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(13, 'I003', 'Faltas por retardos acumulados', 4, NULL, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(14, 'I004', 'Subsidio para el empleo aplicado', 4, NULL, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(15, 'P001', 'Sueldo', 1, 12, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(16, 'P002', 'Horas clase', 1, 12, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(17, 'P003', 'Horas clase por suplencia', 1, 12, 1, true, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(18, 'P004', 'Prima vacacional', 1, 6, 4, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(19, 'P005', 'Aguinaldo', 1, 11, 3, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(20, 'P006', 'Prima dominical', 1, 7, 6, true, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(21, 'P007', 'Horas extra dobles', 1, 8, 7, true, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(22, 'P008', 'Horas extra triples', 1, 8, 1, true, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(23, 'P009', 'PTU', 1, 10, 5, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(24, 'P010', 'Vacaciones pagadas', 1, 12, 1, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(25, 'P011', 'Prima de antigüedad', 1, 5, 8, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(26, 'P012', 'Indemnización', 1, 3, 8, false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(27, 'P013', 'Premio de puntualidad', 1, 9, 1, true, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(28, 'P014', 'Premio de asistencia', 1, 1, 1, true, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(29, 'P015', 'Bono / gratificación', 1, 2, 1, true, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_creditos_fonacot OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 'F-1', 6000.00, 1000.00, 5500.00, '2026-01-01', 1, '2026-10-23 20:58:02.068688-06', 1, '2026-10-23 20:58:02.068688-06', true);

INSERT INTO nomina.tb_bi_creditos_infonavit OVERRIDING SYSTEM VALUE VALUES
	(1, 1, '1234567890', 3, 20.0000, true, '2026-01-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_cuotas_imss OVERRIDING SYSTEM VALUE VALUES
	(1, 'rt', 'Riesgos de trabajo', NULL, 0.0000, 'sbc', 'otros', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'eym_cuota_fija', 'Enfermedades y maternidad - cuota fija', 20.4000, 0.0000, 'uma', 'eym', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'eym_excedente', 'Enfermedades y maternidad - excedente 3 UMA', 1.1000, 0.4000, 'excedente_3_uma', 'eym', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'eym_dinero', 'Enfermedades y maternidad - prest. en dinero', 0.7000, 0.2500, 'sbc', 'eym', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 'eym_gmp', 'Gastos médicos de pensionados', 1.0500, 0.3750, 'sbc', 'eym', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, 'iv', 'Invalidez y vida', 1.7500, 0.6250, 'sbc', 'otros', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, 'guarderias', 'Guarderías y prestaciones sociales', 1.0000, 0.0000, 'sbc', 'otros', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, 'retiro', 'Retiro', 2.0000, 0.0000, 'sbc', 'retiro', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, 'cv', 'Cesantía en edad avanzada y vejez', NULL, 1.1250, 'sbc', 'otros', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, 'infonavit', 'INFONAVIT', 5.0000, 0.0000, 'sbc', 'retiro', '2023-01-01', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_empleados_nomina OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 1, 1, '12345678901', NULL, 3, 1, 1, 1, 1, false, false, 1, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 2, 1, 1, '12345678902', NULL, 3, 1, 1, 1, 1, false, false, 1, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 3, 1, 1, '12345678903', NULL, 3, 1, 1, 1, 1, false, false, 1, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_esquemas_prestaciones OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 'Ley', 15, 25.00, 25.00, false, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_estados OVERRIDING SYSTEM VALUE VALUES
	(1, 'AGU', 'Aguascalientes', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'BCN', 'Baja California', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'BCS', 'Baja California Sur', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'CAM', 'Campeche', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 'CHH', 'Chihuahua', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, 'CHP', 'Chiapas', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, 'CMX', 'Ciudad de México', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, 'COA', 'Coahuila', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, 'COL', 'Colima', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, 'DUR', 'Durango', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(11, 'GRO', 'Guerrero', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(12, 'GUA', 'Guanajuato', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(13, 'HID', 'Hidalgo', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(14, 'JAL', 'Jalisco', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(15, 'MEX', 'Estado de México', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(16, 'MIC', 'Michoacán', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(17, 'MOR', 'Morelos', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(18, 'NAY', 'Nayarit', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(19, 'NLE', 'Nuevo León', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(20, 'OAX', 'Oaxaca', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(21, 'PUE', 'Puebla', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(22, 'QUE', 'Querétaro', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(23, 'ROO', 'Quintana Roo', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(24, 'SIN', 'Sinaloa', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(25, 'SLP', 'San Luis Potosí', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(26, 'SON', 'Sonora', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(27, 'TAB', 'Tabasco', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(28, 'TAM', 'Tamaulipas', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(29, 'TLA', 'Tlaxcala', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(30, 'VER', 'Veracruz', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(31, 'YUC', 'Yucatán', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(32, 'ZAC', 'Zacatecas', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_indicadores OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 117.3100, '2026-02-01', NULL, 'INEGI, comunicado 1/26', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 1, 113.1400, '2025-02-01', '2026-01-31', 'INEGI, DOF 10-ene-2025', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 2, 315.0400, '2026-01-01', NULL, 'CONASAMI, DOF dic-2025', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 2, 278.8000, '2025-01-01', '2025-12-31', 'CONASAMI 2025', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 3, 440.8700, '2026-01-01', NULL, 'CONASAMI, DOF dic-2025', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, 3, 419.8800, '2025-01-01', '2025-12-31', 'CONASAMI 2025', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, 4, 100.8100, '2024-01-01', NULL, 'INFONAVIT (sin cambio 2025-2026)', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_metodos_pago OVERRIDING SYSTEM VALUE VALUES
	(1, 'transferencia', 'Transferencia bancaria', true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'efectivo', 'Efectivo', false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'cheque', 'Cheque', false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_movimientos_imss OVERRIDING SYSTEM VALUE VALUES
	(1, 2, 1, '2024-01-08', 315.04, 332.73, 332.73, NULL, false, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 3, 1, '2025-02-01', 315.04, 332.73, 332.73, NULL, false, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_nomina_cuotas_imss OVERRIDING SYSTEM VALUE VALUES
	(29, 4, 'rt', 13.00, 528.10, 37.32, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(30, 4, 'eym_cuota_fija', 14.00, 117.31, 335.04, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(31, 4, 'eym_excedente', 14.00, 176.17, 27.13, 9.87, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(32, 4, 'eym_dinero', 14.00, 528.10, 51.75, 18.48, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(33, 4, 'eym_gmp', 14.00, 528.10, 77.63, 27.73, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(34, 4, 'iv', 13.00, 528.10, 120.14, 42.91, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(35, 4, 'guarderias', 13.00, 528.10, 68.65, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(36, 4, 'retiro', 14.00, 528.10, 147.87, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(37, 4, 'cv', 13.00, 528.10, 515.79, 77.23, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(38, 4, 'infonavit', 14.00, 528.10, 369.67, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(39, 5, 'rt', 15.00, 332.73, 27.13, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(40, 5, 'eym_cuota_fija', 15.00, 117.31, 358.97, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(41, 5, 'eym_dinero', 15.00, 332.73, 47.42, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(42, 5, 'eym_gmp', 15.00, 332.73, 71.12, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(43, 5, 'iv', 15.00, 332.73, 118.53, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(44, 5, 'guarderias', 15.00, 332.73, 49.91, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(45, 5, 'retiro', 15.00, 332.73, 99.82, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(46, 5, 'cv', 15.00, 332.73, 356.90, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(47, 5, 'infonavit', 15.00, 332.73, 249.55, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(48, 6, 'rt', 15.00, 332.73, 27.13, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(49, 6, 'eym_cuota_fija', 15.00, 117.31, 358.97, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(50, 6, 'eym_dinero', 15.00, 332.73, 47.42, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(51, 6, 'eym_gmp', 15.00, 332.73, 71.12, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(52, 6, 'iv', 15.00, 332.73, 118.53, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(53, 6, 'guarderias', 15.00, 332.73, 49.91, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(54, 6, 'retiro', 15.00, 332.73, 99.82, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(55, 6, 'cv', 15.00, 332.73, 356.90, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(56, 6, 'infonavit', 15.00, 332.73, 249.55, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_nomina_detalle OVERRIDING SYSTEM VALUE VALUES
	(13, 4, 15, 13.00, 6500.00, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(14, 4, 18, 1.00, 0.00, 125.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(15, 4, 2, 1.00, 544.61, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(16, 4, 1, 1.00, 176.22, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(17, 4, 11, 1.00, 0.00, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(18, 4, 12, 1.00, 0.00, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(19, 4, 6, 15.00, 995.26, 0.00, DEFAULT, NULL, 1, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(20, 4, 7, 1.00, 500.00, 0.00, DEFAULT, NULL, NULL, 1, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(21, 4, 4, 1.00, 500.00, 0.00, DEFAULT, 1, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(22, 5, 16, 8.00, 2000.00, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(23, 5, 14, 1.00, 109.28, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(24, 5, 8, 1.00, 400.00, 0.00, DEFAULT, NULL, NULL, NULL, 1, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(25, 7, 19, 15.00, 3980.70, 3519.30, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(26, 7, 2, 1.00, 713.31, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(27, 7, 7, 1.00, 1000.00, 0.00, DEFAULT, NULL, NULL, 1, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(28, 7, 4, 1.00, 500.00, 0.00, DEFAULT, 1, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(29, 8, 19, 15.00, 0.00, 2000.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(30, 8, 8, 1.00, 400.00, 0.00, DEFAULT, NULL, NULL, NULL, 1, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(38, 10, 15, 5.00, 2500.00, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(39, 10, 19, 12.04, 6020.55, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(40, 10, 24, 35.56, 17780.00, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(41, 10, 18, 35.56, 2810.35, 1634.65, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(42, 10, 25, 91.46, 0.00, 45731.40, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(43, 10, 26, 90.00, 8797.20, 38731.80, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(44, 10, 26, 152.44, 80502.51, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(45, 10, 2, 1.00, 12681.17, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(46, 10, 7, 1.00, 5500.00, 0.00, DEFAULT, NULL, NULL, 1, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(47, 10, 4, 1.00, 2500.00, 0.00, DEFAULT, 1, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(48, 11, 23, 365.00, 0.00, 493.15, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(49, 11, 8, 1.00, 98.63, 0.00, DEFAULT, NULL, NULL, NULL, 1, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(50, 12, 23, 292.00, 874.93, 1759.65, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(51, 12, 2, 1.00, 156.81, 0.00, DEFAULT, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(52, 12, 7, 1.00, 1000.00, 0.00, DEFAULT, NULL, NULL, 1, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(53, 12, 4, 1.00, 500.00, 0.00, DEFAULT, 1, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_nominas OVERRIDING SYSTEM VALUE VALUES
	(4, 19, 1, 13.00, 1.00, 1.00, 500.00, 528.10, 528.10, 6625.00, 6500.00, 125.00, 2716.09, 0.00, 544.61, 0.00, 544.61, 176.22, 3908.91, 1750.99, NULL, 'autorizado', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', 1, '2026-10-23 20:58:02.068688-06', true),
	(5, 19, 2, 15.00, 0.00, 0.00, 315.04, 332.73, 332.73, 2000.00, 2000.00, 0.00, 400.00, 0.00, 109.28, 264.30, 0.00, 0.00, 1600.00, 1379.35, NULL, 'autorizado', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', 1, '2026-10-23 20:58:02.068688-06', true),
	(6, 19, 3, 15.00, 0.00, 0.00, 315.04, 332.73, 332.73, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 264.30, 0.00, 0.00, 0.00, 1379.35, 'Sin percepciones en el periodo', 'autorizado', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', 1, '2026-10-23 20:58:02.068688-06', true),
	(7, 25, 1, 365.00, 0.00, 0.00, 500.00, 0.00, 0.00, 7500.00, 3980.70, 3519.30, 2213.31, 0.00, 713.31, 0.00, 713.31, 0.00, 5286.69, 0.00, NULL, 'calculado', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(8, 25, 2, 365.00, 0.00, 0.00, 133.33, 0.00, 0.00, 2000.00, 0.00, 2000.00, 400.00, 0.00, 0.00, 0.00, 0.00, 0.00, 1600.00, 0.00, NULL, 'calculado', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(10, 26, 1, 5.00, 0.00, 0.00, 500.00, 528.10, 0.00, 204508.46, 118410.61, 86097.85, 20681.17, 0.00, 12681.17, 0.00, 12681.17, 0.00, 183827.29, 0.00, NULL, 'calculado', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(11, 27, 2, 365.00, 0.00, 0.00, 5.48, 0.00, 0.00, 493.15, 0.00, 493.15, 98.63, 0.00, 0.00, 0.00, 0.00, 0.00, 394.52, 0.00, NULL, 'calculado', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(12, 27, 1, 292.00, 0.00, 0.00, 500.00, 0.00, 0.00, 2634.58, 874.93, 1759.65, 1656.81, 0.00, 156.81, 0.00, 156.81, 0.00, 977.77, 0.00, NULL, 'calculado', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_pensiones_alimenticias OVERRIDING SYSTEM VALUE VALUES
	(1, 2, 'Beneficiario de prueba', NULL, 1, 20.0000, '2026-01-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_periodicidades_pago OVERRIDING SYSTEM VALUE VALUES
	(1, 'semanal', '02', 'Semanal', 7.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'catorcenal', '03', 'Catorcenal', 14.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'quincenal', '04', 'Quincenal', 15.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'mensual', '05', 'Mensual', 30.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 'unica', '99', 'Otra periodicidad (aguinaldo, PTU, finiquito)', 0.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_periodos_nomina OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 1, 3, 2026, 1, '1ra quincena enero 2026', '2026-01-01', '2026-01-15', '2026-01-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 1, 1, 3, 2026, 2, '2da quincena enero 2026', '2026-01-16', '2026-01-31', '2026-01-31', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 1, 1, 3, 2026, 3, '1ra quincena febrero 2026', '2026-02-01', '2026-02-15', '2026-02-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(4, 1, 1, 3, 2026, 4, '2da quincena febrero 2026', '2026-02-16', '2026-02-28', '2026-02-28', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(5, 1, 1, 3, 2026, 5, '1ra quincena marzo 2026', '2026-03-01', '2026-03-15', '2026-03-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(6, 1, 1, 3, 2026, 6, '2da quincena marzo 2026', '2026-03-16', '2026-03-31', '2026-03-31', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(7, 1, 1, 3, 2026, 7, '1ra quincena abril 2026', '2026-04-01', '2026-04-15', '2026-04-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(8, 1, 1, 3, 2026, 8, '2da quincena abril 2026', '2026-04-16', '2026-04-30', '2026-04-30', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(9, 1, 1, 3, 2026, 9, '1ra quincena mayo 2026', '2026-05-01', '2026-05-15', '2026-05-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(10, 1, 1, 3, 2026, 10, '2da quincena mayo 2026', '2026-05-16', '2026-05-31', '2026-05-31', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(11, 1, 1, 3, 2026, 11, '1ra quincena junio 2026', '2026-06-01', '2026-06-15', '2026-06-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(12, 1, 1, 3, 2026, 12, '2da quincena junio 2026', '2026-06-16', '2026-06-30', '2026-06-30', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(13, 1, 1, 3, 2026, 13, '1ra quincena julio 2026', '2026-07-01', '2026-07-15', '2026-07-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(14, 1, 1, 3, 2026, 14, '2da quincena julio 2026', '2026-07-16', '2026-07-31', '2026-07-31', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(15, 1, 1, 3, 2026, 15, '1ra quincena agosto 2026', '2026-08-01', '2026-08-15', '2026-08-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(16, 1, 1, 3, 2026, 16, '2da quincena agosto 2026', '2026-08-16', '2026-08-31', '2026-08-31', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(17, 1, 1, 3, 2026, 17, '1ra quincena septiembre 2026', '2026-09-01', '2026-09-15', '2026-09-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(18, 1, 1, 3, 2026, 18, '2da quincena septiembre 2026', '2026-09-16', '2026-09-30', '2026-09-30', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(20, 1, 1, 3, 2026, 20, '2da quincena octubre 2026', '2026-10-16', '2026-10-31', '2026-10-31', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(21, 1, 1, 3, 2026, 21, '1ra quincena noviembre 2026', '2026-11-01', '2026-11-15', '2026-11-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(22, 1, 1, 3, 2026, 22, '2da quincena noviembre 2026', '2026-11-16', '2026-11-30', '2026-11-30', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(23, 1, 1, 3, 2026, 23, '1ra quincena diciembre 2026', '2026-12-01', '2026-12-15', '2026-12-15', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(24, 1, 1, 3, 2026, 24, '2da quincena diciembre 2026', '2026-12-16', '2026-12-31', '2026-12-31', 15.00, 'abierto', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(19, 1, 1, 3, 2026, 19, '1ra quincena octubre 2026', '2026-10-01', '2026-10-15', '2026-10-15', 15.00, 'cerrado', 1, '2026-10-23 20:58:02.068688-06', 1, '2026-10-23 20:58:02.068688-06', 1, '2026-10-23 20:58:02.068688-06', true),
	(25, 1, 2, 5, 2026, 1, 'Aguinaldo 2026', '2026-01-01', '2026-12-31', '2026-12-15', 0.00, 'calculado', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(26, 1, 4, 5, 2026, 1, 'Finiquito Ana Lopez', '2026-10-20', '2026-10-20', '2026-10-20', 0.00, 'calculado', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(27, 1, 3, 5, 2027, 1, 'PTU 2026', '2027-05-01', '2027-05-31', '2027-05-30', 0.00, 'calculado', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_primas_riesgo_trabajo OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 0.54355, '2026-03-01', 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_ptu OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 2026, 50000.00, DEFAULT, NULL, 27, 'calculado', 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_ptu_empleados OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 2, 365, 2000.00, 892.86, 579.71, 1472.57, 493.15, 0.00, 493.15, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 1, 1, 292, 6625.00, 714.29, 1920.29, 2634.58, 45000.00, 0.00, 2634.58, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 1, 3, 365, 0.00, 892.86, 0.00, 892.86, 0.00, 0.00, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_registros_patronales OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 'A1234567890', 1, 1, 1, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_subsidio_empleo OVERRIDING SYSTEM VALUE VALUES
	(1, '2025-01-01', '2025-12-31', 13.800, 3439.46, 10171.00, DEFAULT, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, '2026-01-01', '2026-01-31', 15.590, 3439.46, 11492.66, DEFAULT, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, '2026-02-01', NULL, 15.020, 3566.22, 11492.66, DEFAULT, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tarifas_isr OVERRIDING SYSTEM VALUE VALUES
	(1, 2026, 'mensual', 0.01, 844.59, 0.00, 1.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 2026, 'mensual', 844.60, 7168.51, 16.22, 6.40, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 2026, 'mensual', 7168.52, 12598.02, 420.95, 10.88, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 2026, 'mensual', 12598.03, 14644.64, 1011.68, 16.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 2026, 'mensual', 14644.65, 17533.64, 1339.14, 17.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, 2026, 'mensual', 17533.65, 35362.83, 1856.84, 21.36, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, 2026, 'mensual', 35362.84, 55736.68, 5665.16, 23.52, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, 2026, 'mensual', 55736.69, 106410.50, 10457.09, 30.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, 2026, 'mensual', 106410.51, 141880.66, 25659.23, 32.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, 2026, 'mensual', 141880.67, 425641.99, 37009.69, 34.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(11, 2026, 'mensual', 425642.00, NULL, 133488.54, 35.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(12, 2026, 'quincenal', 0.01, 416.70, 0.00, 1.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(13, 2026, 'quincenal', 416.71, 3537.15, 7.95, 6.40, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(14, 2026, 'quincenal', 3537.16, 6216.15, 207.75, 10.88, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(15, 2026, 'quincenal', 6216.16, 7225.95, 499.20, 16.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(16, 2026, 'quincenal', 7225.96, 8651.40, 660.75, 17.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(17, 2026, 'quincenal', 8651.41, 17448.75, 916.20, 21.36, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(18, 2026, 'quincenal', 17448.76, 27501.60, 2795.25, 23.52, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(19, 2026, 'quincenal', 27501.61, 52505.25, 5159.70, 30.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(20, 2026, 'quincenal', 52505.26, 70006.95, 12660.75, 32.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(21, 2026, 'quincenal', 70006.96, 210020.70, 18261.30, 34.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(22, 2026, 'quincenal', 210020.71, NULL, 65866.05, 35.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(23, 2026, 'semanal', 0.01, 194.46, 0.00, 1.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(24, 2026, 'semanal', 194.47, 1650.67, 3.71, 6.40, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(25, 2026, 'semanal', 1650.68, 2900.87, 96.95, 10.88, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(26, 2026, 'semanal', 2900.88, 3372.11, 232.96, 16.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(27, 2026, 'semanal', 3372.12, 4037.32, 308.35, 17.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(28, 2026, 'semanal', 4037.33, 8142.75, 427.56, 21.36, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(29, 2026, 'semanal', 8142.76, 12834.08, 1304.45, 23.52, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(30, 2026, 'semanal', 12834.09, 24502.45, 2407.86, 30.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(31, 2026, 'semanal', 24502.46, 32669.91, 5908.35, 32.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(32, 2026, 'semanal', 32669.92, 98009.66, 8521.94, 34.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(33, 2026, 'semanal', 98009.67, NULL, 30737.49, 35.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(34, 2026, 'diaria', 0.01, 27.78, 0.00, 1.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(35, 2026, 'diaria', 27.79, 235.81, 0.53, 6.40, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(36, 2026, 'diaria', 235.82, 414.41, 13.85, 10.88, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(37, 2026, 'diaria', 414.42, 481.73, 33.28, 16.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(38, 2026, 'diaria', 481.74, 576.76, 44.05, 17.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(39, 2026, 'diaria', 576.77, 1163.25, 61.08, 21.36, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(40, 2026, 'diaria', 1163.26, 1833.44, 186.35, 23.52, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(41, 2026, 'diaria', 1833.45, 3500.35, 343.98, 30.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(42, 2026, 'diaria', 3500.36, 4667.13, 844.05, 32.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(43, 2026, 'diaria', 4667.14, 14001.38, 1217.42, 34.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(44, 2026, 'diaria', 14001.39, NULL, 4391.07, 35.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(45, 2026, 'anual', 0.01, 10135.11, 0.00, 1.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(46, 2026, 'anual', 10135.12, 86022.11, 194.59, 6.40, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(47, 2026, 'anual', 86022.12, 151176.19, 5051.37, 10.88, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(48, 2026, 'anual', 151176.20, 175735.66, 12140.13, 16.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(49, 2026, 'anual', 175735.67, 210403.69, 16069.64, 17.92, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(50, 2026, 'anual', 210403.70, 424353.97, 22282.14, 21.36, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(51, 2026, 'anual', 424353.98, 668840.14, 67981.92, 23.52, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(52, 2026, 'anual', 668840.15, 1276925.98, 125485.07, 30.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(53, 2026, 'anual', 1276925.99, 1702567.97, 307910.81, 32.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(54, 2026, 'anual', 1702567.98, 5107703.92, 444116.23, 34.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(55, 2026, 'anual', 5107703.93, NULL, 1601862.46, 35.00, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_concepto OVERRIDING SYSTEM VALUE VALUES
	(1, 'percepcion', 'Percepción', 'c_TipoPercepcion', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'deduccion', 'Deducción', 'c_TipoDeduccion', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'otro_pago', 'Otro pago', 'c_TipoOtroPago', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'informativo', 'Informativo (no suma al neto)', NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_contrato OVERRIDING SYSTEM VALUE VALUES
	(1, '01', 'Contrato de trabajo por tiempo indeterminado', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, '02', 'Contrato de trabajo para obra determinada', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, '03', 'Contrato de trabajo por tiempo determinado', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, '04', 'Contrato de trabajo por temporada', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, '05', 'Contrato de trabajo sujeto a prueba', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, '06', 'Contrato de trabajo con capacitación inicial', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, '07', 'Modalidad de contratación por pago de hora laborada', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, '08', 'Modalidad de trabajo por comisión laboral', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, '09', 'Modalidades de contratación donde no existe relación de trabajo', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, '10', 'Jubilación, pensión, retiro', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(11, '99', 'Otro contrato', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_descuento OVERRIDING SYSTEM VALUE VALUES
	(1, 'porcentaje', 'Porcentaje (INFONAVIT: del SBC; pensión: del neto legal)', true, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'cuota_fija', 'Cuota fija mensual en pesos', true, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'veces_umi', 'Veces UMI (mensual)', true, false, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'monto_fijo', 'Monto fijo por periodo', false, true, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_exencion OVERRIDING SYSTEM VALUE VALUES
	(1, 'gravado', 'Totalmente gravado', 0.00, NULL, 'ninguno', false, NULL, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'exento', 'Totalmente exento', 100.00, NULL, 'ninguno', false, 'Art. 93 LISR', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'aguinaldo', 'Aguinaldo: exento hasta 30 UMA al año', 100.00, 30.00, 'ninguno', true, 'Art. 93 fr. XIV LISR', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'prima_vacacional', 'Prima vacacional: exento hasta 15 UMA al año', 100.00, 15.00, 'ninguno', true, 'Art. 93 fr. XIV LISR', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 'ptu', 'PTU: exento hasta 15 UMA al año', 100.00, 15.00, 'ninguno', true, 'Art. 93 fr. XIV LISR', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, 'prima_dominical', 'Prima dominical: exento 1 UMA por domingo', 100.00, 1.00, 'domingos', false, 'Art. 93 fr. XIV LISR', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, 'horas_extra', 'Horas extra dobles: 50% exento, tope 5 UMA por semana', 50.00, 5.00, 'semanas', false, 'Art. 93 fr. I LISR', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, 'separacion', 'Separación: exento 90 UMA por año de servicio', 100.00, 90.00, 'anios_servicio', true, 'Art. 93 fr. XIII LISR', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_indicador OVERRIDING SYSTEM VALUE VALUES
	(1, 'uma', 'Unidad de Medida y Actualización', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'salario_minimo_general', 'Salario mínimo general', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'salario_minimo_frontera', 'Salario mínimo zona libre de la frontera norte', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'umi', 'Unidad Mixta INFONAVIT', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_jornada OVERRIDING SYSTEM VALUE VALUES
	(1, '01', 'Diurna', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, '02', 'Nocturna', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, '03', 'Mixta', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, '04', 'Por hora', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, '05', 'Reducida', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, '06', 'Continuada', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, '07', 'Partida', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, '08', 'Por turnos', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, '99', 'Otra jornada', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_movimiento_imss OVERRIDING SYSTEM VALUE VALUES
	(1, 'alta', '08', 'Alta', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'reingreso', '08', 'Reingreso', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'modificacion', '07', 'Modificación de salario', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'baja', '02', 'Baja', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_nomina OVERRIDING SYSTEM VALUE VALUES
	(1, 'ordinaria', 'O', 'Nómina ordinaria', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'aguinaldo', 'E', 'Aguinaldo', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, 'ptu', 'E', 'Reparto de utilidades', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, 'finiquito', 'E', 'Finiquito / liquidación', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, 'extraordinaria', 'E', 'Nómina extraordinaria', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_tipos_regimen OVERRIDING SYSTEM VALUE VALUES
	(1, '02', 'Sueldos (incluye ingresos señalados en la fracción I del art. 94 LISR)', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, '03', 'Jubilados', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(3, '04', 'Pensionados', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(4, '05', 'Asimilados miembros sociedades cooperativas de producción', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(5, '06', 'Asimilados integrantes sociedades y asociaciones civiles', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(6, '07', 'Asimilados miembros consejos', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(7, '08', 'Asimilados comisionistas', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(8, '09', 'Asimilados honorarios', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(9, '10', 'Asimilados acciones', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(10, '11', 'Asimilados otros', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(11, '12', 'Jubilados o pensionados', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(12, '13', 'Indemnización o separación', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(13, '99', 'Otro régimen', 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO nomina.tb_bi_zonas_salario_minimo OVERRIDING SYSTEM VALUE VALUES
	(1, 'general', 'Resto del país', 2, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true),
	(2, 'frontera', 'Zona libre de la frontera norte', 3, 1, '2026-10-23 20:53:36.336465-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_acciones_directorio OVERRIDING SYSTEM VALUE VALUES
	(1, 'crear', 'Cuenta creada en AD', 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(2, 'empatar', 'Cuenta existente de Microsoft 365 empatada con AD', 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(3, 'actualizar', 'Atributos actualizados (puesto, jefe, departamento...)', 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(5, 'deshabilitar', 'Cuenta deshabilitada por baja', 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(6, 'habilitar', 'Cuenta habilitada por reingreso', 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(4, 'mover', 'Cuenta movida de OU (cambio de campus o empresa)', 1, '2026-10-24 11:46:14.650911-06', NULL, '2026-10-24 14:42:59.768337-06', true);

INSERT INTO recursos_humanos.tb_bi_asistencia_diaria OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 4, NULL, '2026-10-01', 1, 0, 7.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 1, 5, NULL, '2026-10-02', 1, 0, 7.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 1, 1, NULL, '2026-10-05', 1, 0, 7.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(4, 2, 6, NULL, '2026-10-05', 1, 0, 2.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(5, 1, 2, NULL, '2026-10-06', 1, 0, 7.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(6, 1, 3, NULL, '2026-10-07', 3, 0, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(7, 2, 7, NULL, '2026-10-07', 1, 0, 2.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(8, 1, 4, NULL, '2026-10-08', 1, 0, 7.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(9, 1, 5, NULL, '2026-10-09', 1, 0, 7.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(10, 1, 1, NULL, '2026-10-12', 1, 0, 7.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(11, 2, 6, NULL, '2026-10-12', 1, 0, 2.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(12, 1, 2, 1, '2026-10-13', 7, 0, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(13, 1, 3, NULL, '2026-10-14', 6, 0, 0.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(14, 2, 7, NULL, '2026-10-14', 1, 0, 2.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(15, 1, 4, NULL, '2026-10-15', 1, 0, 7.00, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_asistencias OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 1, '2026-10-01 08:55:00', 'checada', 1, 1, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 1, 1, '2026-10-01 17:05:00', 'checada', 1, 2, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 1, 1, '2026-10-02 08:55:00', 'checada', 1, 3, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(4, 1, 1, '2026-10-02 17:05:00', 'checada', 1, 4, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(5, 1, 1, '2026-10-05 08:55:00', 'checada', 1, 5, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(6, 1, 1, '2026-10-05 17:05:00', 'checada', 1, 6, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(7, 1, 1, '2026-10-06 08:55:00', 'checada', 1, 7, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(8, 1, 1, '2026-10-06 17:05:00', 'checada', 1, 8, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(9, 1, 1, '2026-10-08 08:55:00', 'checada', 1, 9, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(10, 1, 1, '2026-10-08 17:05:00', 'checada', 1, 10, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(11, 1, 1, '2026-10-09 08:55:00', 'checada', 1, 11, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(12, 1, 1, '2026-10-09 17:05:00', 'checada', 1, 12, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(13, 1, 1, '2026-10-12 08:55:00', 'checada', 1, 13, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(14, 1, 1, '2026-10-12 17:05:00', 'checada', 1, 14, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(15, 1, 1, '2026-10-14 08:55:00', 'checada', 1, 15, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(16, 1, 1, '2026-10-14 17:05:00', 'checada', 1, 16, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(17, 1, 1, '2026-10-15 08:55:00', 'checada', 1, 17, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(18, 1, 1, '2026-10-15 17:05:00', 'checada', 1, 18, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(19, 2, 1, '2026-10-05 06:58:00', 'checada', 1, 1001, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(20, 2, 1, '2026-10-05 09:01:00', 'checada', 1, 1002, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(21, 2, 1, '2026-10-07 06:58:00', 'checada', 1, 1003, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(22, 2, 1, '2026-10-07 09:01:00', 'checada', 1, 1004, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(23, 2, 1, '2026-10-12 06:58:00', 'checada', 1, 1005, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(24, 2, 1, '2026-10-12 09:01:00', 'checada', 1, 1006, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(25, 2, 1, '2026-10-14 06:58:00', 'checada', 1, 1007, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(26, 2, 1, '2026-10-14 09:01:00', 'checada', 1, 1008, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_bajas_laborales OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 5, '2026-10-20', 183827.29, 'pendiente', 10, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_causas_baja_imss OVERRIDING SYSTEM VALUE VALUES
	(1, '1', 'Término de contrato', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, '2', 'Separación voluntaria', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, '3', 'Abandono de empleo', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, '4', 'Defunción', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(5, '5', 'Clausura', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(6, '6', 'Otras', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(7, '7', 'Ausentismo', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(8, '8', 'Rescisión de contrato', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(9, '9', 'Jubilación', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(10, 'A', 'Pensión', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_checadores OVERRIDING SYSTEM VALUE VALUES
	(1, 'Hikvision Centenario', 'FX1', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_departamentos OVERRIDING SYSTEM VALUE VALUES
	(1, 'Sistemas', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true, NULL),
	(2, 'Academia', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true, NULL);

INSERT INTO recursos_humanos.tb_bi_dias_festivos OVERRIDING SYSTEM VALUE VALUES
	(1, '2026-01-01', 'Año Nuevo', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, '2026-02-02', 'Día de la Constitución', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, '2026-03-16', 'Natalicio de Benito Juárez', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, '2026-05-01', 'Día del Trabajo', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(5, '2026-09-16', 'Día de la Independencia', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(6, '2026-11-16', 'Día de la Revolución', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(7, '2026-12-25', 'Navidad', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(8, '2027-01-01', 'Año Nuevo', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(9, '2027-02-01', 'Día de la Constitución', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(10, '2027-03-15', 'Natalicio de Benito Juárez', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(11, '2027-05-01', 'Día del Trabajo', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(12, '2027-09-16', 'Día de la Independencia', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(13, '2027-11-15', 'Día de la Revolución', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(14, '2027-12-25', 'Navidad', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_empleados OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 1, 1, 3, NULL, 101, 'E001', '2019-03-10', 1, true, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true, NULL, NULL, NULL),
	(3, 3, 1, 2, 1, NULL, 103, 'E003', '2025-02-01', 1, true, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true, NULL, NULL, NULL),
	(2, 2, 1, 2, 1, NULL, 102, 'E002', '2024-01-08', 1, true, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true, NULL, NULL, NULL);

INSERT INTO recursos_humanos.tb_bi_empresas OVERRIDING SYSTEM VALUE VALUES
	(1, 'Universidad de Prueba AC', 'USAG', 'UPR010101AB1', 2, '20000', NULL, NULL, NULL, NULL, 'NOM', 1, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true, NULL, NULL);

INSERT INTO recursos_humanos.tb_bi_estatus_asistencia OVERRIDING SYSTEM VALUE VALUES
	(1, 'a_tiempo', 'A tiempo', '#2E7D32', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'retardo', 'Retardo', '#F9A825', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, 'falta_injustificada', 'Falta injustificada', '#C62828', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, 'falta_justificada', 'Falta justificada / permiso con goce', '#1565C0', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(5, 'permiso_sin_goce', 'Permiso sin goce', '#6A1B9A', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(6, 'incapacidad', 'Incapacidad IMSS', '#00838F', false, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(7, 'vacaciones', 'Vacaciones', '#558B2F', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(8, 'suplencia', 'Clase cubierta por suplente', '#757575', false, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_estatus_laborales OVERRIDING SYSTEM VALUE VALUES
	(1, 'activo', 'Activo', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'baja', 'Baja', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, 'suspendido', 'Suspendido', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_historial_horarios OVERRIDING SYSTEM VALUE VALUES
	(1, 1, NULL, NULL, 1, 1, '09:00:00', '17:00:00', '14:00:00', '15:00:00', '2026-01-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 1, NULL, NULL, 1, 2, '09:00:00', '17:00:00', '14:00:00', '15:00:00', '2026-01-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 1, NULL, NULL, 1, 3, '09:00:00', '17:00:00', '14:00:00', '15:00:00', '2026-01-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(4, 1, NULL, NULL, 1, 4, '09:00:00', '17:00:00', '14:00:00', '15:00:00', '2026-01-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(5, 1, NULL, NULL, 1, 5, '09:00:00', '17:00:00', '14:00:00', '15:00:00', '2026-01-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(6, 2, 1, 1, 2, 1, '07:00:00', '09:00:00', NULL, NULL, '2026-09-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(7, 2, 1, 1, 2, 3, '07:00:00', '09:00:00', NULL, NULL, '2026-09-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_historial_sueldos OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 500.00, NULL, '2019-03-10', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 2, NULL, 250.00, '2024-01-08', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 3, NULL, 250.00, '2025-02-01', NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_incapacidades_imss OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 'XX001', 1, 2, '2026-10-14', '2026-10-14', 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_lft_vacaciones OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 12, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 2, 14, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, 3, 16, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, 4, 18, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(5, 5, 20, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(6, 6, 22, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(7, 7, 22, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(8, 8, 22, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(9, 9, 22, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(10, 10, 22, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(11, 11, 24, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(12, 12, 24, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(13, 13, 24, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(14, 14, 24, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(15, 15, 24, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(16, 16, 26, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(17, 17, 26, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(18, 18, 26, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(19, 19, 26, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(20, 20, 26, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(21, 21, 28, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(22, 22, 28, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(23, 23, 28, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(24, 24, 28, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(25, 25, 28, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(26, 26, 30, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(27, 27, 30, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(28, 28, 30, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(29, 29, 30, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(30, 30, 30, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(31, 31, 32, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(32, 32, 32, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(33, 33, 32, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(34, 34, 32, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(35, 35, 32, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(36, 36, 34, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(37, 37, 34, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(38, 38, 34, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(39, 39, 34, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(40, 40, 34, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_materias OVERRIDING SYSTEM VALUE VALUES
	(1, 'Bases de Datos', 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_modos_checada OVERRIDING SYSTEM VALUE VALUES
	(1, 'por_jornada', 'Una entrada y una salida por día', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'por_clase', 'Checa en cada clase', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_motivos_baja OVERRIDING SYSTEM VALUE VALUES
	(1, 'Renuncia voluntaria', true, 3, false, false, 2, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'Termino de contrato', true, 1, false, false, 1, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, 'Despido justificado (rescision)', true, 2, false, false, 8, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, 'Despido injustificado', true, 2, true, false, 6, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(5, 'Despido injustificado sin reinstalacion (art. 50)', true, 2, true, true, 6, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(6, 'Jubilacion o pension', true, 2, false, false, 9, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(7, 'Fallecimiento', true, 2, false, false, 4, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(8, 'Abandono de trabajo', true, 1, false, false, 3, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_origenes_checada OVERRIDING SYSTEM VALUE VALUES
	(1, 'api_hikvision', 'Checador Hikvision (en línea)', true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'usb_offline', 'Checador Hikvision (descarga USB)', true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, 'manual_rh', 'Captura manual de Recursos Humanos', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_perfiles OVERRIDING SYSTEM VALUE VALUES
	(1, 'Docente por Asignatura', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'Docente de Tiempo Completo', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, 'Administrativo', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, 'Operativo', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(5, 'Directivo', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_periodos_acumulacion OVERRIDING SYSTEM VALUE VALUES
	(1, 'mensual', 'Los retardos se acumulan en el mes', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'quincenal', 'Los retardos se acumulan en la quincena', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_periodos_escolares OVERRIDING SYSTEM VALUE VALUES
	(1, 'Sep-Dic 2026', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_permisos OVERRIDING SYSTEM VALUE VALUES
	(1, 'empleados.ver', 'Empleados', 'Ver empleados', true, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(2, 'empleados.editar', 'Empleados', 'Dar de alta y modificar empleados', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(3, 'asistencia.ver', 'Asistencia', 'Ver asistencia y checadas', true, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(4, 'asistencia.registrar_checadas', 'Asistencia', 'Enviar checadas (agente de checadores o descarga USB)', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(5, 'asistencia.procesar', 'Asistencia', 'Recalcular la asistencia', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(6, 'horarios.ver', 'Asistencia', 'Ver horarios', true, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(7, 'horarios.editar', 'Asistencia', 'Dar de alta y terminar horarios', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(8, 'solicitudes.ver', 'Solicitudes', 'Ver solicitudes', true, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(9, 'solicitudes.crear_otros', 'Solicitudes', 'Crear solicitudes y adjuntar comprobantes a nombre de otro empleado', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(10, 'solicitudes.aprobar', 'Solicitudes', 'Aprobación final después del jefe y cancelar en cualquier momento', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(11, 'vacaciones.ver', 'Solicitudes', 'Ver saldos de vacaciones', true, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(12, 'vacaciones.generar_saldos', 'Solicitudes', 'Generar saldos de vacaciones', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(13, 'incapacidades.ver', 'Incidencias', 'Ver incapacidades', true, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(14, 'incapacidades.editar', 'Incidencias', 'Registrar y cancelar incapacidades', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(15, 'suplencias.ver', 'Incidencias', 'Ver suplencias', true, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(16, 'suplencias.asignar', 'Incidencias', 'Asignar suplentes en cualquier clase (el jefe ya puede en su equipo)', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(17, 'suplencias.aprobar', 'Incidencias', 'Aprobar suplencias', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(18, 'nomina.ver', 'Nómina', 'Ver periodos, incidencias y recibos de todos', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(19, 'nomina.calcular', 'Nómina', 'Generar periodos, capturar incidencias y calcular', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(20, 'nomina.autorizar', 'Nómina', 'Autorizar la nómina', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(21, 'nomina.cerrar', 'Nómina', 'Cerrar periodos', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(22, 'nomina.datos_ver', 'Nómina', 'Ver datos de nómina y sueldos de los empleados', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(23, 'nomina.datos_editar', 'Nómina', 'Modificar datos de nómina y sueldos', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(24, 'bajas.ver', 'Prestaciones', 'Ver bajas y finiquitos', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(25, 'bajas.editar', 'Prestaciones', 'Registrar bajas y calcular finiquitos', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(26, 'bajas.pagar', 'Prestaciones', 'Marcar finiquitos como pagados', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(27, 'ptu.ver', 'Prestaciones', 'Ver repartos de PTU', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(28, 'ptu.editar', 'Prestaciones', 'Capturar y calcular la PTU', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(29, 'creditos.ver', 'Créditos', 'Ver créditos y descuentos', true, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(30, 'creditos.editar', 'Créditos', 'Dar de alta y terminar créditos y descuentos', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(31, 'catalogos.editar', 'Catálogos', 'Modificar catálogos y empresas', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(32, 'auditoria.ver', 'Auditoría', 'Consultar la bitácora', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(33, 'roles.editar', 'Seguridad', 'Crear roles y asignar permisos', false, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(34, 'usuarios.ver', 'Seguridad', 'Ver usuarios', false, 1, '2026-10-24 23:02:09.567767-06', NULL, NULL, true),
	(35, 'usuarios.editar', 'Seguridad', 'Crear usuarios, cambiar su rol o estatus y restablecer contraseñas', false, 1, '2026-10-24 23:02:09.567767-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_personas OVERRIDING SYSTEM VALUE VALUES
	(1, 'Ana', 'Lopez', NULL, NULL, 'LOAA800101AAA', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 'Pedro', 'Ruiz', NULL, NULL, 'RUPE800101AAA', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 'Luis', 'Diaz', NULL, NULL, 'DILU800101AAA', NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_proveedores_autenticacion OVERRIDING SYSTEM VALUE VALUES
	(1, 'local', 'Usuario y contraseña del sistema', true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'active_directory', 'Active Directory (dominio USAG)', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, 'entra_id', 'Microsoft Entra ID', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, 'sistema', 'Procesos automáticos', false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_puestos OVERRIDING SYSTEM VALUE VALUES
	(1, 'RECTOR', 'Rector', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(2, 'DIR-TI', 'Director de TI', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(3, 'DIR-ACAD', 'Director Académico', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(4, 'COORD', 'Coordinador de carrera', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(5, 'DOC-ASIG', 'Docente por asignatura', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(6, 'DOC-TC', 'Docente de tiempo completo', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(7, 'JEFE-RH', 'Jefe de Recursos Humanos', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(8, 'AUX-RH', 'Auxiliar de Recursos Humanos', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(9, 'CONTADOR', 'Contador', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(10, 'AUX-ADM', 'Auxiliar administrativo', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(11, 'SOP-TI', 'Soporte técnico', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(12, 'DES-SW', 'Desarrollador de software', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(13, 'RECEP', 'Recepcionista', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(14, 'INTEND', 'Intendencia', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true),
	(15, 'VIGIL', 'Vigilancia', NULL, 1, '2026-10-24 11:46:14.650911-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_regimenes_fiscales OVERRIDING SYSTEM VALUE VALUES
	(1, '601', 'General de Ley Personas Morales', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, '603', 'Personas Morales con Fines no Lucrativos', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, '605', 'Sueldos y Salarios e Ingresos Asimilados a Salarios', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, '606', 'Arrendamiento', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(5, '607', 'Régimen de Enajenación o Adquisición de Bienes', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(6, '608', 'Demás ingresos', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(7, '610', 'Residentes en el Extranjero sin Establecimiento Permanente en México', true, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(8, '611', 'Ingresos por Dividendos (socios y accionistas)', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(9, '612', 'Personas Físicas con Actividades Empresariales y Profesionales', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(10, '614', 'Ingresos por intereses', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(11, '615', 'Régimen de los ingresos por obtención de premios', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(12, '616', 'Sin obligaciones fiscales', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(13, '620', 'Sociedades Cooperativas de Producción que optan por diferir sus ingresos', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(14, '621', 'Incorporación Fiscal', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(15, '622', 'Actividades Agrícolas, Ganaderas, Silvícolas y Pesqueras', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(16, '623', 'Opcional para Grupos de Sociedades', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(17, '624', 'Coordinados', false, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(18, '625', 'Régimen de las Actividades Empresariales con ingresos a través de Plataformas Tecnológicas', true, false, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(19, '626', 'Régimen Simplificado de Confianza', true, true, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_reglas_asistencia OVERRIDING SYSTEM VALUE VALUES
	(1, 'Regla Administrativos 2026', 10, 15, 3, true, 1, 1, '2026-01-01', NULL, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'Regla Docentes 2026', 10, 15, 3, true, 1, 1, '2026-01-01', NULL, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_reglas_prima_antiguedad OVERRIDING SYSTEM VALUE VALUES
	(1, 'no', 'No se paga', 0, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'siempre', 'Se paga sin importar la antigüedad', 0, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, 'con_15_anios', 'Solo con 15 años o más de servicio (renuncia voluntaria)', 15, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_roles OVERRIDING SYSTEM VALUE VALUES
	(1, 'Sistema', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, 'sistema'),
	(2, 'Empleado', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, 'empleado'),
	(3, 'Docente', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, 'docente'),
	(4, 'Jefe Directo / Coordinador', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, 'jefe_directo'),
	(5, 'Recursos Humanos', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, 'recursos_humanos'),
	(7, 'Administrador TI', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, 'administrador_ti'),
	(6, 'Finanzas', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, 'finanzas'),
	(8, 'Dirección', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true, 'direccion');

INSERT INTO recursos_humanos.tb_bi_roles_permisos OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 4, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(2, 2, 1, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(3, 2, 3, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(4, 2, 6, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(5, 2, 8, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(6, 2, 11, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(7, 2, 13, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(8, 2, 15, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(9, 2, 29, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(10, 3, 1, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(11, 3, 3, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(12, 3, 6, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(13, 3, 8, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(14, 3, 11, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(15, 3, 13, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(16, 3, 15, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(17, 3, 29, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(18, 4, 1, 'equipo', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(19, 4, 3, 'equipo', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(20, 4, 6, 'equipo', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(21, 4, 8, 'equipo', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(22, 4, 11, 'equipo', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(23, 4, 13, 'equipo', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(24, 4, 15, 'equipo', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(25, 4, 29, 'propio', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(26, 5, 1, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(27, 5, 2, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(28, 5, 3, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(29, 5, 4, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(30, 5, 5, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(31, 5, 6, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(32, 5, 7, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(33, 5, 8, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(34, 5, 9, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(35, 5, 10, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(36, 5, 11, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(37, 5, 12, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(38, 5, 13, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(39, 5, 14, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(40, 5, 15, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(41, 5, 16, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(42, 5, 17, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(43, 5, 18, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(44, 5, 19, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(45, 5, 21, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(46, 5, 22, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(47, 5, 23, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(48, 5, 24, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(49, 5, 25, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(50, 5, 26, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(51, 5, 27, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(52, 5, 28, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(53, 5, 29, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(54, 5, 30, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(55, 5, 31, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(56, 6, 1, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(57, 6, 3, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(58, 6, 8, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(59, 6, 11, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(60, 6, 13, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(61, 6, 18, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(62, 6, 21, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(63, 6, 22, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(64, 6, 24, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(65, 6, 26, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(66, 6, 27, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(67, 6, 29, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(68, 8, 1, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(69, 8, 3, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(70, 8, 6, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(71, 8, 8, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(72, 8, 11, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(73, 8, 13, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(74, 8, 15, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(75, 8, 18, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(76, 8, 20, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(77, 8, 21, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(78, 8, 22, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(79, 8, 24, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(80, 8, 26, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(81, 8, 27, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(82, 8, 29, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(83, 8, 32, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(84, 7, 1, 'todos', 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(85, 7, 31, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(86, 7, 32, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(87, 7, 33, NULL, 1, '2026-10-24 22:32:00.02343-06', NULL, NULL, true),
	(88, 5, 34, NULL, 1, '2026-10-24 23:02:09.567767-06', NULL, NULL, true),
	(89, 5, 35, NULL, 1, '2026-10-24 23:02:09.567767-06', NULL, NULL, true),
	(90, 7, 34, NULL, 1, '2026-10-24 23:02:09.567767-06', NULL, NULL, true),
	(91, 7, 35, NULL, 1, '2026-10-24 23:02:09.567767-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_saldos_vacaciones OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 7, 7, 22, 0, 0, '2027-09-10', false, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(3, 3, 1, 1, 12, 0, 0, '2027-08-01', false, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true),
	(2, 2, 2, 2, 14, 0, 0, '2027-07-08', false, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_solicitudes OVERRIDING SYSTEM VALUE VALUES
	(1, 1, 1, NULL, '2026-10-13', '2026-10-13', NULL, NULL, 1.0, NULL, 'aprobada', NULL, NULL, NULL, NULL, NULL, NULL, 1, '2026-10-23 20:58:02.068688-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_tipos_duracion_permiso OVERRIDING SYSTEM VALUE VALUES
	(1, 'por_dias', 'Por días', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, 'por_horas', 'Por horas', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_tipos_incapacidad OVERRIDING SYSTEM VALUE VALUES
	(1, '01', 'Riesgo de trabajo', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(2, '02', 'Enfermedad en general', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(3, '03', 'Maternidad', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true),
	(4, '04', 'Licencia por cuidados médicos de hijos diagnosticados con cáncer', 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true);

INSERT INTO recursos_humanos.tb_bi_tipos_permiso OVERRIDING SYSTEM VALUE VALUES
	(1, 'Vacaciones', false, true, true, 1, 7, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, NULL),
	(2, 'Permiso con goce', false, true, false, 1, 4, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, NULL),
	(3, 'Permiso sin goce', false, false, false, 1, 5, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, NULL),
	(4, 'Permiso por horas', false, true, false, 2, 4, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, NULL),
	(6, 'Justificacion de falta o retardo', true, true, false, 1, 4, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, NULL),
	(7, 'Omision de checada', false, true, false, 1, 4, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, NULL),
	(8, 'Comision o trabajo fuera', false, true, false, 1, 4, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, NULL),
	(5, 'Dia economico', false, true, true, 1, 4, 1, '2026-10-23 20:49:24.566341-06', 2, '2026-10-24 19:16:40.480727-06', true, NULL);

INSERT INTO recursos_humanos.tb_bi_usuarios VALUES
	(1, NULL, 1, 'sistema', NULL, 'activo', 4, NULL, NULL, 1, '2026-10-23 20:49:24.566341-06', NULL, NULL, true, 0, NULL, false, NULL),
	(8, NULL, 1, 'agente.checadores', '$2a$12$K0ykgceMH4.F9sLVDKeqbOH5gHwdMKw1BDeAKhlDoTCUVU.urllSa', 'activo', 1, NULL, '2026-10-24 14:11:32.00639-06', 1, '2026-10-24 14:11:20.293476-06', 1, '2026-10-24 14:11:32.011102-06', true, 0, NULL, false, NULL),
	(2, 1, 5, 'ana.lopez', '$2a$12$vCmdib6NQlvM/p8GbKOOXOUn53KxVn/S7JUhlbsHeCnkNp8YGD2te', 'activo', 1, NULL, '2026-10-24 19:46:44.681311-06', 1, '2026-10-24 13:18:16.544585-06', 1, '2026-10-24 19:46:44.685047-06', true, 0, NULL, false, NULL);

SELECT pg_catalog.setval('nomina.tb_bi_bancos_id_banco_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_cesantia_vejez_patron_id_cesantia_vejez_seq', 40, true);

SELECT pg_catalog.setval('nomina.tb_bi_clases_riesgo_id_clase_riesgo_seq', 6, true);

SELECT pg_catalog.setval('nomina.tb_bi_claves_sat_concepto_id_clave_sat_concepto_seq', 22, true);

SELECT pg_catalog.setval('nomina.tb_bi_conceptos_hist_id_hist_concepto_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_conceptos_nomina_id_concepto_seq', 29, true);

SELECT pg_catalog.setval('nomina.tb_bi_conceptos_recurrentes_id_concepto_recurrente_seq', 1, false);

SELECT pg_catalog.setval('nomina.tb_bi_creditos_fonacot_id_credito_fonacot_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_creditos_infonavit_id_credito_infonavit_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_cuotas_imss_id_cuota_imss_seq', 10, true);

SELECT pg_catalog.setval('nomina.tb_bi_empleados_nomina_id_empleado_nomina_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_esquemas_prestaciones_id_esquema_prestaciones_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_estados_id_estado_seq', 32, true);

SELECT pg_catalog.setval('nomina.tb_bi_incidencias_nomina_id_incidencia_nomina_seq', 1, false);

SELECT pg_catalog.setval('nomina.tb_bi_indicadores_id_indicador_seq', 7, true);

SELECT pg_catalog.setval('nomina.tb_bi_metodos_pago_id_metodo_pago_seq', 3, true);

SELECT pg_catalog.setval('nomina.tb_bi_movimientos_imss_id_movimiento_imss_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_nomina_cuotas_imss_id_nomina_cuota_seq', 66, true);

SELECT pg_catalog.setval('nomina.tb_bi_nomina_detalle_id_nomina_detalle_seq', 66, true);

SELECT pg_catalog.setval('nomina.tb_bi_nominas_id_nomina_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_pensiones_alimenticias_id_pension_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_periodicidades_pago_id_periodicidad_pago_seq', 5, true);

SELECT pg_catalog.setval('nomina.tb_bi_periodos_nomina_id_periodo_nomina_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_primas_riesgo_trabajo_id_prima_riesgo_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_ptu_empleados_id_ptu_empleado_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_ptu_id_ptu_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_registros_patronales_id_registro_patronal_seq', 33, true);

SELECT pg_catalog.setval('nomina.tb_bi_subsidio_empleo_id_subsidio_seq', 3, true);

SELECT pg_catalog.setval('nomina.tb_bi_tarifas_isr_id_tarifa_isr_seq', 55, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_concepto_id_tipo_concepto_seq', 4, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_contrato_id_tipo_contrato_seq', 11, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_descuento_id_tipo_descuento_seq', 4, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_exencion_id_tipo_exencion_seq', 8, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_indicador_id_tipo_indicador_seq', 4, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_jornada_id_tipo_jornada_seq', 9, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_movimiento_imss_id_tipo_movimiento_imss_seq', 4, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_nomina_id_tipo_nomina_seq', 5, true);

SELECT pg_catalog.setval('nomina.tb_bi_tipos_regimen_id_tipo_regimen_seq', 13, true);

SELECT pg_catalog.setval('nomina.tb_bi_zonas_salario_minimo_id_zona_salario_minimo_seq', 2, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_acciones_directorio_id_accion_directorio_seq', 6, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_asistencia_diaria_id_asistencia_diaria_seq', 75, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_asistencias_id_asistencia_seq', 36, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_auditoria_logs_id_log_seq', 15, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_bajas_laborales_id_baja_seq', 33, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_campus_id_campus_seq', 1, false);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_causas_baja_imss_id_causa_baja_imss_seq', 10, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_checadores_id_checador_seq', 33, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_cuentas_directorio_id_cuenta_directorio_seq', 1, false);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_departamentos_id_departamento_seq', 33, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_dias_festivos_id_dia_festivo_seq', 14, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_empleados_id_empleado_seq', 37, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_empresas_id_empresa_seq', 33, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_estatus_asistencia_id_estatus_asistencia_seq', 8, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_estatus_laborales_id_estatus_laboral_seq', 3, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_historial_horarios_id_historial_horario_seq', 37, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_historial_sueldos_id_historial_sueldo_seq', 33, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_incapacidades_imss_id_incapacidad_seq', 36, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_lft_vacaciones_id_lft_vacacion_seq', 40, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_materias_id_materia_seq', 33, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_modos_checada_id_modo_checada_seq', 2, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_motivos_baja_id_motivo_baja_seq', 8, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_origenes_checada_id_origen_checada_seq', 3, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_perfiles_id_perfil_seq', 5, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_periodos_acumulacion_id_periodo_acumulacion_seq', 2, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_periodos_escolares_id_periodo_seq', 33, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_permisos_id_permiso_seq', 35, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_personas_id_persona_seq', 37, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_proveedores_autenticacion_id_proveedor_autenticacion_seq', 4, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_puestos_id_puesto_seq', 15, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_regimenes_fiscales_id_regimen_fiscal_seq', 19, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_reglas_asistencia_id_regla_asistencia_seq', 2, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_reglas_prima_antiguedad_id_regla_prima_antiguedad_seq', 3, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_roles_grupos_directorio_id_rol_grupo_directorio_seq', 1, false);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_roles_id_rol_seq', 8, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_roles_permisos_id_rol_permiso_seq', 91, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_saldos_vacaciones_id_saldo_vacacion_seq', 39, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_sincronizacion_directorio_logs_id_sincronizacion_log_seq', 1, false);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_solicitudes_id_solicitud_seq', 49, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_suplencias_id_suplencia_seq', 3, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_tipos_duracion_permiso_id_tipo_duracion_permiso_seq', 2, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_tipos_incapacidad_id_tipo_incapacidad_seq', 4, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_tipos_permiso_id_tipo_permiso_seq', 8, true);

SELECT pg_catalog.setval('recursos_humanos.tb_bi_usuarios_id_usuario_seq', 13, true);

ALTER TABLE ONLY nomina.tb_bi_bancos
    ADD CONSTRAINT tb_bi_bancos_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_bancos
    ADD CONSTRAINT tb_bi_bancos_pkey PRIMARY KEY (id_banco);

ALTER TABLE ONLY nomina.tb_bi_cesantia_vejez_patron
    ADD CONSTRAINT tb_bi_cesantia_vejez_patron_pkey PRIMARY KEY (id_cesantia_vejez);

ALTER TABLE ONLY nomina.tb_bi_clases_riesgo
    ADD CONSTRAINT tb_bi_clases_riesgo_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_clases_riesgo
    ADD CONSTRAINT tb_bi_clases_riesgo_pkey PRIMARY KEY (id_clase_riesgo);

ALTER TABLE ONLY nomina.tb_bi_claves_sat_concepto
    ADD CONSTRAINT tb_bi_claves_sat_concepto_pkey PRIMARY KEY (id_clave_sat_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_hist
    ADD CONSTRAINT tb_bi_conceptos_hist_pkey PRIMARY KEY (id_hist_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_nomina
    ADD CONSTRAINT tb_bi_conceptos_nomina_clave_concepto_key UNIQUE (clave_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_nomina
    ADD CONSTRAINT tb_bi_conceptos_nomina_pkey PRIMARY KEY (id_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_recurrentes
    ADD CONSTRAINT tb_bi_conceptos_recurrentes_pkey PRIMARY KEY (id_concepto_recurrente);

ALTER TABLE ONLY nomina.tb_bi_creditos_fonacot
    ADD CONSTRAINT tb_bi_creditos_fonacot_numero_credito_key UNIQUE (numero_credito);

ALTER TABLE ONLY nomina.tb_bi_creditos_fonacot
    ADD CONSTRAINT tb_bi_creditos_fonacot_pkey PRIMARY KEY (id_credito_fonacot);

ALTER TABLE ONLY nomina.tb_bi_creditos_infonavit
    ADD CONSTRAINT tb_bi_creditos_infonavit_numero_credito_key UNIQUE (numero_credito);

ALTER TABLE ONLY nomina.tb_bi_creditos_infonavit
    ADD CONSTRAINT tb_bi_creditos_infonavit_pkey PRIMARY KEY (id_credito_infonavit);

ALTER TABLE ONLY nomina.tb_bi_cuotas_imss
    ADD CONSTRAINT tb_bi_cuotas_imss_pkey PRIMARY KEY (id_cuota_imss);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_empleado_key UNIQUE (id_empleado);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_pkey PRIMARY KEY (id_empleado_nomina);

ALTER TABLE ONLY nomina.tb_bi_esquemas_prestaciones
    ADD CONSTRAINT tb_bi_esquemas_prestaciones_pkey PRIMARY KEY (id_esquema_prestaciones);

ALTER TABLE ONLY nomina.tb_bi_estados
    ADD CONSTRAINT tb_bi_estados_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_estados
    ADD CONSTRAINT tb_bi_estados_pkey PRIMARY KEY (id_estado);

ALTER TABLE ONLY nomina.tb_bi_incidencias_nomina
    ADD CONSTRAINT tb_bi_incidencias_nomina_pkey PRIMARY KEY (id_incidencia_nomina);

ALTER TABLE ONLY nomina.tb_bi_indicadores
    ADD CONSTRAINT tb_bi_indicadores_pkey PRIMARY KEY (id_indicador);

ALTER TABLE ONLY nomina.tb_bi_metodos_pago
    ADD CONSTRAINT tb_bi_metodos_pago_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_metodos_pago
    ADD CONSTRAINT tb_bi_metodos_pago_pkey PRIMARY KEY (id_metodo_pago);

ALTER TABLE ONLY nomina.tb_bi_movimientos_imss
    ADD CONSTRAINT tb_bi_movimientos_imss_pkey PRIMARY KEY (id_movimiento_imss);

ALTER TABLE ONLY nomina.tb_bi_nomina_cuotas_imss
    ADD CONSTRAINT tb_bi_nomina_cuotas_imss_pkey PRIMARY KEY (id_nomina_cuota);

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT tb_bi_nomina_detalle_pkey PRIMARY KEY (id_nomina_detalle);

ALTER TABLE ONLY nomina.tb_bi_nominas
    ADD CONSTRAINT tb_bi_nominas_pkey PRIMARY KEY (id_nomina);

ALTER TABLE ONLY nomina.tb_bi_nominas
    ADD CONSTRAINT tb_bi_nominas_uuid_key UNIQUE (uuid);

ALTER TABLE ONLY nomina.tb_bi_pensiones_alimenticias
    ADD CONSTRAINT tb_bi_pensiones_alimenticias_pkey PRIMARY KEY (id_pension);

ALTER TABLE ONLY nomina.tb_bi_periodicidades_pago
    ADD CONSTRAINT tb_bi_periodicidades_pago_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_periodicidades_pago
    ADD CONSTRAINT tb_bi_periodicidades_pago_pkey PRIMARY KEY (id_periodicidad_pago);

ALTER TABLE ONLY nomina.tb_bi_periodos_nomina
    ADD CONSTRAINT tb_bi_periodos_nomina_pkey PRIMARY KEY (id_periodo_nomina);

ALTER TABLE ONLY nomina.tb_bi_primas_riesgo_trabajo
    ADD CONSTRAINT tb_bi_primas_riesgo_trabajo_pkey PRIMARY KEY (id_prima_riesgo);

ALTER TABLE ONLY nomina.tb_bi_ptu_empleados
    ADD CONSTRAINT tb_bi_ptu_empleados_pkey PRIMARY KEY (id_ptu_empleado);

ALTER TABLE ONLY nomina.tb_bi_ptu
    ADD CONSTRAINT tb_bi_ptu_pkey PRIMARY KEY (id_ptu);

ALTER TABLE ONLY nomina.tb_bi_registros_patronales
    ADD CONSTRAINT tb_bi_registros_patronales_pkey PRIMARY KEY (id_registro_patronal);

ALTER TABLE ONLY nomina.tb_bi_registros_patronales
    ADD CONSTRAINT tb_bi_registros_patronales_registro_patronal_key UNIQUE (registro_patronal);

ALTER TABLE ONLY nomina.tb_bi_subsidio_empleo
    ADD CONSTRAINT tb_bi_subsidio_empleo_fecha_inicio_key UNIQUE (fecha_inicio);

ALTER TABLE ONLY nomina.tb_bi_subsidio_empleo
    ADD CONSTRAINT tb_bi_subsidio_empleo_pkey PRIMARY KEY (id_subsidio);

ALTER TABLE ONLY nomina.tb_bi_tarifas_isr
    ADD CONSTRAINT tb_bi_tarifas_isr_pkey PRIMARY KEY (id_tarifa_isr);

ALTER TABLE ONLY nomina.tb_bi_tipos_concepto
    ADD CONSTRAINT tb_bi_tipos_concepto_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_concepto
    ADD CONSTRAINT tb_bi_tipos_concepto_pkey PRIMARY KEY (id_tipo_concepto);

ALTER TABLE ONLY nomina.tb_bi_tipos_contrato
    ADD CONSTRAINT tb_bi_tipos_contrato_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_contrato
    ADD CONSTRAINT tb_bi_tipos_contrato_pkey PRIMARY KEY (id_tipo_contrato);

ALTER TABLE ONLY nomina.tb_bi_tipos_descuento
    ADD CONSTRAINT tb_bi_tipos_descuento_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_descuento
    ADD CONSTRAINT tb_bi_tipos_descuento_pkey PRIMARY KEY (id_tipo_descuento);

ALTER TABLE ONLY nomina.tb_bi_tipos_exencion
    ADD CONSTRAINT tb_bi_tipos_exencion_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_exencion
    ADD CONSTRAINT tb_bi_tipos_exencion_pkey PRIMARY KEY (id_tipo_exencion);

ALTER TABLE ONLY nomina.tb_bi_tipos_indicador
    ADD CONSTRAINT tb_bi_tipos_indicador_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_indicador
    ADD CONSTRAINT tb_bi_tipos_indicador_pkey PRIMARY KEY (id_tipo_indicador);

ALTER TABLE ONLY nomina.tb_bi_tipos_jornada
    ADD CONSTRAINT tb_bi_tipos_jornada_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_jornada
    ADD CONSTRAINT tb_bi_tipos_jornada_pkey PRIMARY KEY (id_tipo_jornada);

ALTER TABLE ONLY nomina.tb_bi_tipos_movimiento_imss
    ADD CONSTRAINT tb_bi_tipos_movimiento_imss_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_movimiento_imss
    ADD CONSTRAINT tb_bi_tipos_movimiento_imss_pkey PRIMARY KEY (id_tipo_movimiento_imss);

ALTER TABLE ONLY nomina.tb_bi_tipos_nomina
    ADD CONSTRAINT tb_bi_tipos_nomina_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_nomina
    ADD CONSTRAINT tb_bi_tipos_nomina_pkey PRIMARY KEY (id_tipo_nomina);

ALTER TABLE ONLY nomina.tb_bi_tipos_regimen
    ADD CONSTRAINT tb_bi_tipos_regimen_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_tipos_regimen
    ADD CONSTRAINT tb_bi_tipos_regimen_pkey PRIMARY KEY (id_tipo_regimen);

ALTER TABLE ONLY nomina.tb_bi_zonas_salario_minimo
    ADD CONSTRAINT tb_bi_zonas_salario_minimo_clave_key UNIQUE (clave);

ALTER TABLE ONLY nomina.tb_bi_zonas_salario_minimo
    ADD CONSTRAINT tb_bi_zonas_salario_minimo_pkey PRIMARY KEY (id_zona_salario_minimo);

ALTER TABLE ONLY nomina.tb_bi_cesantia_vejez_patron
    ADD CONSTRAINT ux_cesantia_vejez UNIQUE (anio, veces_uma_desde);

ALTER TABLE ONLY nomina.tb_bi_claves_sat_concepto
    ADD CONSTRAINT ux_claves_sat_concepto UNIQUE (id_tipo_concepto, clave);

ALTER TABLE ONLY nomina.tb_bi_cuotas_imss
    ADD CONSTRAINT ux_cuotas_imss UNIQUE (clave_ramo, fecha_inicio);

ALTER TABLE ONLY nomina.tb_bi_esquemas_prestaciones
    ADD CONSTRAINT ux_esquemas UNIQUE (id_empresa, nombre_esquema);

ALTER TABLE ONLY nomina.tb_bi_indicadores
    ADD CONSTRAINT ux_indicadores UNIQUE (id_tipo_indicador, fecha_inicio);

ALTER TABLE ONLY nomina.tb_bi_nominas
    ADD CONSTRAINT ux_nominas UNIQUE (id_periodo_nomina, id_empleado);

ALTER TABLE ONLY nomina.tb_bi_periodos_nomina
    ADD CONSTRAINT ux_periodos_numero UNIQUE (id_empresa, id_tipo_nomina, id_periodicidad_pago, ejercicio, numero_periodo);

ALTER TABLE ONLY nomina.tb_bi_primas_riesgo_trabajo
    ADD CONSTRAINT ux_primas_riesgo UNIQUE (id_registro_patronal, fecha_inicio);

ALTER TABLE ONLY nomina.tb_bi_ptu
    ADD CONSTRAINT ux_ptu UNIQUE (id_empresa, ejercicio);

ALTER TABLE ONLY nomina.tb_bi_ptu_empleados
    ADD CONSTRAINT ux_ptu_empleados UNIQUE (id_ptu, id_empleado);

ALTER TABLE ONLY nomina.tb_bi_tarifas_isr
    ADD CONSTRAINT ux_tarifas_isr UNIQUE (anio, periodicidad, limite_inferior);

ALTER TABLE ONLY recursos_humanos.tb_bi_acciones_directorio
    ADD CONSTRAINT tb_bi_acciones_directorio_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_acciones_directorio
    ADD CONSTRAINT tb_bi_acciones_directorio_pkey PRIMARY KEY (id_accion_directorio);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencia_diaria
    ADD CONSTRAINT tb_bi_asistencia_diaria_pkey PRIMARY KEY (id_asistencia_diaria);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencias
    ADD CONSTRAINT tb_bi_asistencias_pkey PRIMARY KEY (id_asistencia);

ALTER TABLE ONLY recursos_humanos.tb_bi_auditoria_logs
    ADD CONSTRAINT tb_bi_auditoria_logs_pkey PRIMARY KEY (id_log);

ALTER TABLE ONLY recursos_humanos.tb_bi_bajas_laborales
    ADD CONSTRAINT tb_bi_bajas_laborales_pkey PRIMARY KEY (id_baja);

ALTER TABLE ONLY recursos_humanos.tb_bi_campus
    ADD CONSTRAINT tb_bi_campus_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_campus
    ADD CONSTRAINT tb_bi_campus_pkey PRIMARY KEY (id_campus);

ALTER TABLE ONLY recursos_humanos.tb_bi_causas_baja_imss
    ADD CONSTRAINT tb_bi_causas_baja_imss_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_causas_baja_imss
    ADD CONSTRAINT tb_bi_causas_baja_imss_pkey PRIMARY KEY (id_causa_baja_imss);

ALTER TABLE ONLY recursos_humanos.tb_bi_checadores
    ADD CONSTRAINT tb_bi_checadores_numero_serie_key UNIQUE (numero_serie);

ALTER TABLE ONLY recursos_humanos.tb_bi_checadores
    ADD CONSTRAINT tb_bi_checadores_pkey PRIMARY KEY (id_checador);

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT tb_bi_cuentas_directorio_correo_institucional_key UNIQUE (correo_institucional);

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT tb_bi_cuentas_directorio_entra_object_id_key UNIQUE (entra_object_id);

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT tb_bi_cuentas_directorio_id_empleado_key UNIQUE (id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT tb_bi_cuentas_directorio_object_guid_key UNIQUE (object_guid);

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT tb_bi_cuentas_directorio_pkey PRIMARY KEY (id_cuenta_directorio);

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT tb_bi_cuentas_directorio_sam_account_name_key UNIQUE (sam_account_name);

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT tb_bi_cuentas_directorio_user_principal_name_key UNIQUE (user_principal_name);

ALTER TABLE ONLY recursos_humanos.tb_bi_departamentos
    ADD CONSTRAINT tb_bi_departamentos_nombre_departamento_key UNIQUE (nombre_departamento);

ALTER TABLE ONLY recursos_humanos.tb_bi_departamentos
    ADD CONSTRAINT tb_bi_departamentos_pkey PRIMARY KEY (id_departamento);

ALTER TABLE ONLY recursos_humanos.tb_bi_dias_festivos
    ADD CONSTRAINT tb_bi_dias_festivos_fecha_key UNIQUE (fecha);

ALTER TABLE ONLY recursos_humanos.tb_bi_dias_festivos
    ADD CONSTRAINT tb_bi_dias_festivos_pkey PRIMARY KEY (id_dia_festivo);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_matricula_key UNIQUE (matricula);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_numero_checador_key UNIQUE (numero_checador);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_pkey PRIMARY KEY (id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_empresas
    ADD CONSTRAINT tb_bi_empresas_pkey PRIMARY KEY (id_empresa);

ALTER TABLE ONLY recursos_humanos.tb_bi_empresas
    ADD CONSTRAINT tb_bi_empresas_rfc_key UNIQUE (rfc);

ALTER TABLE ONLY recursos_humanos.tb_bi_estatus_asistencia
    ADD CONSTRAINT tb_bi_estatus_asistencia_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_estatus_asistencia
    ADD CONSTRAINT tb_bi_estatus_asistencia_pkey PRIMARY KEY (id_estatus_asistencia);

ALTER TABLE ONLY recursos_humanos.tb_bi_estatus_laborales
    ADD CONSTRAINT tb_bi_estatus_laborales_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_estatus_laborales
    ADD CONSTRAINT tb_bi_estatus_laborales_pkey PRIMARY KEY (id_estatus_laboral);

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_horarios
    ADD CONSTRAINT tb_bi_historial_horarios_pkey PRIMARY KEY (id_historial_horario);

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_sueldos
    ADD CONSTRAINT tb_bi_historial_sueldos_pkey PRIMARY KEY (id_historial_sueldo);

ALTER TABLE ONLY recursos_humanos.tb_bi_incapacidades_imss
    ADD CONSTRAINT tb_bi_incapacidades_imss_folio_imss_key UNIQUE (folio_imss);

ALTER TABLE ONLY recursos_humanos.tb_bi_incapacidades_imss
    ADD CONSTRAINT tb_bi_incapacidades_imss_pkey PRIMARY KEY (id_incapacidad);

ALTER TABLE ONLY recursos_humanos.tb_bi_lft_vacaciones
    ADD CONSTRAINT tb_bi_lft_vacaciones_anio_antiguedad_key UNIQUE (anio_antiguedad);

ALTER TABLE ONLY recursos_humanos.tb_bi_lft_vacaciones
    ADD CONSTRAINT tb_bi_lft_vacaciones_pkey PRIMARY KEY (id_lft_vacacion);

ALTER TABLE ONLY recursos_humanos.tb_bi_materias
    ADD CONSTRAINT tb_bi_materias_pkey PRIMARY KEY (id_materia);

ALTER TABLE ONLY recursos_humanos.tb_bi_modos_checada
    ADD CONSTRAINT tb_bi_modos_checada_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_modos_checada
    ADD CONSTRAINT tb_bi_modos_checada_pkey PRIMARY KEY (id_modo_checada);

ALTER TABLE ONLY recursos_humanos.tb_bi_motivos_baja
    ADD CONSTRAINT tb_bi_motivos_baja_descripcion_motivo_key UNIQUE (descripcion_motivo);

ALTER TABLE ONLY recursos_humanos.tb_bi_motivos_baja
    ADD CONSTRAINT tb_bi_motivos_baja_pkey PRIMARY KEY (id_motivo_baja);

ALTER TABLE ONLY recursos_humanos.tb_bi_origenes_checada
    ADD CONSTRAINT tb_bi_origenes_checada_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_origenes_checada
    ADD CONSTRAINT tb_bi_origenes_checada_pkey PRIMARY KEY (id_origen_checada);

ALTER TABLE ONLY recursos_humanos.tb_bi_perfiles
    ADD CONSTRAINT tb_bi_perfiles_nombre_perfil_key UNIQUE (nombre_perfil);

ALTER TABLE ONLY recursos_humanos.tb_bi_perfiles
    ADD CONSTRAINT tb_bi_perfiles_pkey PRIMARY KEY (id_perfil);

ALTER TABLE ONLY recursos_humanos.tb_bi_periodos_acumulacion
    ADD CONSTRAINT tb_bi_periodos_acumulacion_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_periodos_acumulacion
    ADD CONSTRAINT tb_bi_periodos_acumulacion_pkey PRIMARY KEY (id_periodo_acumulacion);

ALTER TABLE ONLY recursos_humanos.tb_bi_periodos_escolares
    ADD CONSTRAINT tb_bi_periodos_escolares_nombre_periodo_key UNIQUE (nombre_periodo);

ALTER TABLE ONLY recursos_humanos.tb_bi_periodos_escolares
    ADD CONSTRAINT tb_bi_periodos_escolares_pkey PRIMARY KEY (id_periodo);

ALTER TABLE ONLY recursos_humanos.tb_bi_permisos
    ADD CONSTRAINT tb_bi_permisos_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_permisos
    ADD CONSTRAINT tb_bi_permisos_pkey PRIMARY KEY (id_permiso);

ALTER TABLE ONLY recursos_humanos.tb_bi_personas
    ADD CONSTRAINT tb_bi_personas_curp_key UNIQUE (curp);

ALTER TABLE ONLY recursos_humanos.tb_bi_personas
    ADD CONSTRAINT tb_bi_personas_pkey PRIMARY KEY (id_persona);

ALTER TABLE ONLY recursos_humanos.tb_bi_personas
    ADD CONSTRAINT tb_bi_personas_rfc_key UNIQUE (rfc);

ALTER TABLE ONLY recursos_humanos.tb_bi_proveedores_autenticacion
    ADD CONSTRAINT tb_bi_proveedores_autenticacion_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_proveedores_autenticacion
    ADD CONSTRAINT tb_bi_proveedores_autenticacion_pkey PRIMARY KEY (id_proveedor_autenticacion);

ALTER TABLE ONLY recursos_humanos.tb_bi_puestos
    ADD CONSTRAINT tb_bi_puestos_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_puestos
    ADD CONSTRAINT tb_bi_puestos_nombre_puesto_key UNIQUE (nombre_puesto);

ALTER TABLE ONLY recursos_humanos.tb_bi_puestos
    ADD CONSTRAINT tb_bi_puestos_pkey PRIMARY KEY (id_puesto);

ALTER TABLE ONLY recursos_humanos.tb_bi_regimenes_fiscales
    ADD CONSTRAINT tb_bi_regimenes_fiscales_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_regimenes_fiscales
    ADD CONSTRAINT tb_bi_regimenes_fiscales_pkey PRIMARY KEY (id_regimen_fiscal);

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_asistencia
    ADD CONSTRAINT tb_bi_reglas_asistencia_pkey PRIMARY KEY (id_regla_asistencia);

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_prima_antiguedad
    ADD CONSTRAINT tb_bi_reglas_prima_antiguedad_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_prima_antiguedad
    ADD CONSTRAINT tb_bi_reglas_prima_antiguedad_pkey PRIMARY KEY (id_regla_prima_antiguedad);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_grupos_directorio
    ADD CONSTRAINT tb_bi_roles_grupos_directorio_pkey PRIMARY KEY (id_rol_grupo_directorio);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles
    ADD CONSTRAINT tb_bi_roles_nombre_rol_key UNIQUE (nombre_rol);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_permisos
    ADD CONSTRAINT tb_bi_roles_permisos_pkey PRIMARY KEY (id_rol_permiso);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles
    ADD CONSTRAINT tb_bi_roles_pkey PRIMARY KEY (id_rol);

ALTER TABLE ONLY recursos_humanos.tb_bi_saldos_vacaciones
    ADD CONSTRAINT tb_bi_saldos_vacaciones_pkey PRIMARY KEY (id_saldo_vacacion);

ALTER TABLE ONLY recursos_humanos.tb_bi_sincronizacion_directorio_logs
    ADD CONSTRAINT tb_bi_sincronizacion_directorio_logs_pkey PRIMARY KEY (id_sincronizacion_log);

ALTER TABLE ONLY recursos_humanos.tb_bi_solicitudes
    ADD CONSTRAINT tb_bi_solicitudes_pkey PRIMARY KEY (id_solicitud);

ALTER TABLE ONLY recursos_humanos.tb_bi_suplencias
    ADD CONSTRAINT tb_bi_suplencias_pkey PRIMARY KEY (id_suplencia);

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_duracion_permiso
    ADD CONSTRAINT tb_bi_tipos_duracion_permiso_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_duracion_permiso
    ADD CONSTRAINT tb_bi_tipos_duracion_permiso_pkey PRIMARY KEY (id_tipo_duracion_permiso);

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_incapacidad
    ADD CONSTRAINT tb_bi_tipos_incapacidad_clave_key UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_incapacidad
    ADD CONSTRAINT tb_bi_tipos_incapacidad_pkey PRIMARY KEY (id_tipo_incapacidad);

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_permiso
    ADD CONSTRAINT tb_bi_tipos_permiso_descripcion_key UNIQUE (descripcion);

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_permiso
    ADD CONSTRAINT tb_bi_tipos_permiso_pkey PRIMARY KEY (id_tipo_permiso);

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT tb_bi_usuarios_id_empleado_key UNIQUE (id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT tb_bi_usuarios_id_externo_key UNIQUE (id_externo);

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT tb_bi_usuarios_nombre_usuario_key UNIQUE (nombre_usuario);

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT tb_bi_usuarios_pkey PRIMARY KEY (id_usuario);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencia_diaria
    ADD CONSTRAINT ux_asistencia_diaria UNIQUE (id_historial_horario, fecha_dia);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencias
    ADD CONSTRAINT ux_asistencias_evento UNIQUE (id_checador, numero_evento_hikvision);

ALTER TABLE ONLY recursos_humanos.tb_bi_campus
    ADD CONSTRAINT ux_campus_nombre UNIQUE (id_empresa, nombre_campus);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT ux_empleados_persona_empresa UNIQUE (id_persona, id_empresa);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles
    ADD CONSTRAINT ux_roles_clave UNIQUE (clave);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_grupos_directorio
    ADD CONSTRAINT ux_roles_grupos UNIQUE (id_rol, nombre_grupo);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_permisos
    ADD CONSTRAINT ux_roles_permisos UNIQUE (id_rol, id_permiso);

ALTER TABLE ONLY recursos_humanos.tb_bi_saldos_vacaciones
    ADD CONSTRAINT ux_saldos_vacaciones UNIQUE (id_empleado, anio_servicio);

ALTER TABLE ONLY recursos_humanos.tb_bi_suplencias
    ADD CONSTRAINT ux_suplencias_clase UNIQUE (id_historial_horario, fecha_suplencia);

CREATE INDEX ix_movimientos_imss_empleado ON nomina.tb_bi_movimientos_imss USING btree (id_empleado, fecha_movimiento DESC);

CREATE INDEX ix_nomina_cuotas_nomina ON nomina.tb_bi_nomina_cuotas_imss USING btree (id_nomina);

CREATE INDEX ix_nomina_detalle_nomina ON nomina.tb_bi_nomina_detalle USING btree (id_nomina);

CREATE INDEX ix_nominas_empleado ON nomina.tb_bi_nominas USING btree (id_empleado);

CREATE INDEX ix_asistencia_diaria_empleado_fecha ON recursos_humanos.tb_bi_asistencia_diaria USING btree (id_empleado, fecha_dia);

CREATE INDEX ix_asistencias_empleado_fecha ON recursos_humanos.tb_bi_asistencias USING btree (id_empleado, fecha_hora_registro);

CREATE INDEX ix_auditoria_tabla_registro ON recursos_humanos.tb_bi_auditoria_logs USING btree (tabla_afectada, id_registro);

CREATE INDEX ix_auditoria_usuario_fecha ON recursos_humanos.tb_bi_auditoria_logs USING btree (id_usuario, fecha_accion DESC);

CREATE INDEX ix_historial_horarios_empleado_dia ON recursos_humanos.tb_bi_historial_horarios USING btree (id_empleado, dia_semana);

CREATE INDEX ix_incapacidades_empleado ON recursos_humanos.tb_bi_incapacidades_imss USING btree (id_empleado, fecha_inicio, fecha_fin);

CREATE INDEX ix_sincronizacion_logs_empleado ON recursos_humanos.tb_bi_sincronizacion_directorio_logs USING btree (id_empleado, fecha_sincronizacion DESC);

CREATE INDEX ix_solicitudes_empleado_fechas ON recursos_humanos.tb_bi_solicitudes USING btree (id_empleado, fecha_inicio, fecha_fin);

CREATE INDEX ix_solicitudes_pendientes ON recursos_humanos.tb_bi_solicitudes USING btree (estatus_aprobacion) WHERE ((estatus_aprobacion)::text = ANY ((ARRAY['pendiente'::character varying, 'aprobada_jefe'::character varying])::text[]));

CREATE UNIQUE INDEX ux_historial_sueldos_vigente ON recursos_humanos.tb_bi_historial_sueldos USING btree (id_empleado) WHERE (fecha_fin_vigencia IS NULL);

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_bancos FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_banco');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_cesantia_vejez_patron FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_cesantia_vejez');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_clases_riesgo FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_clase_riesgo');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_claves_sat_concepto FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_clave_sat_concepto');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_conceptos_hist FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_hist_concepto');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_conceptos_nomina FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_concepto');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_conceptos_recurrentes FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_concepto_recurrente');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_creditos_fonacot FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_credito_fonacot');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_creditos_infonavit FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_credito_infonavit');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_cuotas_imss FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_cuota_imss');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_empleados_nomina FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_empleado_nomina');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_esquemas_prestaciones FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_esquema_prestaciones');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_estados FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_estado');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_incidencias_nomina FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_incidencia_nomina');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_indicadores FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_indicador');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_metodos_pago FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_metodo_pago');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_movimientos_imss FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_movimiento_imss');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_pensiones_alimenticias FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_pension');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_periodicidades_pago FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_periodicidad_pago');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_periodos_nomina FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_periodo_nomina');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_primas_riesgo_trabajo FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_prima_riesgo');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_ptu FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_ptu');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_registros_patronales FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_registro_patronal');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_subsidio_empleo FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_subsidio');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tarifas_isr FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tarifa_isr');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_concepto FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_concepto');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_contrato FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_contrato');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_descuento FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_descuento');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_exencion FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_exencion');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_indicador FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_indicador');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_jornada FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_jornada');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_movimiento_imss FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_movimiento_imss');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_nomina FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_nomina');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_tipos_regimen FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_regimen');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON nomina.tb_bi_zonas_salario_minimo FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_zona_salario_minimo');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_acciones_directorio FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_accion_directorio');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_bajas_laborales FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_baja');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_campus FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_campus');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_causas_baja_imss FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_causa_baja_imss');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_checadores FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_checador');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_cuentas_directorio FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_cuenta_directorio');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_departamentos FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_departamento');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_dias_festivos FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_dia_festivo');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_empleados FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_empleado');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_empresas FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_empresa');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_estatus_asistencia FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_estatus_asistencia');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_estatus_laborales FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_estatus_laboral');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_historial_horarios FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_historial_horario');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_historial_sueldos FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_historial_sueldo');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_incapacidades_imss FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_incapacidad');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_lft_vacaciones FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_lft_vacacion');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_materias FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_materia');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_modos_checada FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_modo_checada');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_motivos_baja FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_motivo_baja');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_origenes_checada FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_origen_checada');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_perfiles FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_perfil');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_periodos_acumulacion FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_periodo_acumulacion');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_periodos_escolares FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_periodo');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_permisos FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_permiso');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_personas FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_persona');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_proveedores_autenticacion FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_proveedor_autenticacion');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_puestos FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_puesto');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_regimenes_fiscales FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_regimen_fiscal');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_reglas_asistencia FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_regla_asistencia');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_reglas_prima_antiguedad FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_regla_prima_antiguedad');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_roles FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_rol');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_roles_grupos_directorio FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_rol_grupo_directorio');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_roles_permisos FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_rol_permiso');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_saldos_vacaciones FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_saldo_vacacion');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_solicitudes FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_solicitud');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_suplencias FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_suplencia');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_tipos_duracion_permiso FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_duracion_permiso');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_tipos_incapacidad FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_incapacidad');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_tipos_permiso FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_tipo_permiso');

CREATE TRIGGER tr_auditoria AFTER INSERT OR DELETE OR UPDATE ON recursos_humanos.tb_bi_usuarios FOR EACH ROW EXECUTE FUNCTION recursos_humanos.fn_auditar('id_usuario');

ALTER TABLE ONLY nomina.tb_bi_bancos
    ADD CONSTRAINT fk_nom_bancos_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_bancos
    ADD CONSTRAINT fk_nom_bancos_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_cesantia_vejez_patron
    ADD CONSTRAINT fk_nom_cesantia_vejez_patron_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_cesantia_vejez_patron
    ADD CONSTRAINT fk_nom_cesantia_vejez_patron_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_clases_riesgo
    ADD CONSTRAINT fk_nom_clases_riesgo_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_clases_riesgo
    ADD CONSTRAINT fk_nom_clases_riesgo_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_claves_sat_concepto
    ADD CONSTRAINT fk_nom_claves_sat_concepto_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_claves_sat_concepto
    ADD CONSTRAINT fk_nom_claves_sat_concepto_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_conceptos_hist
    ADD CONSTRAINT fk_nom_conceptos_hist_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_conceptos_hist
    ADD CONSTRAINT fk_nom_conceptos_hist_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_conceptos_nomina
    ADD CONSTRAINT fk_nom_conceptos_nomina_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_conceptos_nomina
    ADD CONSTRAINT fk_nom_conceptos_nomina_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_conceptos_recurrentes
    ADD CONSTRAINT fk_nom_conceptos_recurrentes_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_conceptos_recurrentes
    ADD CONSTRAINT fk_nom_conceptos_recurrentes_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_creditos_fonacot
    ADD CONSTRAINT fk_nom_creditos_fonacot_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_creditos_fonacot
    ADD CONSTRAINT fk_nom_creditos_fonacot_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_creditos_infonavit
    ADD CONSTRAINT fk_nom_creditos_infonavit_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_creditos_infonavit
    ADD CONSTRAINT fk_nom_creditos_infonavit_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_cuotas_imss
    ADD CONSTRAINT fk_nom_cuotas_imss_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_cuotas_imss
    ADD CONSTRAINT fk_nom_cuotas_imss_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT fk_nom_empleados_nomina_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT fk_nom_empleados_nomina_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_esquemas_prestaciones
    ADD CONSTRAINT fk_nom_esquemas_prestaciones_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_esquemas_prestaciones
    ADD CONSTRAINT fk_nom_esquemas_prestaciones_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_estados
    ADD CONSTRAINT fk_nom_estados_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_estados
    ADD CONSTRAINT fk_nom_estados_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_incidencias_nomina
    ADD CONSTRAINT fk_nom_incidencias_nomina_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_incidencias_nomina
    ADD CONSTRAINT fk_nom_incidencias_nomina_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_indicadores
    ADD CONSTRAINT fk_nom_indicadores_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_indicadores
    ADD CONSTRAINT fk_nom_indicadores_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_metodos_pago
    ADD CONSTRAINT fk_nom_metodos_pago_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_metodos_pago
    ADD CONSTRAINT fk_nom_metodos_pago_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_movimientos_imss
    ADD CONSTRAINT fk_nom_movimientos_imss_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_movimientos_imss
    ADD CONSTRAINT fk_nom_movimientos_imss_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_nomina_cuotas_imss
    ADD CONSTRAINT fk_nom_nomina_cuotas_imss_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_nomina_cuotas_imss
    ADD CONSTRAINT fk_nom_nomina_cuotas_imss_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT fk_nom_nomina_detalle_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT fk_nom_nomina_detalle_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_nominas
    ADD CONSTRAINT fk_nom_nominas_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_nominas
    ADD CONSTRAINT fk_nom_nominas_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_pensiones_alimenticias
    ADD CONSTRAINT fk_nom_pensiones_alimenticias_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_pensiones_alimenticias
    ADD CONSTRAINT fk_nom_pensiones_alimenticias_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_periodicidades_pago
    ADD CONSTRAINT fk_nom_periodicidades_pago_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_periodicidades_pago
    ADD CONSTRAINT fk_nom_periodicidades_pago_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_periodos_nomina
    ADD CONSTRAINT fk_nom_periodos_nomina_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_periodos_nomina
    ADD CONSTRAINT fk_nom_periodos_nomina_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_primas_riesgo_trabajo
    ADD CONSTRAINT fk_nom_primas_riesgo_trabajo_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_primas_riesgo_trabajo
    ADD CONSTRAINT fk_nom_primas_riesgo_trabajo_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_ptu_empleados
    ADD CONSTRAINT fk_nom_ptu_empleados_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_ptu_empleados
    ADD CONSTRAINT fk_nom_ptu_empleados_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_ptu
    ADD CONSTRAINT fk_nom_ptu_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_ptu
    ADD CONSTRAINT fk_nom_ptu_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_registros_patronales
    ADD CONSTRAINT fk_nom_registros_patronales_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_registros_patronales
    ADD CONSTRAINT fk_nom_registros_patronales_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_subsidio_empleo
    ADD CONSTRAINT fk_nom_subsidio_empleo_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_subsidio_empleo
    ADD CONSTRAINT fk_nom_subsidio_empleo_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tarifas_isr
    ADD CONSTRAINT fk_nom_tarifas_isr_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tarifas_isr
    ADD CONSTRAINT fk_nom_tarifas_isr_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_concepto
    ADD CONSTRAINT fk_nom_tipos_concepto_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_concepto
    ADD CONSTRAINT fk_nom_tipos_concepto_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_contrato
    ADD CONSTRAINT fk_nom_tipos_contrato_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_contrato
    ADD CONSTRAINT fk_nom_tipos_contrato_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_descuento
    ADD CONSTRAINT fk_nom_tipos_descuento_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_descuento
    ADD CONSTRAINT fk_nom_tipos_descuento_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_exencion
    ADD CONSTRAINT fk_nom_tipos_exencion_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_exencion
    ADD CONSTRAINT fk_nom_tipos_exencion_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_indicador
    ADD CONSTRAINT fk_nom_tipos_indicador_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_indicador
    ADD CONSTRAINT fk_nom_tipos_indicador_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_jornada
    ADD CONSTRAINT fk_nom_tipos_jornada_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_jornada
    ADD CONSTRAINT fk_nom_tipos_jornada_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_movimiento_imss
    ADD CONSTRAINT fk_nom_tipos_movimiento_imss_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_movimiento_imss
    ADD CONSTRAINT fk_nom_tipos_movimiento_imss_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_nomina
    ADD CONSTRAINT fk_nom_tipos_nomina_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_nomina
    ADD CONSTRAINT fk_nom_tipos_nomina_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_regimen
    ADD CONSTRAINT fk_nom_tipos_regimen_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_tipos_regimen
    ADD CONSTRAINT fk_nom_tipos_regimen_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_zonas_salario_minimo
    ADD CONSTRAINT fk_nom_zonas_salario_minimo_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_zonas_salario_minimo
    ADD CONSTRAINT fk_nom_zonas_salario_minimo_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY nomina.tb_bi_claves_sat_concepto
    ADD CONSTRAINT tb_bi_claves_sat_concepto_id_tipo_concepto_fkey FOREIGN KEY (id_tipo_concepto) REFERENCES nomina.tb_bi_tipos_concepto(id_tipo_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_hist
    ADD CONSTRAINT tb_bi_conceptos_hist_id_concepto_fkey FOREIGN KEY (id_concepto) REFERENCES nomina.tb_bi_conceptos_nomina(id_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_hist
    ADD CONSTRAINT tb_bi_conceptos_hist_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_conceptos_nomina
    ADD CONSTRAINT tb_bi_conceptos_nomina_id_clave_sat_concepto_fkey FOREIGN KEY (id_clave_sat_concepto) REFERENCES nomina.tb_bi_claves_sat_concepto(id_clave_sat_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_nomina
    ADD CONSTRAINT tb_bi_conceptos_nomina_id_tipo_concepto_fkey FOREIGN KEY (id_tipo_concepto) REFERENCES nomina.tb_bi_tipos_concepto(id_tipo_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_nomina
    ADD CONSTRAINT tb_bi_conceptos_nomina_id_tipo_exencion_fkey FOREIGN KEY (id_tipo_exencion) REFERENCES nomina.tb_bi_tipos_exencion(id_tipo_exencion);

ALTER TABLE ONLY nomina.tb_bi_conceptos_recurrentes
    ADD CONSTRAINT tb_bi_conceptos_recurrentes_id_concepto_fkey FOREIGN KEY (id_concepto) REFERENCES nomina.tb_bi_conceptos_nomina(id_concepto);

ALTER TABLE ONLY nomina.tb_bi_conceptos_recurrentes
    ADD CONSTRAINT tb_bi_conceptos_recurrentes_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_creditos_fonacot
    ADD CONSTRAINT tb_bi_creditos_fonacot_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_creditos_infonavit
    ADD CONSTRAINT tb_bi_creditos_infonavit_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_creditos_infonavit
    ADD CONSTRAINT tb_bi_creditos_infonavit_id_tipo_descuento_fkey FOREIGN KEY (id_tipo_descuento) REFERENCES nomina.tb_bi_tipos_descuento(id_tipo_descuento);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_banco_fkey FOREIGN KEY (id_banco) REFERENCES nomina.tb_bi_bancos(id_banco);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_clase_riesgo_puesto_fkey FOREIGN KEY (id_clase_riesgo_puesto) REFERENCES nomina.tb_bi_clases_riesgo(id_clase_riesgo);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_esquema_prestaciones_fkey FOREIGN KEY (id_esquema_prestaciones) REFERENCES nomina.tb_bi_esquemas_prestaciones(id_esquema_prestaciones);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_metodo_pago_fkey FOREIGN KEY (id_metodo_pago) REFERENCES nomina.tb_bi_metodos_pago(id_metodo_pago);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_periodicidad_pago_fkey FOREIGN KEY (id_periodicidad_pago) REFERENCES nomina.tb_bi_periodicidades_pago(id_periodicidad_pago);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_registro_patronal_fkey FOREIGN KEY (id_registro_patronal) REFERENCES nomina.tb_bi_registros_patronales(id_registro_patronal);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_tipo_contrato_fkey FOREIGN KEY (id_tipo_contrato) REFERENCES nomina.tb_bi_tipos_contrato(id_tipo_contrato);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_tipo_jornada_fkey FOREIGN KEY (id_tipo_jornada) REFERENCES nomina.tb_bi_tipos_jornada(id_tipo_jornada);

ALTER TABLE ONLY nomina.tb_bi_empleados_nomina
    ADD CONSTRAINT tb_bi_empleados_nomina_id_tipo_regimen_fkey FOREIGN KEY (id_tipo_regimen) REFERENCES nomina.tb_bi_tipos_regimen(id_tipo_regimen);

ALTER TABLE ONLY nomina.tb_bi_esquemas_prestaciones
    ADD CONSTRAINT tb_bi_esquemas_prestaciones_id_empresa_fkey FOREIGN KEY (id_empresa) REFERENCES recursos_humanos.tb_bi_empresas(id_empresa);

ALTER TABLE ONLY nomina.tb_bi_incidencias_nomina
    ADD CONSTRAINT tb_bi_incidencias_nomina_id_concepto_fkey FOREIGN KEY (id_concepto) REFERENCES nomina.tb_bi_conceptos_nomina(id_concepto);

ALTER TABLE ONLY nomina.tb_bi_incidencias_nomina
    ADD CONSTRAINT tb_bi_incidencias_nomina_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_incidencias_nomina
    ADD CONSTRAINT tb_bi_incidencias_nomina_id_periodo_nomina_fkey FOREIGN KEY (id_periodo_nomina) REFERENCES nomina.tb_bi_periodos_nomina(id_periodo_nomina);

ALTER TABLE ONLY nomina.tb_bi_indicadores
    ADD CONSTRAINT tb_bi_indicadores_id_tipo_indicador_fkey FOREIGN KEY (id_tipo_indicador) REFERENCES nomina.tb_bi_tipos_indicador(id_tipo_indicador);

ALTER TABLE ONLY nomina.tb_bi_movimientos_imss
    ADD CONSTRAINT tb_bi_movimientos_imss_id_causa_baja_imss_fkey FOREIGN KEY (id_causa_baja_imss) REFERENCES recursos_humanos.tb_bi_causas_baja_imss(id_causa_baja_imss);

ALTER TABLE ONLY nomina.tb_bi_movimientos_imss
    ADD CONSTRAINT tb_bi_movimientos_imss_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_movimientos_imss
    ADD CONSTRAINT tb_bi_movimientos_imss_id_tipo_movimiento_imss_fkey FOREIGN KEY (id_tipo_movimiento_imss) REFERENCES nomina.tb_bi_tipos_movimiento_imss(id_tipo_movimiento_imss);

ALTER TABLE ONLY nomina.tb_bi_nomina_cuotas_imss
    ADD CONSTRAINT tb_bi_nomina_cuotas_imss_id_nomina_fkey FOREIGN KEY (id_nomina) REFERENCES nomina.tb_bi_nominas(id_nomina) ON DELETE CASCADE;

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT tb_bi_nomina_detalle_id_concepto_fkey FOREIGN KEY (id_concepto) REFERENCES nomina.tb_bi_conceptos_nomina(id_concepto);

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT tb_bi_nomina_detalle_id_credito_fonacot_fkey FOREIGN KEY (id_credito_fonacot) REFERENCES nomina.tb_bi_creditos_fonacot(id_credito_fonacot);

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT tb_bi_nomina_detalle_id_credito_infonavit_fkey FOREIGN KEY (id_credito_infonavit) REFERENCES nomina.tb_bi_creditos_infonavit(id_credito_infonavit);

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT tb_bi_nomina_detalle_id_hist_concepto_fkey FOREIGN KEY (id_hist_concepto) REFERENCES nomina.tb_bi_conceptos_hist(id_hist_concepto);

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT tb_bi_nomina_detalle_id_nomina_fkey FOREIGN KEY (id_nomina) REFERENCES nomina.tb_bi_nominas(id_nomina) ON DELETE CASCADE;

ALTER TABLE ONLY nomina.tb_bi_nomina_detalle
    ADD CONSTRAINT tb_bi_nomina_detalle_id_pension_fkey FOREIGN KEY (id_pension) REFERENCES nomina.tb_bi_pensiones_alimenticias(id_pension);

ALTER TABLE ONLY nomina.tb_bi_nominas
    ADD CONSTRAINT tb_bi_nominas_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_nominas
    ADD CONSTRAINT tb_bi_nominas_id_periodo_nomina_fkey FOREIGN KEY (id_periodo_nomina) REFERENCES nomina.tb_bi_periodos_nomina(id_periodo_nomina);

ALTER TABLE ONLY nomina.tb_bi_pensiones_alimenticias
    ADD CONSTRAINT tb_bi_pensiones_alimenticias_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_pensiones_alimenticias
    ADD CONSTRAINT tb_bi_pensiones_alimenticias_id_tipo_descuento_fkey FOREIGN KEY (id_tipo_descuento) REFERENCES nomina.tb_bi_tipos_descuento(id_tipo_descuento);

ALTER TABLE ONLY nomina.tb_bi_periodos_nomina
    ADD CONSTRAINT tb_bi_periodos_nomina_id_autorizador_nomina_fkey FOREIGN KEY (id_autorizador_nomina) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_periodos_nomina
    ADD CONSTRAINT tb_bi_periodos_nomina_id_empresa_fkey FOREIGN KEY (id_empresa) REFERENCES recursos_humanos.tb_bi_empresas(id_empresa);

ALTER TABLE ONLY nomina.tb_bi_periodos_nomina
    ADD CONSTRAINT tb_bi_periodos_nomina_id_periodicidad_pago_fkey FOREIGN KEY (id_periodicidad_pago) REFERENCES nomina.tb_bi_periodicidades_pago(id_periodicidad_pago);

ALTER TABLE ONLY nomina.tb_bi_periodos_nomina
    ADD CONSTRAINT tb_bi_periodos_nomina_id_tipo_nomina_fkey FOREIGN KEY (id_tipo_nomina) REFERENCES nomina.tb_bi_tipos_nomina(id_tipo_nomina);

ALTER TABLE ONLY nomina.tb_bi_primas_riesgo_trabajo
    ADD CONSTRAINT tb_bi_primas_riesgo_trabajo_id_registro_patronal_fkey FOREIGN KEY (id_registro_patronal) REFERENCES nomina.tb_bi_registros_patronales(id_registro_patronal);

ALTER TABLE ONLY nomina.tb_bi_ptu_empleados
    ADD CONSTRAINT tb_bi_ptu_empleados_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY nomina.tb_bi_ptu_empleados
    ADD CONSTRAINT tb_bi_ptu_empleados_id_ptu_fkey FOREIGN KEY (id_ptu) REFERENCES nomina.tb_bi_ptu(id_ptu) ON DELETE CASCADE;

ALTER TABLE ONLY nomina.tb_bi_ptu
    ADD CONSTRAINT tb_bi_ptu_id_empresa_fkey FOREIGN KEY (id_empresa) REFERENCES recursos_humanos.tb_bi_empresas(id_empresa);

ALTER TABLE ONLY nomina.tb_bi_ptu
    ADD CONSTRAINT tb_bi_ptu_id_periodo_nomina_fkey FOREIGN KEY (id_periodo_nomina) REFERENCES nomina.tb_bi_periodos_nomina(id_periodo_nomina);

ALTER TABLE ONLY nomina.tb_bi_registros_patronales
    ADD CONSTRAINT tb_bi_registros_patronales_id_clase_riesgo_fkey FOREIGN KEY (id_clase_riesgo) REFERENCES nomina.tb_bi_clases_riesgo(id_clase_riesgo);

ALTER TABLE ONLY nomina.tb_bi_registros_patronales
    ADD CONSTRAINT tb_bi_registros_patronales_id_empresa_fkey FOREIGN KEY (id_empresa) REFERENCES recursos_humanos.tb_bi_empresas(id_empresa);

ALTER TABLE ONLY nomina.tb_bi_registros_patronales
    ADD CONSTRAINT tb_bi_registros_patronales_id_estado_fkey FOREIGN KEY (id_estado) REFERENCES nomina.tb_bi_estados(id_estado);

ALTER TABLE ONLY nomina.tb_bi_registros_patronales
    ADD CONSTRAINT tb_bi_registros_patronales_id_zona_salario_minimo_fkey FOREIGN KEY (id_zona_salario_minimo) REFERENCES nomina.tb_bi_zonas_salario_minimo(id_zona_salario_minimo);

ALTER TABLE ONLY nomina.tb_bi_zonas_salario_minimo
    ADD CONSTRAINT tb_bi_zonas_salario_minimo_id_tipo_indicador_fkey FOREIGN KEY (id_tipo_indicador) REFERENCES nomina.tb_bi_tipos_indicador(id_tipo_indicador);

ALTER TABLE ONLY recursos_humanos.tb_bi_acciones_directorio
    ADD CONSTRAINT fk_acciones_directorio_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_acciones_directorio
    ADD CONSTRAINT fk_acciones_directorio_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencia_diaria
    ADD CONSTRAINT fk_asistencia_diaria_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencia_diaria
    ADD CONSTRAINT fk_asistencia_diaria_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencias
    ADD CONSTRAINT fk_asistencias_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencias
    ADD CONSTRAINT fk_asistencias_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_bajas_laborales
    ADD CONSTRAINT fk_bajas_laborales_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_bajas_laborales
    ADD CONSTRAINT fk_bajas_laborales_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_bajas_laborales
    ADD CONSTRAINT fk_bajas_nomina_finiquito FOREIGN KEY (id_nomina_finiquito) REFERENCES nomina.tb_bi_nominas(id_nomina);

ALTER TABLE ONLY recursos_humanos.tb_bi_campus
    ADD CONSTRAINT fk_campus_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_campus
    ADD CONSTRAINT fk_campus_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_causas_baja_imss
    ADD CONSTRAINT fk_causas_baja_imss_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_causas_baja_imss
    ADD CONSTRAINT fk_causas_baja_imss_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_checadores
    ADD CONSTRAINT fk_checadores_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_checadores
    ADD CONSTRAINT fk_checadores_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT fk_cuentas_directorio_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT fk_cuentas_directorio_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_departamentos
    ADD CONSTRAINT fk_departamentos_responsable FOREIGN KEY (id_responsable) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_departamentos
    ADD CONSTRAINT fk_departamentos_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_departamentos
    ADD CONSTRAINT fk_departamentos_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_dias_festivos
    ADD CONSTRAINT fk_dias_festivos_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_dias_festivos
    ADD CONSTRAINT fk_dias_festivos_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT fk_empleados_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT fk_empleados_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_empresas
    ADD CONSTRAINT fk_empresas_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_empresas
    ADD CONSTRAINT fk_empresas_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_estatus_asistencia
    ADD CONSTRAINT fk_estatus_asistencia_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_estatus_asistencia
    ADD CONSTRAINT fk_estatus_asistencia_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_estatus_laborales
    ADD CONSTRAINT fk_estatus_laborales_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_estatus_laborales
    ADD CONSTRAINT fk_estatus_laborales_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_horarios
    ADD CONSTRAINT fk_historial_horarios_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_horarios
    ADD CONSTRAINT fk_historial_horarios_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_sueldos
    ADD CONSTRAINT fk_historial_sueldos_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_sueldos
    ADD CONSTRAINT fk_historial_sueldos_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_incapacidades_imss
    ADD CONSTRAINT fk_incapacidades_imss_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_incapacidades_imss
    ADD CONSTRAINT fk_incapacidades_imss_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_lft_vacaciones
    ADD CONSTRAINT fk_lft_vacaciones_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_lft_vacaciones
    ADD CONSTRAINT fk_lft_vacaciones_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_materias
    ADD CONSTRAINT fk_materias_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_materias
    ADD CONSTRAINT fk_materias_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_modos_checada
    ADD CONSTRAINT fk_modos_checada_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_modos_checada
    ADD CONSTRAINT fk_modos_checada_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_motivos_baja
    ADD CONSTRAINT fk_motivos_baja_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_motivos_baja
    ADD CONSTRAINT fk_motivos_baja_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_origenes_checada
    ADD CONSTRAINT fk_origenes_checada_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_origenes_checada
    ADD CONSTRAINT fk_origenes_checada_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_perfiles
    ADD CONSTRAINT fk_perfiles_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_perfiles
    ADD CONSTRAINT fk_perfiles_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_periodos_acumulacion
    ADD CONSTRAINT fk_periodos_acumulacion_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_periodos_acumulacion
    ADD CONSTRAINT fk_periodos_acumulacion_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_periodos_escolares
    ADD CONSTRAINT fk_periodos_escolares_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_periodos_escolares
    ADD CONSTRAINT fk_periodos_escolares_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_permisos
    ADD CONSTRAINT fk_permisos_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_permisos
    ADD CONSTRAINT fk_permisos_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_personas
    ADD CONSTRAINT fk_personas_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_personas
    ADD CONSTRAINT fk_personas_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_proveedores_autenticacion
    ADD CONSTRAINT fk_proveedores_autenticacion_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_proveedores_autenticacion
    ADD CONSTRAINT fk_proveedores_autenticacion_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_puestos
    ADD CONSTRAINT fk_puestos_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_puestos
    ADD CONSTRAINT fk_puestos_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_regimenes_fiscales
    ADD CONSTRAINT fk_regimenes_fiscales_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_regimenes_fiscales
    ADD CONSTRAINT fk_regimenes_fiscales_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_asistencia
    ADD CONSTRAINT fk_reglas_asistencia_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_asistencia
    ADD CONSTRAINT fk_reglas_asistencia_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_prima_antiguedad
    ADD CONSTRAINT fk_reglas_prima_antiguedad_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_prima_antiguedad
    ADD CONSTRAINT fk_reglas_prima_antiguedad_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_grupos_directorio
    ADD CONSTRAINT fk_roles_grupos_directorio_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_grupos_directorio
    ADD CONSTRAINT fk_roles_grupos_directorio_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_permisos
    ADD CONSTRAINT fk_roles_permisos_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_permisos
    ADD CONSTRAINT fk_roles_permisos_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_roles
    ADD CONSTRAINT fk_roles_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_roles
    ADD CONSTRAINT fk_roles_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_saldos_vacaciones
    ADD CONSTRAINT fk_saldos_vacaciones_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_saldos_vacaciones
    ADD CONSTRAINT fk_saldos_vacaciones_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_sincronizacion_directorio_logs
    ADD CONSTRAINT fk_sincronizacion_directorio_logs_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_sincronizacion_directorio_logs
    ADD CONSTRAINT fk_sincronizacion_directorio_logs_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_solicitudes
    ADD CONSTRAINT fk_solicitudes_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_solicitudes
    ADD CONSTRAINT fk_solicitudes_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_suplencias
    ADD CONSTRAINT fk_suplencias_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_suplencias
    ADD CONSTRAINT fk_suplencias_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_duracion_permiso
    ADD CONSTRAINT fk_tipos_duracion_permiso_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_duracion_permiso
    ADD CONSTRAINT fk_tipos_duracion_permiso_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_incapacidad
    ADD CONSTRAINT fk_tipos_incapacidad_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_incapacidad
    ADD CONSTRAINT fk_tipos_incapacidad_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_permiso
    ADD CONSTRAINT fk_tipos_permiso_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_permiso
    ADD CONSTRAINT fk_tipos_permiso_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT fk_usuarios_usuario_creacion FOREIGN KEY (id_usuario_creacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT fk_usuarios_usuario_modificacion FOREIGN KEY (id_usuario_modificacion) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencia_diaria
    ADD CONSTRAINT tb_bi_asistencia_diaria_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencia_diaria
    ADD CONSTRAINT tb_bi_asistencia_diaria_id_estatus_asistencia_fkey FOREIGN KEY (id_estatus_asistencia) REFERENCES recursos_humanos.tb_bi_estatus_asistencia(id_estatus_asistencia);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencia_diaria
    ADD CONSTRAINT tb_bi_asistencia_diaria_id_historial_horario_fkey FOREIGN KEY (id_historial_horario) REFERENCES recursos_humanos.tb_bi_historial_horarios(id_historial_horario);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencia_diaria
    ADD CONSTRAINT tb_bi_asistencia_diaria_id_solicitud_fkey FOREIGN KEY (id_solicitud) REFERENCES recursos_humanos.tb_bi_solicitudes(id_solicitud);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencias
    ADD CONSTRAINT tb_bi_asistencias_id_checador_fkey FOREIGN KEY (id_checador) REFERENCES recursos_humanos.tb_bi_checadores(id_checador);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencias
    ADD CONSTRAINT tb_bi_asistencias_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_asistencias
    ADD CONSTRAINT tb_bi_asistencias_id_origen_checada_fkey FOREIGN KEY (id_origen_checada) REFERENCES recursos_humanos.tb_bi_origenes_checada(id_origen_checada);

ALTER TABLE ONLY recursos_humanos.tb_bi_auditoria_logs
    ADD CONSTRAINT tb_bi_auditoria_logs_id_usuario_fkey FOREIGN KEY (id_usuario) REFERENCES recursos_humanos.tb_bi_usuarios(id_usuario);

ALTER TABLE ONLY recursos_humanos.tb_bi_bajas_laborales
    ADD CONSTRAINT tb_bi_bajas_laborales_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_bajas_laborales
    ADD CONSTRAINT tb_bi_bajas_laborales_id_motivo_baja_fkey FOREIGN KEY (id_motivo_baja) REFERENCES recursos_humanos.tb_bi_motivos_baja(id_motivo_baja);

ALTER TABLE ONLY recursos_humanos.tb_bi_campus
    ADD CONSTRAINT tb_bi_campus_id_empresa_fkey FOREIGN KEY (id_empresa) REFERENCES recursos_humanos.tb_bi_empresas(id_empresa);

ALTER TABLE ONLY recursos_humanos.tb_bi_campus
    ADD CONSTRAINT tb_bi_campus_id_estado_fkey FOREIGN KEY (id_estado) REFERENCES nomina.tb_bi_estados(id_estado);

ALTER TABLE ONLY recursos_humanos.tb_bi_cuentas_directorio
    ADD CONSTRAINT tb_bi_cuentas_directorio_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_id_campus_fkey FOREIGN KEY (id_campus) REFERENCES recursos_humanos.tb_bi_campus(id_campus);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_id_departamento_fkey FOREIGN KEY (id_departamento) REFERENCES recursos_humanos.tb_bi_departamentos(id_departamento);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_id_empresa_fkey FOREIGN KEY (id_empresa) REFERENCES recursos_humanos.tb_bi_empresas(id_empresa);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_id_estatus_laboral_fkey FOREIGN KEY (id_estatus_laboral) REFERENCES recursos_humanos.tb_bi_estatus_laborales(id_estatus_laboral);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_id_jefe_directo_fkey FOREIGN KEY (id_jefe_directo) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_id_perfil_fkey FOREIGN KEY (id_perfil) REFERENCES recursos_humanos.tb_bi_perfiles(id_perfil);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_id_persona_fkey FOREIGN KEY (id_persona) REFERENCES recursos_humanos.tb_bi_personas(id_persona);

ALTER TABLE ONLY recursos_humanos.tb_bi_empleados
    ADD CONSTRAINT tb_bi_empleados_id_puesto_fkey FOREIGN KEY (id_puesto) REFERENCES recursos_humanos.tb_bi_puestos(id_puesto);

ALTER TABLE ONLY recursos_humanos.tb_bi_empresas
    ADD CONSTRAINT tb_bi_empresas_id_regimen_fiscal_fkey FOREIGN KEY (id_regimen_fiscal) REFERENCES recursos_humanos.tb_bi_regimenes_fiscales(id_regimen_fiscal);

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_horarios
    ADD CONSTRAINT tb_bi_historial_horarios_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_horarios
    ADD CONSTRAINT tb_bi_historial_horarios_id_materia_fkey FOREIGN KEY (id_materia) REFERENCES recursos_humanos.tb_bi_materias(id_materia);

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_horarios
    ADD CONSTRAINT tb_bi_historial_horarios_id_periodo_fkey FOREIGN KEY (id_periodo) REFERENCES recursos_humanos.tb_bi_periodos_escolares(id_periodo);

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_horarios
    ADD CONSTRAINT tb_bi_historial_horarios_id_regla_asistencia_fkey FOREIGN KEY (id_regla_asistencia) REFERENCES recursos_humanos.tb_bi_reglas_asistencia(id_regla_asistencia);

ALTER TABLE ONLY recursos_humanos.tb_bi_historial_sueldos
    ADD CONSTRAINT tb_bi_historial_sueldos_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_incapacidades_imss
    ADD CONSTRAINT tb_bi_incapacidades_imss_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_incapacidades_imss
    ADD CONSTRAINT tb_bi_incapacidades_imss_id_tipo_incapacidad_fkey FOREIGN KEY (id_tipo_incapacidad) REFERENCES recursos_humanos.tb_bi_tipos_incapacidad(id_tipo_incapacidad);

ALTER TABLE ONLY recursos_humanos.tb_bi_motivos_baja
    ADD CONSTRAINT tb_bi_motivos_baja_id_causa_baja_imss_fkey FOREIGN KEY (id_causa_baja_imss) REFERENCES recursos_humanos.tb_bi_causas_baja_imss(id_causa_baja_imss);

ALTER TABLE ONLY recursos_humanos.tb_bi_motivos_baja
    ADD CONSTRAINT tb_bi_motivos_baja_id_regla_prima_antiguedad_fkey FOREIGN KEY (id_regla_prima_antiguedad) REFERENCES recursos_humanos.tb_bi_reglas_prima_antiguedad(id_regla_prima_antiguedad);

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_asistencia
    ADD CONSTRAINT tb_bi_reglas_asistencia_id_modo_checada_fkey FOREIGN KEY (id_modo_checada) REFERENCES recursos_humanos.tb_bi_modos_checada(id_modo_checada);

ALTER TABLE ONLY recursos_humanos.tb_bi_reglas_asistencia
    ADD CONSTRAINT tb_bi_reglas_asistencia_id_periodo_acumulacion_fkey FOREIGN KEY (id_periodo_acumulacion) REFERENCES recursos_humanos.tb_bi_periodos_acumulacion(id_periodo_acumulacion);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_grupos_directorio
    ADD CONSTRAINT tb_bi_roles_grupos_directorio_id_rol_fkey FOREIGN KEY (id_rol) REFERENCES recursos_humanos.tb_bi_roles(id_rol);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_permisos
    ADD CONSTRAINT tb_bi_roles_permisos_id_permiso_fkey FOREIGN KEY (id_permiso) REFERENCES recursos_humanos.tb_bi_permisos(id_permiso);

ALTER TABLE ONLY recursos_humanos.tb_bi_roles_permisos
    ADD CONSTRAINT tb_bi_roles_permisos_id_rol_fkey FOREIGN KEY (id_rol) REFERENCES recursos_humanos.tb_bi_roles(id_rol);

ALTER TABLE ONLY recursos_humanos.tb_bi_saldos_vacaciones
    ADD CONSTRAINT tb_bi_saldos_vacaciones_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_saldos_vacaciones
    ADD CONSTRAINT tb_bi_saldos_vacaciones_id_lft_vacacion_fkey FOREIGN KEY (id_lft_vacacion) REFERENCES recursos_humanos.tb_bi_lft_vacaciones(id_lft_vacacion);

ALTER TABLE ONLY recursos_humanos.tb_bi_sincronizacion_directorio_logs
    ADD CONSTRAINT tb_bi_sincronizacion_directorio_logs_id_accion_directorio_fkey FOREIGN KEY (id_accion_directorio) REFERENCES recursos_humanos.tb_bi_acciones_directorio(id_accion_directorio);

ALTER TABLE ONLY recursos_humanos.tb_bi_sincronizacion_directorio_logs
    ADD CONSTRAINT tb_bi_sincronizacion_directorio_logs_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_solicitudes
    ADD CONSTRAINT tb_bi_solicitudes_id_asistencia_omision_fkey FOREIGN KEY (id_asistencia_omision) REFERENCES recursos_humanos.tb_bi_asistencias(id_asistencia);

ALTER TABLE ONLY recursos_humanos.tb_bi_solicitudes
    ADD CONSTRAINT tb_bi_solicitudes_id_autorizador_jefe_fkey FOREIGN KEY (id_autorizador_jefe) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_solicitudes
    ADD CONSTRAINT tb_bi_solicitudes_id_autorizador_rh_fkey FOREIGN KEY (id_autorizador_rh) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_solicitudes
    ADD CONSTRAINT tb_bi_solicitudes_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_solicitudes
    ADD CONSTRAINT tb_bi_solicitudes_id_tipo_permiso_fkey FOREIGN KEY (id_tipo_permiso) REFERENCES recursos_humanos.tb_bi_tipos_permiso(id_tipo_permiso);

ALTER TABLE ONLY recursos_humanos.tb_bi_suplencias
    ADD CONSTRAINT tb_bi_suplencias_id_autorizador_fkey FOREIGN KEY (id_autorizador) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_suplencias
    ADD CONSTRAINT tb_bi_suplencias_id_empleado_suplente_fkey FOREIGN KEY (id_empleado_suplente) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_suplencias
    ADD CONSTRAINT tb_bi_suplencias_id_empleado_titular_fkey FOREIGN KEY (id_empleado_titular) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_suplencias
    ADD CONSTRAINT tb_bi_suplencias_id_historial_horario_fkey FOREIGN KEY (id_historial_horario) REFERENCES recursos_humanos.tb_bi_historial_horarios(id_historial_horario);

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_permiso
    ADD CONSTRAINT tb_bi_tipos_permiso_id_estatus_asistencia_fkey FOREIGN KEY (id_estatus_asistencia) REFERENCES recursos_humanos.tb_bi_estatus_asistencia(id_estatus_asistencia);

ALTER TABLE ONLY recursos_humanos.tb_bi_tipos_permiso
    ADD CONSTRAINT tb_bi_tipos_permiso_id_tipo_duracion_permiso_fkey FOREIGN KEY (id_tipo_duracion_permiso) REFERENCES recursos_humanos.tb_bi_tipos_duracion_permiso(id_tipo_duracion_permiso);

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT tb_bi_usuarios_id_empleado_fkey FOREIGN KEY (id_empleado) REFERENCES recursos_humanos.tb_bi_empleados(id_empleado);

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT tb_bi_usuarios_id_proveedor_autenticacion_fkey FOREIGN KEY (id_proveedor_autenticacion) REFERENCES recursos_humanos.tb_bi_proveedores_autenticacion(id_proveedor_autenticacion);

ALTER TABLE ONLY recursos_humanos.tb_bi_usuarios
    ADD CONSTRAINT tb_bi_usuarios_id_rol_fkey FOREIGN KEY (id_rol) REFERENCES recursos_humanos.tb_bi_roles(id_rol);

DO $$
BEGIN
    EXECUTE format('ALTER DATABASE %I SET timezone TO %L', current_database(), 'America/Mexico_City');
    EXECUTE format('REVOKE CONNECT ON DATABASE %I FROM PUBLIC', current_database());
END
$$;
