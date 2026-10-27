namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class BaseDatos
    {
        private static readonly Dictionary<string, string> MensajesPorRestriccion = new()
        {
            ["tb_bi_personas_curp_key"] = "Ya existe una persona con esa CURP. Revisa el dato.",
            ["tb_bi_personas_rfc_key"] = "Ya existe una persona con ese RFC. Revisa el dato.",
            ["tb_bi_empleados_matricula_key"] = "Ya existe un empleado con esa matrícula.",
            ["tb_bi_empleados_numero_checador_key"] = "Ese número de checador ya está asignado a otro empleado.",
            ["ux_empleados_persona_empresa"] = "Esta persona ya está registrada como empleado en esa empresa.",
            ["tb_bi_empleados_id_empresa_fkey"] = "La empresa seleccionada no existe.",
            ["tb_bi_empleados_id_departamento_fkey"] = "El departamento seleccionado no existe.",
            ["tb_bi_empleados_id_perfil_fkey"] = "El perfil seleccionado no existe.",
            ["tb_bi_empleados_id_puesto_fkey"] = "El puesto seleccionado no existe.",
            ["tb_bi_empleados_id_campus_fkey"] = "El campus seleccionado no existe.",
            ["tb_bi_empleados_id_jefe_directo_fkey"] = "El jefe directo seleccionado no existe.",
            ["tb_bi_empleados_id_estatus_laboral_fkey"] = "El estatus laboral seleccionado no existe.",
            ["ck_empleados_jefe"] = "El jefe directo debe ser otro empleado.",
            ["ck_empleados_biometricos"] = "Para asignar número de checador primero debe firmar el aviso de privacidad de biométricos.",
            ["ux_asistencias_evento"] = "Esa checada ya estaba registrada. Vuelve a enviar el lote.",
            ["tb_bi_solicitudes_id_tipo_permiso_fkey"] = "El tipo de permiso seleccionado no existe.",
            ["ck_tipos_permiso_dias_por_anio"] = "El tope de días por año debe ser mayor a cero.",
            ["ck_solicitudes_fechas"] = "La fecha final no puede ser anterior a la inicial.",
            ["ck_solicitudes_horas"] = "La hora de inicio debe ser menor a la de fin.",
            ["ck_solicitudes_rechazo"] = "Para rechazar una solicitud hay que indicar el motivo.",
            ["ck_saldos_disfrutados"] = "No hay días de vacaciones suficientes.",
            ["ck_horarios_dia"] = "El día de la semana debe ser de 1 (lunes) a 7 (domingo).",
            ["ck_horarios_horas"] = "La hora de salida debe ser mayor a la de entrada.",
            ["ck_horarios_comida"] = "La comida necesita hora de inicio y de fin, y el fin debe ser mayor.",
            ["ck_horarios_docente"] = "Un horario de clase necesita materia y periodo escolar.",
            ["ck_horarios_vigencia"] = "La fecha de fin de vigencia no puede ser anterior a la de inicio.",
            ["tb_bi_historial_horarios_id_materia_fkey"] = "La materia seleccionada no existe.",
            ["tb_bi_historial_horarios_id_periodo_fkey"] = "El periodo escolar seleccionado no existe.",
            ["tb_bi_historial_horarios_id_regla_asistencia_fkey"] = "La regla de asistencia seleccionada no existe.",
            ["tb_bi_incapacidades_imss_folio_imss_key"] = "Ya existe una incapacidad con ese folio del IMSS.",
            ["tb_bi_incapacidades_imss_id_tipo_incapacidad_fkey"] = "El tipo de incapacidad seleccionado no existe.",
            ["ck_incapacidades_fechas"] = "La fecha final no puede ser anterior a la inicial.",
            ["ux_suplencias_clase"] = "Esa clase ya tiene suplente ese día.",
            ["ck_suplencias_personas"] = "El suplente debe ser otro empleado.",
            ["tb_bi_incidencias_nomina_importe_check"] = "El importe debe ser mayor a cero.",
            ["tb_bi_incidencias_nomina_id_concepto_fkey"] = "El concepto seleccionado no existe.",
            ["ck_empleados_nomina_nss"] = "El NSS debe tener 11 dígitos.",
            ["ck_empleados_nomina_clabe"] = "La CLABE debe tener 18 dígitos.",
            ["tb_bi_empleados_nomina_id_registro_patronal_fkey"] = "El registro patronal seleccionado no existe.",
            ["tb_bi_empleados_nomina_id_esquema_prestaciones_fkey"] = "El esquema de prestaciones seleccionado no existe.",
            ["tb_bi_empleados_nomina_id_periodicidad_pago_fkey"] = "La periodicidad de pago seleccionada no existe.",
            ["tb_bi_empleados_nomina_id_tipo_contrato_fkey"] = "El tipo de contrato seleccionado no existe.",
            ["tb_bi_empleados_nomina_id_tipo_regimen_fkey"] = "El tipo de régimen seleccionado no existe.",
            ["tb_bi_empleados_nomina_id_tipo_jornada_fkey"] = "El tipo de jornada seleccionado no existe.",
            ["tb_bi_empleados_nomina_id_clase_riesgo_puesto_fkey"] = "La clase de riesgo seleccionada no existe.",
            ["tb_bi_empleados_nomina_id_metodo_pago_fkey"] = "El método de pago seleccionado no existe.",
            ["tb_bi_empleados_nomina_id_banco_fkey"] = "El banco seleccionado no existe.",
            ["ck_sueldos_monto"] = "Indica el salario diario o la tarifa por hora.",
            ["ck_sueldos_vigencia"] = "La fecha de fin no puede ser anterior a la de inicio.",
            ["ux_historial_sueldos_vigente"] = "El empleado ya tiene un sueldo vigente.",
            ["ux_periodos_numero"] = "Ya existe ese periodo de nómina para la empresa.",
            ["ck_periodos_fechas"] = "La fecha final del periodo no puede ser anterior a la inicial.",
            ["ux_ptu"] = "Ya existe el reparto de PTU de ese ejercicio para la empresa.",
            ["tb_bi_bajas_laborales_id_motivo_baja_fkey"] = "El motivo de baja seleccionado no existe.",
            ["tb_bi_creditos_infonavit_numero_credito_key"] = "Ya existe un crédito INFONAVIT con ese número.",
            ["tb_bi_creditos_infonavit_id_tipo_descuento_fkey"] = "El tipo de descuento seleccionado no existe.",
            ["tb_bi_creditos_fonacot_numero_credito_key"] = "Ya existe un crédito FONACOT con ese número.",
            ["ck_fonacot_saldo"] = "El saldo FONACOT debe estar entre cero y el importe total.",
            ["tb_bi_pensiones_alimenticias_id_tipo_descuento_fkey"] = "El tipo de descuento seleccionado no existe.",
            ["tb_bi_conceptos_hist_id_concepto_fkey"] = "El concepto seleccionado no existe.",
            ["tb_bi_conceptos_recurrentes_id_concepto_fkey"] = "El concepto seleccionado no existe.",
            ["ux_roles_clave"] = "Ya existe un rol con esa clave.",
            ["tb_bi_roles_nombre_rol_key"] = "Ya existe un rol con ese nombre.",
            ["ck_roles_permisos_alcance"] = "El alcance debe ser todos, equipo o propio.",
            ["tb_bi_usuarios_nombre_usuario_key"] = "Ya existe un usuario con ese nombre.",
            ["tb_bi_usuarios_id_empleado_key"] = "El empleado ya tiene usuario.",
            ["ck_usuarios_password"] = "Un usuario local necesita contraseña.",
            ["ck_usuarios_estatus"] = "El estatus debe ser activo, bloqueado o inactivo.",
            ["tb_bi_empresas_rfc_key"] = "Ya existe una empresa con ese RFC.",
            ["tb_bi_empresas_id_regimen_fiscal_fkey"] = "El régimen fiscal seleccionado no existe.",
            ["tb_bi_campus_clave_key"] = "Ya existe un campus con esa clave.",
            ["ux_campus_nombre"] = "La empresa ya tiene un campus con ese nombre.",
            ["tb_bi_campus_id_empresa_fkey"] = "La empresa seleccionada no existe.",
            ["tb_bi_campus_id_estado_fkey"] = "El estado seleccionado no existe.",
            ["tb_bi_registros_patronales_registro_patronal_key"] = "Ese registro patronal ya está dado de alta.",
            ["tb_bi_registros_patronales_id_empresa_fkey"] = "La empresa seleccionada no existe.",
            ["tb_bi_registros_patronales_id_clase_riesgo_fkey"] = "La clase de riesgo seleccionada no existe.",
            ["tb_bi_registros_patronales_id_zona_salario_minimo_fkey"] = "La zona de salario mínimo seleccionada no existe.",
            ["tb_bi_registros_patronales_id_estado_fkey"] = "El estado seleccionado no existe.",
            ["ux_esquemas"] = "La empresa ya tiene un esquema de prestaciones con ese nombre.",
            ["tb_bi_esquemas_prestaciones_id_empresa_fkey"] = "La empresa seleccionada no existe.",
            ["tb_bi_esquemas_prestaciones_dias_aguinaldo_check"] = "El aguinaldo es de al menos 15 días.",
            ["tb_bi_esquemas_prestaciones_porcentaje_prima_vacacional_check"] = "La prima vacacional es de al menos 25%.",
            ["tb_bi_esquemas_prestaciones_porcentaje_prima_dominical_check"] = "La prima dominical es de al menos 25%.",
            ["tb_bi_departamentos_nombre_departamento_key"] = "Ya existe un departamento con ese nombre.",
            ["fk_departamentos_responsable"] = "El responsable seleccionado no existe.",
            ["tb_bi_puestos_clave_key"] = "Ya existe un puesto con esa clave.",
            ["tb_bi_puestos_nombre_puesto_key"] = "Ya existe un puesto con ese nombre.",
            ["tb_bi_perfiles_nombre_perfil_key"] = "Ya existe un perfil con ese nombre.",
            ["tb_bi_dias_festivos_fecha_key"] = "Ya hay un día festivo en esa fecha.",
            ["tb_bi_checadores_numero_serie_key"] = "Ya existe un checador con ese número de serie.",
            ["ck_reglas_minutos"] = "Los minutos para falta deben ser iguales o mayores a la tolerancia.",
            ["ck_reglas_retardos"] = "El máximo de retardos debe ser mayor a cero.",
            ["ck_reglas_vigencia"] = "La fecha de fin de vigencia no puede ser anterior a la de inicio.",
            ["tb_bi_reglas_asistencia_id_modo_checada_fkey"] = "El modo de checada seleccionado no existe.",
            ["tb_bi_reglas_asistencia_id_periodo_acumulacion_fkey"] = "El periodo de acumulación seleccionado no existe.",
            ["tb_bi_periodos_escolares_nombre_periodo_key"] = "Ya existe un periodo escolar con ese nombre.",
            ["ck_periodos_escolares_fechas"] = "La fecha final no puede ser anterior a la inicial."
        };

        public static ReglaNegocioException ReglaEnFuncion(string mensaje) =>
            new("BD-001", mensaje);

        public static ReglaNegocioException Duplicado(string? restriccion) =>
            new("BD-002", Mensaje(restriccion, "Ya existe un registro con esos datos. Revisa la información."));

        public static ReglaNegocioException ReferenciaInvalida(string? restriccion) =>
            new("BD-003", Mensaje(restriccion, "Uno de los datos seleccionados no existe, o el registro lo usa otro. Revisa la información."));

        public static ReglaNegocioException RestriccionIncumplida(string? restriccion) =>
            new("BD-004", Mensaje(restriccion, "Los datos no cumplen una regla del sistema. Revisa la información."));

        private static string Mensaje(string? restriccion, string generico) =>
            restriccion is not null && MensajesPorRestriccion.TryGetValue(restriccion, out var mensaje) ? mensaje : generico;
    }
}
