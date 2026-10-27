using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Infrastructure.Persistence;

public partial class RecursosHumanosDbContext : DbContext
{
    public RecursosHumanosDbContext(DbContextOptions<RecursosHumanosDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccionesDirectorio> AccionesDirectorio { get; set; }

    public virtual DbSet<AsistenciaDiaria> AsistenciaDiaria { get; set; }

    public virtual DbSet<Asistencias> Asistencias { get; set; }

    public virtual DbSet<AuditoriaLogs> AuditoriaLogs { get; set; }

    public virtual DbSet<BajasLaborales> BajasLaborales { get; set; }

    public virtual DbSet<Bancos> Bancos { get; set; }

    public virtual DbSet<Campus> Campus { get; set; }

    public virtual DbSet<CausasBajaImss> CausasBajaImss { get; set; }

    public virtual DbSet<CesantiaVejezPatron> CesantiaVejezPatron { get; set; }

    public virtual DbSet<Checadores> Checadores { get; set; }

    public virtual DbSet<ClasesRiesgo> ClasesRiesgo { get; set; }

    public virtual DbSet<ClavesSatConcepto> ClavesSatConcepto { get; set; }

    public virtual DbSet<ConceptosHist> ConceptosHist { get; set; }

    public virtual DbSet<ConceptosNomina> ConceptosNomina { get; set; }

    public virtual DbSet<ConceptosRecurrentes> ConceptosRecurrentes { get; set; }

    public virtual DbSet<CreditosFonacot> CreditosFonacot { get; set; }

    public virtual DbSet<CreditosInfonavit> CreditosInfonavit { get; set; }

    public virtual DbSet<CuentasDirectorio> CuentasDirectorio { get; set; }

    public virtual DbSet<CuotasImss> CuotasImss { get; set; }

    public virtual DbSet<Departamentos> Departamentos { get; set; }

    public virtual DbSet<DiasFestivos> DiasFestivos { get; set; }

    public virtual DbSet<Empleados> Empleados { get; set; }

    public virtual DbSet<EmpleadosNomina> EmpleadosNomina { get; set; }

    public virtual DbSet<Empresas> Empresas { get; set; }

    public virtual DbSet<EsquemasPrestaciones> EsquemasPrestaciones { get; set; }

    public virtual DbSet<Estados> Estados { get; set; }

    public virtual DbSet<EstatusAsistencia> EstatusAsistencia { get; set; }

    public virtual DbSet<EstatusLaborales> EstatusLaborales { get; set; }

    public virtual DbSet<HistorialHorarios> HistorialHorarios { get; set; }

    public virtual DbSet<HistorialSueldos> HistorialSueldos { get; set; }

    public virtual DbSet<IncapacidadesImss> IncapacidadesImss { get; set; }

    public virtual DbSet<IncidenciasNomina> IncidenciasNomina { get; set; }

    public virtual DbSet<Indicadores> Indicadores { get; set; }

    public virtual DbSet<LftVacaciones> LftVacaciones { get; set; }

    public virtual DbSet<Materias> Materias { get; set; }

    public virtual DbSet<MetodosPago> MetodosPago { get; set; }

    public virtual DbSet<ModosChecada> ModosChecada { get; set; }

    public virtual DbSet<MotivosBaja> MotivosBaja { get; set; }

    public virtual DbSet<MovimientosImss> MovimientosImss { get; set; }

    public virtual DbSet<NominaCuotasImss> NominaCuotasImss { get; set; }

    public virtual DbSet<NominaDetalle> NominaDetalle { get; set; }

    public virtual DbSet<Nominas> Nominas { get; set; }

    public virtual DbSet<OrigenesChecada> OrigenesChecada { get; set; }

    public virtual DbSet<PensionesAlimenticias> PensionesAlimenticias { get; set; }

    public virtual DbSet<Perfiles> Perfiles { get; set; }

    public virtual DbSet<PeriodicidadesPago> PeriodicidadesPago { get; set; }

    public virtual DbSet<PeriodosAcumulacion> PeriodosAcumulacion { get; set; }

    public virtual DbSet<PeriodosEscolares> PeriodosEscolares { get; set; }

    public virtual DbSet<PeriodosNomina> PeriodosNomina { get; set; }

    public virtual DbSet<Permisos> Permisos { get; set; }

    public virtual DbSet<Personas> Personas { get; set; }

    public virtual DbSet<PrimasRiesgoTrabajo> PrimasRiesgoTrabajo { get; set; }

    public virtual DbSet<ProveedoresAutenticacion> ProveedoresAutenticacion { get; set; }

    public virtual DbSet<Ptu> Ptu { get; set; }

    public virtual DbSet<PtuEmpleados> PtuEmpleados { get; set; }

    public virtual DbSet<Puestos> Puestos { get; set; }

    public virtual DbSet<RegimenesFiscales> RegimenesFiscales { get; set; }

    public virtual DbSet<RegistrosPatronales> RegistrosPatronales { get; set; }

    public virtual DbSet<ReglasAsistencia> ReglasAsistencia { get; set; }

    public virtual DbSet<ReglasPrimaAntiguedad> ReglasPrimaAntiguedad { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<RolesGruposDirectorio> RolesGruposDirectorio { get; set; }

    public virtual DbSet<RolesPermisos> RolesPermisos { get; set; }

    public virtual DbSet<SaldosVacaciones> SaldosVacaciones { get; set; }

    public virtual DbSet<SincronizacionDirectorioLogs> SincronizacionDirectorioLogs { get; set; }

    public virtual DbSet<Solicitudes> Solicitudes { get; set; }

    public virtual DbSet<SubsidioEmpleo> SubsidioEmpleo { get; set; }

    public virtual DbSet<Suplencias> Suplencias { get; set; }

    public virtual DbSet<TarifasIsr> TarifasIsr { get; set; }

    public virtual DbSet<TiposConcepto> TiposConcepto { get; set; }

    public virtual DbSet<TiposContrato> TiposContrato { get; set; }

    public virtual DbSet<TiposDescuento> TiposDescuento { get; set; }

    public virtual DbSet<TiposDuracionPermiso> TiposDuracionPermiso { get; set; }

    public virtual DbSet<TiposExencion> TiposExencion { get; set; }

    public virtual DbSet<TiposIncapacidad> TiposIncapacidad { get; set; }

    public virtual DbSet<TiposIndicador> TiposIndicador { get; set; }

    public virtual DbSet<TiposJornada> TiposJornada { get; set; }

    public virtual DbSet<TiposMovimientoImss> TiposMovimientoImss { get; set; }

    public virtual DbSet<TiposNomina> TiposNomina { get; set; }

    public virtual DbSet<TiposPermiso> TiposPermiso { get; set; }

    public virtual DbSet<TiposRegimen> TiposRegimen { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<ZonasSalarioMinimo> ZonasSalarioMinimo { get; set; }

    public virtual DbSet<VwBiAcumuladosAnuales> VwBiAcumuladosAnuales { get; set; }

    public virtual DbSet<VwBiAsistenciaDiaria> VwBiAsistenciaDiaria { get; set; }

    public virtual DbSet<VwBiConceptosNomina> VwBiConceptosNomina { get; set; }

    public virtual DbSet<VwBiCostoPatronal> VwBiCostoPatronal { get; set; }

    public virtual DbSet<VwBiDirectorioEmpleados> VwBiDirectorioEmpleados { get; set; }

    public virtual DbSet<VwBiDirectorioPendientes> VwBiDirectorioPendientes { get; set; }

    public virtual DbSet<VwBiMovimientosImss> VwBiMovimientosImss { get; set; }

    public virtual DbSet<VwBiPeriodosNomina> VwBiPeriodosNomina { get; set; }

    public virtual DbSet<VwBiRecibos> VwBiRecibos { get; set; }

    public virtual DbSet<VwBiRegistrosPatronales> VwBiRegistrosPatronales { get; set; }

    public virtual DbSet<VwBiResumenAsistenciaPeriodo> VwBiResumenAsistenciaPeriodo { get; set; }

    public virtual DbSet<VwBiVacacionesDisponibles> VwBiVacacionesDisponibles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccionesDirectorio>(entity =>
        {
            entity.HasKey(e => e.IdAccionDirectorio).HasName("tb_bi_acciones_directorio_pkey");

            entity.ToTable("tb_bi_acciones_directorio", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_acciones_directorio_clave_key").IsUnique();

            entity.Property(e => e.IdAccionDirectorio)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_accion_directorio");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.AccionesDirectorioIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_acciones_directorio_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.AccionesDirectorioIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_acciones_directorio_usuario_modificacion");
        });

        modelBuilder.Entity<AsistenciaDiaria>(entity =>
        {
            entity.HasKey(e => e.IdAsistenciaDiaria).HasName("tb_bi_asistencia_diaria_pkey");

            entity.ToTable("tb_bi_asistencia_diaria", "recursos_humanos");

            entity.HasIndex(e => new { e.IdEmpleado, e.FechaDia }, "ix_asistencia_diaria_empleado_fecha");

            entity.HasIndex(e => new { e.IdHistorialHorario, e.FechaDia }, "ux_asistencia_diaria").IsUnique();

            entity.Property(e => e.IdAsistenciaDiaria)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_asistencia_diaria");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaDia).HasColumnName("fecha_dia");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.HorasCubiertas)
                .HasPrecision(5, 2)
                .HasColumnName("horas_cubiertas");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdEstatusAsistencia).HasColumnName("id_estatus_asistencia");
            entity.Property(e => e.IdHistorialHorario).HasColumnName("id_historial_horario");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.MinutosRetardo).HasColumnName("minutos_retardo");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.AsistenciaDiaria)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_asistencia_diaria_id_empleado_fkey");

            entity.HasOne(d => d.IdEstatusAsistenciaNavigation).WithMany(p => p.AsistenciaDiaria)
                .HasForeignKey(d => d.IdEstatusAsistencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_asistencia_diaria_id_estatus_asistencia_fkey");

            entity.HasOne(d => d.IdHistorialHorarioNavigation).WithMany(p => p.AsistenciaDiaria)
                .HasForeignKey(d => d.IdHistorialHorario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_asistencia_diaria_id_historial_horario_fkey");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.AsistenciaDiaria)
                .HasForeignKey(d => d.IdSolicitud)
                .HasConstraintName("tb_bi_asistencia_diaria_id_solicitud_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.AsistenciaDiariaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_asistencia_diaria_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.AsistenciaDiariaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_asistencia_diaria_usuario_modificacion");
        });

        modelBuilder.Entity<Asistencias>(entity =>
        {
            entity.HasKey(e => e.IdAsistencia).HasName("tb_bi_asistencias_pkey");

            entity.ToTable("tb_bi_asistencias", "recursos_humanos");

            entity.HasIndex(e => new { e.IdEmpleado, e.FechaHoraRegistro }, "ix_asistencias_empleado_fecha");

            entity.HasIndex(e => new { e.IdChecador, e.NumeroEventoHikvision }, "ux_asistencias_evento").IsUnique();

            entity.Property(e => e.IdAsistencia)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_asistencia");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaHoraRegistro)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_hora_registro");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdChecador).HasColumnName("id_checador");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdOrigenChecada)
                .HasDefaultValue(1)
                .HasColumnName("id_origen_checada");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NumeroEventoHikvision).HasColumnName("numero_evento_hikvision");
            entity.Property(e => e.TipoRegistro)
                .HasMaxLength(10)
                .HasDefaultValueSql("'checada'::character varying")
                .HasColumnName("tipo_registro");

            entity.HasOne(d => d.IdChecadorNavigation).WithMany(p => p.Asistencias)
                .HasForeignKey(d => d.IdChecador)
                .HasConstraintName("tb_bi_asistencias_id_checador_fkey");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Asistencias)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_asistencias_id_empleado_fkey");

            entity.HasOne(d => d.IdOrigenChecadaNavigation).WithMany(p => p.Asistencias)
                .HasForeignKey(d => d.IdOrigenChecada)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_asistencias_id_origen_checada_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.AsistenciasIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_asistencias_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.AsistenciasIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_asistencias_usuario_modificacion");
        });

        modelBuilder.Entity<AuditoriaLogs>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("tb_bi_auditoria_logs_pkey");

            entity.ToTable("tb_bi_auditoria_logs", "recursos_humanos");

            entity.HasIndex(e => new { e.TablaAfectada, e.IdRegistro }, "ix_auditoria_tabla_registro");

            entity.HasIndex(e => new { e.IdUsuario, e.FechaAccion }, "ix_auditoria_usuario_fecha").IsDescending(false, true);

            entity.Property(e => e.IdLog)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_log");
            entity.Property(e => e.Accion)
                .HasMaxLength(10)
                .HasColumnName("accion");
            entity.Property(e => e.FechaAccion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_accion");
            entity.Property(e => e.IdRegistro)
                .HasMaxLength(50)
                .HasColumnName("id_registro");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IpOrigen)
                .HasMaxLength(45)
                .HasColumnName("ip_origen");
            entity.Property(e => e.JsonDatosNuevos)
                .HasColumnType("jsonb")
                .HasColumnName("json_datos_nuevos");
            entity.Property(e => e.JsonDatosPrevios)
                .HasColumnType("jsonb")
                .HasColumnName("json_datos_previos");
            entity.Property(e => e.TablaAfectada)
                .HasMaxLength(100)
                .HasColumnName("tabla_afectada");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AuditoriaLogs)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_auditoria_logs_id_usuario_fkey");
        });

        modelBuilder.Entity<BajasLaborales>(entity =>
        {
            entity.HasKey(e => e.IdBaja).HasName("tb_bi_bajas_laborales_pkey");

            entity.ToTable("tb_bi_bajas_laborales", "recursos_humanos");

            entity.Property(e => e.IdBaja)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_baja");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.EstatusPago)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pendiente'::character varying")
                .HasColumnName("estatus_pago");
            entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdMotivoBaja).HasColumnName("id_motivo_baja");
            entity.Property(e => e.IdNominaFiniquito).HasColumnName("id_nomina_finiquito");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.MontoFiniquitoCalculado)
                .HasPrecision(12, 2)
                .HasColumnName("monto_finiquito_calculado");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.BajasLaborales)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_bajas_laborales_id_empleado_fkey");

            entity.HasOne(d => d.IdMotivoBajaNavigation).WithMany(p => p.BajasLaborales)
                .HasForeignKey(d => d.IdMotivoBaja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_bajas_laborales_id_motivo_baja_fkey");

            entity.HasOne(d => d.IdNominaFiniquitoNavigation).WithMany(p => p.BajasLaborales)
                .HasForeignKey(d => d.IdNominaFiniquito)
                .HasConstraintName("fk_bajas_nomina_finiquito");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.BajasLaboralesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bajas_laborales_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.BajasLaboralesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_bajas_laborales_usuario_modificacion");
        });

        modelBuilder.Entity<Bancos>(entity =>
        {
            entity.HasKey(e => e.IdBanco).HasName("tb_bi_bancos_pkey");

            entity.ToTable("tb_bi_bancos", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_bancos_clave_key").IsUnique();

            entity.Property(e => e.IdBanco)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_banco");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("clave");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombreCorto)
                .HasMaxLength(50)
                .HasColumnName("nombre_corto");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(200)
                .HasColumnName("razon_social");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.BancosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_bancos_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.BancosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_bancos_usuario_modificacion");
        });

        modelBuilder.Entity<Campus>(entity =>
        {
            entity.HasKey(e => e.IdCampus).HasName("tb_bi_campus_pkey");

            entity.ToTable("tb_bi_campus", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_campus_clave_key").IsUnique();

            entity.HasIndex(e => new { e.IdEmpresa, e.NombreCampus }, "ux_campus_nombre").IsUnique();

            entity.Property(e => e.IdCampus)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_campus");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CalleNumero)
                .HasMaxLength(200)
                .HasColumnName("calle_numero");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .HasColumnName("ciudad");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("codigo_postal");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombreCampus)
                .HasMaxLength(100)
                .HasColumnName("nombre_campus");
            entity.Property(e => e.OuDirectorio)
                .HasMaxLength(300)
                .HasColumnName("ou_directorio");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Campus)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_campus_id_empresa_fkey");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Campus)
                .HasForeignKey(d => d.IdEstado)
                .HasConstraintName("tb_bi_campus_id_estado_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.CampusIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_campus_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.CampusIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_campus_usuario_modificacion");
        });

        modelBuilder.Entity<CausasBajaImss>(entity =>
        {
            entity.HasKey(e => e.IdCausaBajaImss).HasName("tb_bi_causas_baja_imss_pkey");

            entity.ToTable("tb_bi_causas_baja_imss", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_causas_baja_imss_clave_key").IsUnique();

            entity.Property(e => e.IdCausaBajaImss)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_causa_baja_imss");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(1)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.CausasBajaImssIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_causas_baja_imss_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.CausasBajaImssIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_causas_baja_imss_usuario_modificacion");
        });

        modelBuilder.Entity<CesantiaVejezPatron>(entity =>
        {
            entity.HasKey(e => e.IdCesantiaVejez).HasName("tb_bi_cesantia_vejez_patron_pkey");

            entity.ToTable("tb_bi_cesantia_vejez_patron", "nomina");

            entity.HasIndex(e => new { e.Anio, e.VecesUmaDesde }, "ux_cesantia_vejez").IsUnique();

            entity.Property(e => e.IdCesantiaVejez)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_cesantia_vejez");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Porcentaje)
                .HasPrecision(7, 4)
                .HasColumnName("porcentaje");
            entity.Property(e => e.VecesUmaDesde)
                .HasPrecision(6, 2)
                .HasColumnName("veces_uma_desde");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.CesantiaVejezPatronIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_cesantia_vejez_patron_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.CesantiaVejezPatronIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_cesantia_vejez_patron_usuario_modificacion");
        });

        modelBuilder.Entity<Checadores>(entity =>
        {
            entity.HasKey(e => e.IdChecador).HasName("tb_bi_checadores_pkey");

            entity.ToTable("tb_bi_checadores", "recursos_humanos");

            entity.HasIndex(e => e.NumeroSerie, "tb_bi_checadores_numero_serie_key").IsUnique();

            entity.Property(e => e.IdChecador)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_checador");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.IpDispositivo)
                .HasMaxLength(45)
                .HasColumnName("ip_dispositivo");
            entity.Property(e => e.NombreChecador)
                .HasMaxLength(100)
                .HasColumnName("nombre_checador");
            entity.Property(e => e.NumeroSerie)
                .HasMaxLength(60)
                .HasColumnName("numero_serie");
            entity.Property(e => e.Ubicacion)
                .HasMaxLength(150)
                .HasColumnName("ubicacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ChecadoresIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_checadores_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ChecadoresIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_checadores_usuario_modificacion");
        });

        modelBuilder.Entity<ClasesRiesgo>(entity =>
        {
            entity.HasKey(e => e.IdClaseRiesgo).HasName("tb_bi_clases_riesgo_pkey");

            entity.ToTable("tb_bi_clases_riesgo", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_clases_riesgo_clave_key").IsUnique();

            entity.Property(e => e.IdClaseRiesgo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_clase_riesgo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(2)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.PrimaMedia)
                .HasPrecision(8, 5)
                .HasColumnName("prima_media");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ClasesRiesgoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_clases_riesgo_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ClasesRiesgoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_clases_riesgo_usuario_modificacion");
        });

        modelBuilder.Entity<ClavesSatConcepto>(entity =>
        {
            entity.HasKey(e => e.IdClaveSatConcepto).HasName("tb_bi_claves_sat_concepto_pkey");

            entity.ToTable("tb_bi_claves_sat_concepto", "nomina");

            entity.HasIndex(e => new { e.IdTipoConcepto, e.Clave }, "ux_claves_sat_concepto").IsUnique();

            entity.Property(e => e.IdClaveSatConcepto)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_clave_sat_concepto");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(5)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdTipoConcepto).HasColumnName("id_tipo_concepto");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdTipoConceptoNavigation).WithMany(p => p.ClavesSatConcepto)
                .HasForeignKey(d => d.IdTipoConcepto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_claves_sat_concepto_id_tipo_concepto_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ClavesSatConceptoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_claves_sat_concepto_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ClavesSatConceptoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_claves_sat_concepto_usuario_modificacion");
        });

        modelBuilder.Entity<ConceptosHist>(entity =>
        {
            entity.HasKey(e => e.IdHistConcepto).HasName("tb_bi_conceptos_hist_pkey");

            entity.ToTable("tb_bi_conceptos_hist", "nomina");

            entity.Property(e => e.IdHistConcepto)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_hist_concepto");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaInicioDescuento)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("fecha_inicio_descuento");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdConcepto).HasColumnName("id_concepto");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.MontoTotalDeuda)
                .HasPrecision(12, 2)
                .HasColumnName("monto_total_deuda");
            entity.Property(e => e.ParcialidadesPagadas).HasColumnName("parcialidades_pagadas");
            entity.Property(e => e.SaldoRestante)
                .HasPrecision(12, 2)
                .HasColumnName("saldo_restante");
            entity.Property(e => e.TotalParcialidades).HasColumnName("total_parcialidades");

            entity.HasOne(d => d.IdConceptoNavigation).WithMany(p => p.ConceptosHist)
                .HasForeignKey(d => d.IdConcepto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_conceptos_hist_id_concepto_fkey");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.ConceptosHist)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_conceptos_hist_id_empleado_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ConceptosHistIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_conceptos_hist_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ConceptosHistIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_conceptos_hist_usuario_modificacion");
        });

        modelBuilder.Entity<ConceptosNomina>(entity =>
        {
            entity.HasKey(e => e.IdConcepto).HasName("tb_bi_conceptos_nomina_pkey");

            entity.ToTable("tb_bi_conceptos_nomina", "nomina");

            entity.HasIndex(e => e.ClaveConcepto, "tb_bi_conceptos_nomina_clave_concepto_key").IsUnique();

            entity.Property(e => e.IdConcepto)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_concepto");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ClaveConcepto)
                .HasMaxLength(10)
                .HasColumnName("clave_concepto");
            entity.Property(e => e.EsCalculado).HasColumnName("es_calculado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdClaveSatConcepto).HasColumnName("id_clave_sat_concepto");
            entity.Property(e => e.IdTipoConcepto).HasColumnName("id_tipo_concepto");
            entity.Property(e => e.IdTipoExencion)
                .HasDefaultValue(1)
                .HasColumnName("id_tipo_exencion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.IntegraSdi).HasColumnName("integra_sdi");
            entity.Property(e => e.NombreConcepto)
                .HasMaxLength(100)
                .HasColumnName("nombre_concepto");

            entity.HasOne(d => d.IdClaveSatConceptoNavigation).WithMany(p => p.ConceptosNomina)
                .HasForeignKey(d => d.IdClaveSatConcepto)
                .HasConstraintName("tb_bi_conceptos_nomina_id_clave_sat_concepto_fkey");

            entity.HasOne(d => d.IdTipoConceptoNavigation).WithMany(p => p.ConceptosNomina)
                .HasForeignKey(d => d.IdTipoConcepto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_conceptos_nomina_id_tipo_concepto_fkey");

            entity.HasOne(d => d.IdTipoExencionNavigation).WithMany(p => p.ConceptosNomina)
                .HasForeignKey(d => d.IdTipoExencion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_conceptos_nomina_id_tipo_exencion_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ConceptosNominaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_conceptos_nomina_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ConceptosNominaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_conceptos_nomina_usuario_modificacion");
        });

        modelBuilder.Entity<ConceptosRecurrentes>(entity =>
        {
            entity.HasKey(e => e.IdConceptoRecurrente).HasName("tb_bi_conceptos_recurrentes_pkey");

            entity.ToTable("tb_bi_conceptos_recurrentes", "nomina");

            entity.Property(e => e.IdConceptoRecurrente)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_concepto_recurrente");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdConcepto).HasColumnName("id_concepto");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Importe)
                .HasPrecision(12, 2)
                .HasColumnName("importe");

            entity.HasOne(d => d.IdConceptoNavigation).WithMany(p => p.ConceptosRecurrentes)
                .HasForeignKey(d => d.IdConcepto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_conceptos_recurrentes_id_concepto_fkey");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.ConceptosRecurrentes)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_conceptos_recurrentes_id_empleado_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ConceptosRecurrentesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_conceptos_recurrentes_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ConceptosRecurrentesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_conceptos_recurrentes_usuario_modificacion");
        });

        modelBuilder.Entity<CreditosFonacot>(entity =>
        {
            entity.HasKey(e => e.IdCreditoFonacot).HasName("tb_bi_creditos_fonacot_pkey");

            entity.ToTable("tb_bi_creditos_fonacot", "nomina");

            entity.HasIndex(e => e.NumeroCredito, "tb_bi_creditos_fonacot_numero_credito_key").IsUnique();

            entity.Property(e => e.IdCreditoFonacot)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_credito_fonacot");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.ImporteTotal)
                .HasPrecision(12, 2)
                .HasColumnName("importe_total");
            entity.Property(e => e.NumeroCredito)
                .HasMaxLength(20)
                .HasColumnName("numero_credito");
            entity.Property(e => e.RetencionMensual)
                .HasPrecision(12, 2)
                .HasColumnName("retencion_mensual");
            entity.Property(e => e.Saldo)
                .HasPrecision(12, 2)
                .HasColumnName("saldo");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.CreditosFonacot)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_creditos_fonacot_id_empleado_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.CreditosFonacotIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_creditos_fonacot_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.CreditosFonacotIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_creditos_fonacot_usuario_modificacion");
        });

        modelBuilder.Entity<CreditosInfonavit>(entity =>
        {
            entity.HasKey(e => e.IdCreditoInfonavit).HasName("tb_bi_creditos_infonavit_pkey");

            entity.ToTable("tb_bi_creditos_infonavit", "nomina");

            entity.HasIndex(e => e.NumeroCredito, "tb_bi_creditos_infonavit_numero_credito_key").IsUnique();

            entity.Property(e => e.IdCreditoInfonavit)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_credito_infonavit");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaSuspension).HasColumnName("fecha_suspension");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdTipoDescuento).HasColumnName("id_tipo_descuento");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.IncluyeSeguroVivienda)
                .HasDefaultValue(true)
                .HasColumnName("incluye_seguro_vivienda");
            entity.Property(e => e.NumeroCredito)
                .HasMaxLength(10)
                .HasColumnName("numero_credito");
            entity.Property(e => e.ValorDescuento)
                .HasPrecision(10, 4)
                .HasColumnName("valor_descuento");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.CreditosInfonavit)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_creditos_infonavit_id_empleado_fkey");

            entity.HasOne(d => d.IdTipoDescuentoNavigation).WithMany(p => p.CreditosInfonavit)
                .HasForeignKey(d => d.IdTipoDescuento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_creditos_infonavit_id_tipo_descuento_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.CreditosInfonavitIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_creditos_infonavit_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.CreditosInfonavitIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_creditos_infonavit_usuario_modificacion");
        });

        modelBuilder.Entity<CuentasDirectorio>(entity =>
        {
            entity.HasKey(e => e.IdCuentaDirectorio).HasName("tb_bi_cuentas_directorio_pkey");

            entity.ToTable("tb_bi_cuentas_directorio", "recursos_humanos");

            entity.HasIndex(e => e.CorreoInstitucional, "tb_bi_cuentas_directorio_correo_institucional_key").IsUnique();

            entity.HasIndex(e => e.EntraObjectId, "tb_bi_cuentas_directorio_entra_object_id_key").IsUnique();

            entity.HasIndex(e => e.IdEmpleado, "tb_bi_cuentas_directorio_id_empleado_key").IsUnique();

            entity.HasIndex(e => e.ObjectGuid, "tb_bi_cuentas_directorio_object_guid_key").IsUnique();

            entity.HasIndex(e => e.SamAccountName, "tb_bi_cuentas_directorio_sam_account_name_key").IsUnique();

            entity.HasIndex(e => e.UserPrincipalName, "tb_bi_cuentas_directorio_user_principal_name_key").IsUnique();

            entity.Property(e => e.IdCuentaDirectorio)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_cuenta_directorio");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CorreoInstitucional)
                .HasMaxLength(150)
                .HasColumnName("correo_institucional");
            entity.Property(e => e.CuentaExistenteM365).HasColumnName("cuenta_existente_m365");
            entity.Property(e => e.DistinguishedName)
                .HasMaxLength(400)
                .HasColumnName("distinguished_name");
            entity.Property(e => e.EntraObjectId).HasColumnName("entra_object_id");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaUltimaSincronizacion).HasColumnName("fecha_ultima_sincronizacion");
            entity.Property(e => e.HashSincronizado)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("hash_sincronizado");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.ObjectGuid).HasColumnName("object_guid");
            entity.Property(e => e.SamAccountName)
                .HasMaxLength(20)
                .HasColumnName("sam_account_name");
            entity.Property(e => e.UltimoError)
                .HasMaxLength(1000)
                .HasColumnName("ultimo_error");
            entity.Property(e => e.UserPrincipalName)
                .HasMaxLength(150)
                .HasColumnName("user_principal_name");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithOne(p => p.CuentasDirectorio)
                .HasForeignKey<CuentasDirectorio>(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_cuentas_directorio_id_empleado_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.CuentasDirectorioIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cuentas_directorio_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.CuentasDirectorioIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_cuentas_directorio_usuario_modificacion");
        });

        modelBuilder.Entity<CuotasImss>(entity =>
        {
            entity.HasKey(e => e.IdCuotaImss).HasName("tb_bi_cuotas_imss_pkey");

            entity.ToTable("tb_bi_cuotas_imss", "nomina");

            entity.HasIndex(e => new { e.ClaveRamo, e.FechaInicio }, "ux_cuotas_imss").IsUnique();

            entity.Property(e => e.IdCuotaImss)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_cuota_imss");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.BaseCalculo)
                .HasMaxLength(20)
                .HasColumnName("base_calculo");
            entity.Property(e => e.ClaveRamo)
                .HasMaxLength(20)
                .HasColumnName("clave_ramo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.PorcentajeObrero)
                .HasPrecision(7, 4)
                .HasColumnName("porcentaje_obrero");
            entity.Property(e => e.PorcentajePatron)
                .HasPrecision(7, 4)
                .HasColumnName("porcentaje_patron");
            entity.Property(e => e.TipoDias)
                .HasMaxLength(10)
                .HasColumnName("tipo_dias");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.CuotasImssIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_cuotas_imss_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.CuotasImssIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_cuotas_imss_usuario_modificacion");
        });

        modelBuilder.Entity<Departamentos>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("tb_bi_departamentos_pkey");

            entity.ToTable("tb_bi_departamentos", "recursos_humanos");

            entity.HasIndex(e => e.NombreDepartamento, "tb_bi_departamentos_nombre_departamento_key").IsUnique();

            entity.Property(e => e.IdDepartamento)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_departamento");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.GrupoDirectorio)
                .HasMaxLength(100)
                .HasColumnName("grupo_directorio");
            entity.Property(e => e.IdResponsable).HasColumnName("id_responsable");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombreDepartamento)
                .HasMaxLength(100)
                .HasColumnName("nombre_departamento");

            entity.HasOne(d => d.IdResponsableNavigation).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.IdResponsable)
                .HasConstraintName("fk_departamentos_responsable");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.DepartamentosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_departamentos_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.DepartamentosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_departamentos_usuario_modificacion");
        });

        modelBuilder.Entity<DiasFestivos>(entity =>
        {
            entity.HasKey(e => e.IdDiaFestivo).HasName("tb_bi_dias_festivos_pkey");

            entity.ToTable("tb_bi_dias_festivos", "recursos_humanos");

            entity.HasIndex(e => e.Fecha, "tb_bi_dias_festivos_fecha_key").IsUnique();

            entity.Property(e => e.IdDiaFestivo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_dia_festivo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.DescripcionFestividad)
                .HasMaxLength(150)
                .HasColumnName("descripcion_festividad");
            entity.Property(e => e.EsLaborable).HasColumnName("es_laborable");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.DiasFestivosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dias_festivos_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.DiasFestivosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_dias_festivos_usuario_modificacion");
        });

        modelBuilder.Entity<Empleados>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado).HasName("tb_bi_empleados_pkey");

            entity.ToTable("tb_bi_empleados", "recursos_humanos");

            entity.HasIndex(e => e.Matricula, "tb_bi_empleados_matricula_key").IsUnique();

            entity.HasIndex(e => e.NumeroChecador, "tb_bi_empleados_numero_checador_key").IsUnique();

            entity.HasIndex(e => new { e.IdPersona, e.IdEmpresa }, "ux_empleados_persona_empresa").IsUnique();

            entity.Property(e => e.IdEmpleado)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_empleado");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AvisoPrivacidadBiometricos).HasColumnName("aviso_privacidad_biometricos");
            entity.Property(e => e.ExtensionTelefono)
                .HasMaxLength(10)
                .HasColumnName("extension_telefono");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFirmaAviso).HasColumnName("fecha_firma_aviso");
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdCampus).HasColumnName("id_campus");
            entity.Property(e => e.IdDepartamento).HasColumnName("id_departamento");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdEstatusLaboral)
                .HasDefaultValue(1)
                .HasColumnName("id_estatus_laboral");
            entity.Property(e => e.IdJefeDirecto).HasColumnName("id_jefe_directo");
            entity.Property(e => e.IdPerfil).HasColumnName("id_perfil");
            entity.Property(e => e.IdPersona).HasColumnName("id_persona");
            entity.Property(e => e.IdPuesto).HasColumnName("id_puesto");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Matricula)
                .HasMaxLength(30)
                .HasColumnName("matricula");
            entity.Property(e => e.NumeroChecador).HasColumnName("numero_checador");

            entity.HasOne(d => d.IdCampusNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdCampus)
                .HasConstraintName("tb_bi_empleados_id_campus_fkey");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_id_departamento_fkey");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_id_empresa_fkey");

            entity.HasOne(d => d.IdEstatusLaboralNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdEstatusLaboral)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_id_estatus_laboral_fkey");

            entity.HasOne(d => d.IdJefeDirectoNavigation).WithMany(p => p.InverseIdJefeDirectoNavigation)
                .HasForeignKey(d => d.IdJefeDirecto)
                .HasConstraintName("tb_bi_empleados_id_jefe_directo_fkey");

            entity.HasOne(d => d.IdPerfilNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdPerfil)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_id_perfil_fkey");

            entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdPersona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_id_persona_fkey");

            entity.HasOne(d => d.IdPuestoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdPuesto)
                .HasConstraintName("tb_bi_empleados_id_puesto_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.EmpleadosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_empleados_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.EmpleadosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_empleados_usuario_modificacion");
        });

        modelBuilder.Entity<EmpleadosNomina>(entity =>
        {
            entity.HasKey(e => e.IdEmpleadoNomina).HasName("tb_bi_empleados_nomina_pkey");

            entity.ToTable("tb_bi_empleados_nomina", "nomina");

            entity.HasIndex(e => e.IdEmpleado, "tb_bi_empleados_nomina_id_empleado_key").IsUnique();

            entity.Property(e => e.IdEmpleadoNomina)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_empleado_nomina");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clabe)
                .HasMaxLength(18)
                .IsFixedLength()
                .HasColumnName("clabe");
            entity.Property(e => e.ExcluidoPtu).HasColumnName("excluido_ptu");
            entity.Property(e => e.FechaAltaImss).HasColumnName("fecha_alta_imss");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdBanco).HasColumnName("id_banco");
            entity.Property(e => e.IdClaseRiesgoPuesto)
                .HasDefaultValue(1)
                .HasColumnName("id_clase_riesgo_puesto");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdEsquemaPrestaciones).HasColumnName("id_esquema_prestaciones");
            entity.Property(e => e.IdMetodoPago)
                .HasDefaultValue(1)
                .HasColumnName("id_metodo_pago");
            entity.Property(e => e.IdPeriodicidadPago)
                .HasDefaultValue(3)
                .HasColumnName("id_periodicidad_pago");
            entity.Property(e => e.IdRegistroPatronal).HasColumnName("id_registro_patronal");
            entity.Property(e => e.IdTipoContrato)
                .HasDefaultValue(1)
                .HasColumnName("id_tipo_contrato");
            entity.Property(e => e.IdTipoJornada)
                .HasDefaultValue(1)
                .HasColumnName("id_tipo_jornada");
            entity.Property(e => e.IdTipoRegimen)
                .HasDefaultValue(1)
                .HasColumnName("id_tipo_regimen");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Nss)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasColumnName("nss");
            entity.Property(e => e.Sindicalizado).HasColumnName("sindicalizado");

            entity.HasOne(d => d.IdBancoNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdBanco)
                .HasConstraintName("tb_bi_empleados_nomina_id_banco_fkey");

            entity.HasOne(d => d.IdClaseRiesgoPuestoNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdClaseRiesgoPuesto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_clase_riesgo_puesto_fkey");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithOne(p => p.EmpleadosNomina)
                .HasForeignKey<EmpleadosNomina>(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_empleado_fkey");

            entity.HasOne(d => d.IdEsquemaPrestacionesNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdEsquemaPrestaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_esquema_prestaciones_fkey");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_metodo_pago_fkey");

            entity.HasOne(d => d.IdPeriodicidadPagoNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdPeriodicidadPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_periodicidad_pago_fkey");

            entity.HasOne(d => d.IdRegistroPatronalNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdRegistroPatronal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_registro_patronal_fkey");

            entity.HasOne(d => d.IdTipoContratoNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdTipoContrato)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_tipo_contrato_fkey");

            entity.HasOne(d => d.IdTipoJornadaNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdTipoJornada)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_tipo_jornada_fkey");

            entity.HasOne(d => d.IdTipoRegimenNavigation).WithMany(p => p.EmpleadosNomina)
                .HasForeignKey(d => d.IdTipoRegimen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empleados_nomina_id_tipo_regimen_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.EmpleadosNominaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_empleados_nomina_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.EmpleadosNominaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_empleados_nomina_usuario_modificacion");
        });

        modelBuilder.Entity<Empresas>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("tb_bi_empresas_pkey");

            entity.ToTable("tb_bi_empresas", "recursos_humanos");

            entity.HasIndex(e => e.Rfc, "tb_bi_empresas_rfc_key").IsUnique();

            entity.Property(e => e.IdEmpresa)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_empresa");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CodigoPostalExpedicion)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("codigo_postal_expedicion");
            entity.Property(e => e.DominioCorreo)
                .HasMaxLength(100)
                .HasColumnName("dominio_correo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FolioSiguiente)
                .HasDefaultValue(1)
                .HasColumnName("folio_siguiente");
            entity.Property(e => e.IdRegimenFiscal).HasColumnName("id_regimen_fiscal");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(100)
                .HasColumnName("nombre_comercial");
            entity.Property(e => e.NumeroCertificadoCsd)
                .HasMaxLength(20)
                .HasColumnName("numero_certificado_csd");
            entity.Property(e => e.OuDirectorio)
                .HasMaxLength(300)
                .HasColumnName("ou_directorio");
            entity.Property(e => e.ProveedorPac)
                .HasMaxLength(50)
                .HasColumnName("proveedor_pac");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(200)
                .HasColumnName("razon_social");
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .HasColumnName("rfc");
            entity.Property(e => e.RutaCertificadoCsd)
                .HasMaxLength(300)
                .HasColumnName("ruta_certificado_csd");
            entity.Property(e => e.SerieRecibos)
                .HasMaxLength(10)
                .HasDefaultValueSql("'NOM'::character varying")
                .HasColumnName("serie_recibos");
            entity.Property(e => e.VigenciaCsd).HasColumnName("vigencia_csd");

            entity.HasOne(d => d.IdRegimenFiscalNavigation).WithMany(p => p.Empresas)
                .HasForeignKey(d => d.IdRegimenFiscal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_empresas_id_regimen_fiscal_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.EmpresasIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_empresas_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.EmpresasIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_empresas_usuario_modificacion");
        });

        modelBuilder.Entity<EsquemasPrestaciones>(entity =>
        {
            entity.HasKey(e => e.IdEsquemaPrestaciones).HasName("tb_bi_esquemas_prestaciones_pkey");

            entity.ToTable("tb_bi_esquemas_prestaciones", "nomina");

            entity.HasIndex(e => new { e.IdEmpresa, e.NombreEsquema }, "ux_esquemas").IsUnique();

            entity.Property(e => e.IdEsquemaPrestaciones)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_esquema_prestaciones");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.DescontarFaltasAguinaldo).HasColumnName("descontar_faltas_aguinaldo");
            entity.Property(e => e.DiasAguinaldo)
                .HasDefaultValue(15)
                .HasColumnName("dias_aguinaldo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombreEsquema)
                .HasMaxLength(100)
                .HasColumnName("nombre_esquema");
            entity.Property(e => e.PorcentajePrimaDominical)
                .HasPrecision(6, 2)
                .HasDefaultValue(25m)
                .HasColumnName("porcentaje_prima_dominical");
            entity.Property(e => e.PorcentajePrimaVacacional)
                .HasPrecision(6, 2)
                .HasDefaultValue(25m)
                .HasColumnName("porcentaje_prima_vacacional");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.EsquemasPrestaciones)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_esquemas_prestaciones_id_empresa_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.EsquemasPrestacionesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_esquemas_prestaciones_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.EsquemasPrestacionesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_esquemas_prestaciones_usuario_modificacion");
        });

        modelBuilder.Entity<Estados>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("tb_bi_estados_pkey");

            entity.ToTable("tb_bi_estados", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_estados_clave_key").IsUnique();

            entity.Property(e => e.IdEstado)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_estado");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.EstadosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_estados_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.EstadosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_estados_usuario_modificacion");
        });

        modelBuilder.Entity<EstatusAsistencia>(entity =>
        {
            entity.HasKey(e => e.IdEstatusAsistencia).HasName("tb_bi_estatus_asistencia_pkey");

            entity.ToTable("tb_bi_estatus_asistencia", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_estatus_asistencia_clave_key").IsUnique();

            entity.Property(e => e.IdEstatusAsistencia)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_estatus_asistencia");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(30)
                .HasColumnName("clave");
            entity.Property(e => e.ColorHex)
                .HasMaxLength(7)
                .IsFixedLength()
                .HasColumnName("color_hex");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsFalta).HasColumnName("es_falta");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.GoceSueldo)
                .HasDefaultValue(true)
                .HasColumnName("goce_sueldo");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.EstatusAsistenciaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_estatus_asistencia_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.EstatusAsistenciaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_estatus_asistencia_usuario_modificacion");
        });

        modelBuilder.Entity<EstatusLaborales>(entity =>
        {
            entity.HasKey(e => e.IdEstatusLaboral).HasName("tb_bi_estatus_laborales_pkey");

            entity.ToTable("tb_bi_estatus_laborales", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_estatus_laborales_clave_key").IsUnique();

            entity.Property(e => e.IdEstatusLaboral)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_estatus_laboral");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.EstatusLaboralesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_estatus_laborales_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.EstatusLaboralesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_estatus_laborales_usuario_modificacion");
        });

        modelBuilder.Entity<HistorialHorarios>(entity =>
        {
            entity.HasKey(e => e.IdHistorialHorario).HasName("tb_bi_historial_horarios_pkey");

            entity.ToTable("tb_bi_historial_horarios", "recursos_humanos");

            entity.HasIndex(e => new { e.IdEmpleado, e.DiaSemana }, "ix_historial_horarios_empleado_dia");

            entity.Property(e => e.IdHistorialHorario)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_historial_horario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.DiaSemana).HasColumnName("dia_semana");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFinVigencia).HasColumnName("fecha_fin_vigencia");
            entity.Property(e => e.FechaInicioVigencia).HasColumnName("fecha_inicio_vigencia");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.HoraComidaFin).HasColumnName("hora_comida_fin");
            entity.Property(e => e.HoraComidaInicio).HasColumnName("hora_comida_inicio");
            entity.Property(e => e.HoraEntrada).HasColumnName("hora_entrada");
            entity.Property(e => e.HoraSalida).HasColumnName("hora_salida");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdMateria).HasColumnName("id_materia");
            entity.Property(e => e.IdPeriodo).HasColumnName("id_periodo");
            entity.Property(e => e.IdReglaAsistencia).HasColumnName("id_regla_asistencia");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.HistorialHorarios)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_historial_horarios_id_empleado_fkey");

            entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.HistorialHorarios)
                .HasForeignKey(d => d.IdMateria)
                .HasConstraintName("tb_bi_historial_horarios_id_materia_fkey");

            entity.HasOne(d => d.IdPeriodoNavigation).WithMany(p => p.HistorialHorarios)
                .HasForeignKey(d => d.IdPeriodo)
                .HasConstraintName("tb_bi_historial_horarios_id_periodo_fkey");

            entity.HasOne(d => d.IdReglaAsistenciaNavigation).WithMany(p => p.HistorialHorarios)
                .HasForeignKey(d => d.IdReglaAsistencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_historial_horarios_id_regla_asistencia_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.HistorialHorariosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historial_horarios_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.HistorialHorariosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_historial_horarios_usuario_modificacion");
        });

        modelBuilder.Entity<HistorialSueldos>(entity =>
        {
            entity.HasKey(e => e.IdHistorialSueldo).HasName("tb_bi_historial_sueldos_pkey");

            entity.ToTable("tb_bi_historial_sueldos", "recursos_humanos");

            entity.HasIndex(e => e.IdEmpleado, "ux_historial_sueldos_vigente")
                .IsUnique()
                .HasFilter("(fecha_fin_vigencia IS NULL)");

            entity.Property(e => e.IdHistorialSueldo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_historial_sueldo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFinVigencia).HasColumnName("fecha_fin_vigencia");
            entity.Property(e => e.FechaInicioVigencia).HasColumnName("fecha_inicio_vigencia");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.SalarioBase)
                .HasPrecision(12, 2)
                .HasColumnName("salario_base");
            entity.Property(e => e.TarifaPorHora)
                .HasPrecision(10, 2)
                .HasColumnName("tarifa_por_hora");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.HistorialSueldos)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_historial_sueldos_id_empleado_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.HistorialSueldosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historial_sueldos_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.HistorialSueldosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_historial_sueldos_usuario_modificacion");
        });

        modelBuilder.Entity<IncapacidadesImss>(entity =>
        {
            entity.HasKey(e => e.IdIncapacidad).HasName("tb_bi_incapacidades_imss_pkey");

            entity.ToTable("tb_bi_incapacidades_imss", "recursos_humanos");

            entity.HasIndex(e => new { e.IdEmpleado, e.FechaInicio, e.FechaFin }, "ix_incapacidades_empleado");

            entity.HasIndex(e => e.FolioImss, "tb_bi_incapacidades_imss_folio_imss_key").IsUnique();

            entity.Property(e => e.IdIncapacidad)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_incapacidad");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.DiasAutorizados).HasColumnName("dias_autorizados");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FolioImss)
                .HasMaxLength(30)
                .HasColumnName("folio_imss");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdTipoIncapacidad).HasColumnName("id_tipo_incapacidad");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.IncapacidadesImss)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_incapacidades_imss_id_empleado_fkey");

            entity.HasOne(d => d.IdTipoIncapacidadNavigation).WithMany(p => p.IncapacidadesImss)
                .HasForeignKey(d => d.IdTipoIncapacidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_incapacidades_imss_id_tipo_incapacidad_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.IncapacidadesImssIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_incapacidades_imss_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.IncapacidadesImssIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_incapacidades_imss_usuario_modificacion");
        });

        modelBuilder.Entity<IncidenciasNomina>(entity =>
        {
            entity.HasKey(e => e.IdIncidenciaNomina).HasName("tb_bi_incidencias_nomina_pkey");

            entity.ToTable("tb_bi_incidencias_nomina", "nomina");

            entity.Property(e => e.IdIncidenciaNomina)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_incidencia_nomina");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Cantidad)
                .HasPrecision(10, 2)
                .HasDefaultValue(1m)
                .HasColumnName("cantidad");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdConcepto).HasColumnName("id_concepto");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdPeriodoNomina).HasColumnName("id_periodo_nomina");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Importe)
                .HasPrecision(12, 2)
                .HasColumnName("importe");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(300)
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdConceptoNavigation).WithMany(p => p.IncidenciasNomina)
                .HasForeignKey(d => d.IdConcepto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_incidencias_nomina_id_concepto_fkey");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.IncidenciasNomina)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_incidencias_nomina_id_empleado_fkey");

            entity.HasOne(d => d.IdPeriodoNominaNavigation).WithMany(p => p.IncidenciasNomina)
                .HasForeignKey(d => d.IdPeriodoNomina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_incidencias_nomina_id_periodo_nomina_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.IncidenciasNominaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_incidencias_nomina_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.IncidenciasNominaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_incidencias_nomina_usuario_modificacion");
        });

        modelBuilder.Entity<Indicadores>(entity =>
        {
            entity.HasKey(e => e.IdIndicador).HasName("tb_bi_indicadores_pkey");

            entity.ToTable("tb_bi_indicadores", "nomina");

            entity.HasIndex(e => new { e.IdTipoIndicador, e.FechaInicio }, "ux_indicadores").IsUnique();

            entity.Property(e => e.IdIndicador)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_indicador");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.Fuente)
                .HasMaxLength(200)
                .HasColumnName("fuente");
            entity.Property(e => e.IdTipoIndicador).HasColumnName("id_tipo_indicador");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.ValorDiario)
                .HasPrecision(12, 4)
                .HasColumnName("valor_diario");

            entity.HasOne(d => d.IdTipoIndicadorNavigation).WithMany(p => p.Indicadores)
                .HasForeignKey(d => d.IdTipoIndicador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_indicadores_id_tipo_indicador_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.IndicadoresIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_indicadores_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.IndicadoresIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_indicadores_usuario_modificacion");
        });

        modelBuilder.Entity<LftVacaciones>(entity =>
        {
            entity.HasKey(e => e.IdLftVacacion).HasName("tb_bi_lft_vacaciones_pkey");

            entity.ToTable("tb_bi_lft_vacaciones", "recursos_humanos");

            entity.HasIndex(e => e.AnioAntiguedad, "tb_bi_lft_vacaciones_anio_antiguedad_key").IsUnique();

            entity.Property(e => e.IdLftVacacion)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_lft_vacacion");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AnioAntiguedad).HasColumnName("anio_antiguedad");
            entity.Property(e => e.DiasDeLey).HasColumnName("dias_de_ley");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.LftVacacionesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lft_vacaciones_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.LftVacacionesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_lft_vacaciones_usuario_modificacion");
        });

        modelBuilder.Entity<Materias>(entity =>
        {
            entity.HasKey(e => e.IdMateria).HasName("tb_bi_materias_pkey");

            entity.ToTable("tb_bi_materias", "recursos_humanos");

            entity.Property(e => e.IdMateria)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_materia");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombreMateria)
                .HasMaxLength(150)
                .HasColumnName("nombre_materia");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.MateriasIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_materias_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.MateriasIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_materias_usuario_modificacion");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("tb_bi_metodos_pago_pkey");

            entity.ToTable("tb_bi_metodos_pago", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_metodos_pago_clave_key").IsUnique();

            entity.Property(e => e.IdMetodoPago)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_metodo_pago");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.RequiereClabe).HasColumnName("requiere_clabe");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.MetodosPagoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_metodos_pago_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.MetodosPagoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_metodos_pago_usuario_modificacion");
        });

        modelBuilder.Entity<ModosChecada>(entity =>
        {
            entity.HasKey(e => e.IdModoChecada).HasName("tb_bi_modos_checada_pkey");

            entity.ToTable("tb_bi_modos_checada", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_modos_checada_clave_key").IsUnique();

            entity.Property(e => e.IdModoChecada)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_modo_checada");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ModosChecadaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_modos_checada_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ModosChecadaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_modos_checada_usuario_modificacion");
        });

        modelBuilder.Entity<MotivosBaja>(entity =>
        {
            entity.HasKey(e => e.IdMotivoBaja).HasName("tb_bi_motivos_baja_pkey");

            entity.ToTable("tb_bi_motivos_baja", "recursos_humanos");

            entity.HasIndex(e => e.DescripcionMotivo, "tb_bi_motivos_baja_descripcion_motivo_key").IsUnique();

            entity.Property(e => e.IdMotivoBaja)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_motivo_baja");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Aplica20DiasPorAnio).HasColumnName("aplica_20_dias_por_anio");
            entity.Property(e => e.AplicaFiniquito)
                .HasDefaultValue(true)
                .HasColumnName("aplica_finiquito");
            entity.Property(e => e.AplicaIndemnizacion90Dias).HasColumnName("aplica_indemnizacion_90_dias");
            entity.Property(e => e.DescripcionMotivo)
                .HasMaxLength(100)
                .HasColumnName("descripcion_motivo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdCausaBajaImss).HasColumnName("id_causa_baja_imss");
            entity.Property(e => e.IdReglaPrimaAntiguedad)
                .HasDefaultValue(1)
                .HasColumnName("id_regla_prima_antiguedad");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdCausaBajaImssNavigation).WithMany(p => p.MotivosBaja)
                .HasForeignKey(d => d.IdCausaBajaImss)
                .HasConstraintName("tb_bi_motivos_baja_id_causa_baja_imss_fkey");

            entity.HasOne(d => d.IdReglaPrimaAntiguedadNavigation).WithMany(p => p.MotivosBaja)
                .HasForeignKey(d => d.IdReglaPrimaAntiguedad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_motivos_baja_id_regla_prima_antiguedad_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.MotivosBajaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_motivos_baja_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.MotivosBajaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_motivos_baja_usuario_modificacion");
        });

        modelBuilder.Entity<MovimientosImss>(entity =>
        {
            entity.HasKey(e => e.IdMovimientoImss).HasName("tb_bi_movimientos_imss_pkey");

            entity.ToTable("tb_bi_movimientos_imss", "nomina");

            entity.HasIndex(e => new { e.IdEmpleado, e.FechaMovimiento }, "ix_movimientos_imss_empleado").IsDescending(false, true);

            entity.Property(e => e.IdMovimientoImss)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_movimiento_imss");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AcuseIdse)
                .HasMaxLength(100)
                .HasColumnName("acuse_idse");
            entity.Property(e => e.EnviadoIdse).HasColumnName("enviado_idse");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaEnvioIdse).HasColumnName("fecha_envio_idse");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaMovimiento).HasColumnName("fecha_movimiento");
            entity.Property(e => e.IdCausaBajaImss).HasColumnName("id_causa_baja_imss");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdTipoMovimientoImss).HasColumnName("id_tipo_movimiento_imss");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.SalarioBaseCotizacion)
                .HasPrecision(12, 2)
                .HasColumnName("salario_base_cotizacion");
            entity.Property(e => e.SalarioDiario)
                .HasPrecision(12, 2)
                .HasColumnName("salario_diario");
            entity.Property(e => e.SalarioDiarioIntegrado)
                .HasPrecision(12, 2)
                .HasColumnName("salario_diario_integrado");

            entity.HasOne(d => d.IdCausaBajaImssNavigation).WithMany(p => p.MovimientosImss)
                .HasForeignKey(d => d.IdCausaBajaImss)
                .HasConstraintName("tb_bi_movimientos_imss_id_causa_baja_imss_fkey");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.MovimientosImss)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_movimientos_imss_id_empleado_fkey");

            entity.HasOne(d => d.IdTipoMovimientoImssNavigation).WithMany(p => p.MovimientosImss)
                .HasForeignKey(d => d.IdTipoMovimientoImss)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_movimientos_imss_id_tipo_movimiento_imss_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.MovimientosImssIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_movimientos_imss_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.MovimientosImssIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_movimientos_imss_usuario_modificacion");
        });

        modelBuilder.Entity<NominaCuotasImss>(entity =>
        {
            entity.HasKey(e => e.IdNominaCuota).HasName("tb_bi_nomina_cuotas_imss_pkey");

            entity.ToTable("tb_bi_nomina_cuotas_imss", "nomina");

            entity.HasIndex(e => e.IdNomina, "ix_nomina_cuotas_nomina");

            entity.Property(e => e.IdNominaCuota)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_nomina_cuota");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Base)
                .HasPrecision(12, 2)
                .HasColumnName("base");
            entity.Property(e => e.ClaveRamo)
                .HasMaxLength(20)
                .HasColumnName("clave_ramo");
            entity.Property(e => e.CuotaObrero)
                .HasPrecision(12, 2)
                .HasColumnName("cuota_obrero");
            entity.Property(e => e.CuotaPatron)
                .HasPrecision(12, 2)
                .HasColumnName("cuota_patron");
            entity.Property(e => e.DiasCotizados)
                .HasPrecision(5, 2)
                .HasColumnName("dias_cotizados");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdNomina).HasColumnName("id_nomina");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdNominaNavigation).WithMany(p => p.NominaCuotasImss)
                .HasForeignKey(d => d.IdNomina)
                .HasConstraintName("tb_bi_nomina_cuotas_imss_id_nomina_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.NominaCuotasImssIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_nomina_cuotas_imss_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.NominaCuotasImssIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_nomina_cuotas_imss_usuario_modificacion");
        });

        modelBuilder.Entity<NominaDetalle>(entity =>
        {
            entity.HasKey(e => e.IdNominaDetalle).HasName("tb_bi_nomina_detalle_pkey");

            entity.ToTable("tb_bi_nomina_detalle", "nomina");

            entity.HasIndex(e => e.IdNomina, "ix_nomina_detalle_nomina");

            entity.Property(e => e.IdNominaDetalle)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_nomina_detalle");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Cantidad)
                .HasPrecision(10, 2)
                .HasColumnName("cantidad");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdConcepto).HasColumnName("id_concepto");
            entity.Property(e => e.IdCreditoFonacot).HasColumnName("id_credito_fonacot");
            entity.Property(e => e.IdCreditoInfonavit).HasColumnName("id_credito_infonavit");
            entity.Property(e => e.IdHistConcepto).HasColumnName("id_hist_concepto");
            entity.Property(e => e.IdNomina).HasColumnName("id_nomina");
            entity.Property(e => e.IdPension).HasColumnName("id_pension");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.ImporteExento)
                .HasPrecision(14, 2)
                .HasColumnName("importe_exento");
            entity.Property(e => e.ImporteGravado)
                .HasPrecision(14, 2)
                .HasColumnName("importe_gravado");
            entity.Property(e => e.ImporteTotal)
                .HasPrecision(14, 2)
                .HasComputedColumnSql("(importe_gravado + importe_exento)", true)
                .HasColumnName("importe_total");

            entity.HasOne(d => d.IdConceptoNavigation).WithMany(p => p.NominaDetalle)
                .HasForeignKey(d => d.IdConcepto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_nomina_detalle_id_concepto_fkey");

            entity.HasOne(d => d.IdCreditoFonacotNavigation).WithMany(p => p.NominaDetalle)
                .HasForeignKey(d => d.IdCreditoFonacot)
                .HasConstraintName("tb_bi_nomina_detalle_id_credito_fonacot_fkey");

            entity.HasOne(d => d.IdCreditoInfonavitNavigation).WithMany(p => p.NominaDetalle)
                .HasForeignKey(d => d.IdCreditoInfonavit)
                .HasConstraintName("tb_bi_nomina_detalle_id_credito_infonavit_fkey");

            entity.HasOne(d => d.IdHistConceptoNavigation).WithMany(p => p.NominaDetalle)
                .HasForeignKey(d => d.IdHistConcepto)
                .HasConstraintName("tb_bi_nomina_detalle_id_hist_concepto_fkey");

            entity.HasOne(d => d.IdNominaNavigation).WithMany(p => p.NominaDetalle)
                .HasForeignKey(d => d.IdNomina)
                .HasConstraintName("tb_bi_nomina_detalle_id_nomina_fkey");

            entity.HasOne(d => d.IdPensionNavigation).WithMany(p => p.NominaDetalle)
                .HasForeignKey(d => d.IdPension)
                .HasConstraintName("tb_bi_nomina_detalle_id_pension_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.NominaDetalleIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_nomina_detalle_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.NominaDetalleIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_nomina_detalle_usuario_modificacion");
        });

        modelBuilder.Entity<Nominas>(entity =>
        {
            entity.HasKey(e => e.IdNomina).HasName("tb_bi_nominas_pkey");

            entity.ToTable("tb_bi_nominas", "nomina");

            entity.HasIndex(e => e.IdEmpleado, "ix_nominas_empleado");

            entity.HasIndex(e => e.Uuid, "tb_bi_nominas_uuid_key").IsUnique();

            entity.HasIndex(e => new { e.IdPeriodoNomina, e.IdEmpleado }, "ux_nominas").IsUnique();

            entity.Property(e => e.IdNomina)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_nomina");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.DiasFalta)
                .HasPrecision(6, 2)
                .HasColumnName("dias_falta");
            entity.Property(e => e.DiasIncapacidad)
                .HasPrecision(6, 2)
                .HasColumnName("dias_incapacidad");
            entity.Property(e => e.DiasPagados)
                .HasPrecision(6, 2)
                .HasColumnName("dias_pagados");
            entity.Property(e => e.EstatusRecibo)
                .HasMaxLength(15)
                .HasDefaultValueSql("'calculado'::character varying")
                .HasColumnName("estatus_recibo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaTimbrado).HasColumnName("fecha_timbrado");
            entity.Property(e => e.Folio).HasColumnName("folio");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdPeriodoNomina).HasColumnName("id_periodo_nomina");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.ImssObrero)
                .HasPrecision(12, 2)
                .HasColumnName("imss_obrero");
            entity.Property(e => e.ImssPatronal)
                .HasPrecision(12, 2)
                .HasColumnName("imss_patronal");
            entity.Property(e => e.IsrCausado)
                .HasPrecision(12, 2)
                .HasColumnName("isr_causado");
            entity.Property(e => e.IsrRetenido)
                .HasPrecision(12, 2)
                .HasColumnName("isr_retenido");
            entity.Property(e => e.MensajeTimbrado)
                .HasMaxLength(500)
                .HasColumnName("mensaje_timbrado");
            entity.Property(e => e.NetoAPagar)
                .HasPrecision(14, 2)
                .HasColumnName("neto_a_pagar");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .HasColumnName("observaciones");
            entity.Property(e => e.RutaPdf)
                .HasMaxLength(300)
                .HasColumnName("ruta_pdf");
            entity.Property(e => e.RutaXml)
                .HasMaxLength(300)
                .HasColumnName("ruta_xml");
            entity.Property(e => e.SalarioBaseCotizacion)
                .HasPrecision(12, 2)
                .HasColumnName("salario_base_cotizacion");
            entity.Property(e => e.SalarioDiario)
                .HasPrecision(12, 2)
                .HasColumnName("salario_diario");
            entity.Property(e => e.SalarioDiarioIntegrado)
                .HasPrecision(12, 2)
                .HasColumnName("salario_diario_integrado");
            entity.Property(e => e.Serie)
                .HasMaxLength(10)
                .HasColumnName("serie");
            entity.Property(e => e.SubsidioCausado)
                .HasPrecision(12, 2)
                .HasColumnName("subsidio_causado");
            entity.Property(e => e.TotalDeducciones)
                .HasPrecision(14, 2)
                .HasColumnName("total_deducciones");
            entity.Property(e => e.TotalExento)
                .HasPrecision(14, 2)
                .HasColumnName("total_exento");
            entity.Property(e => e.TotalGravado)
                .HasPrecision(14, 2)
                .HasColumnName("total_gravado");
            entity.Property(e => e.TotalOtrosPagos)
                .HasPrecision(14, 2)
                .HasColumnName("total_otros_pagos");
            entity.Property(e => e.TotalPercepciones)
                .HasPrecision(14, 2)
                .HasColumnName("total_percepciones");
            entity.Property(e => e.Uuid).HasColumnName("uuid");
            entity.Property(e => e.UuidSustituido).HasColumnName("uuid_sustituido");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Nominas)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_nominas_id_empleado_fkey");

            entity.HasOne(d => d.IdPeriodoNominaNavigation).WithMany(p => p.Nominas)
                .HasForeignKey(d => d.IdPeriodoNomina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_nominas_id_periodo_nomina_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.NominasIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_nominas_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.NominasIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_nominas_usuario_modificacion");
        });

        modelBuilder.Entity<OrigenesChecada>(entity =>
        {
            entity.HasKey(e => e.IdOrigenChecada).HasName("tb_bi_origenes_checada_pkey");

            entity.ToTable("tb_bi_origenes_checada", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_origenes_checada_clave_key").IsUnique();

            entity.Property(e => e.IdOrigenChecada)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_origen_checada");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.RequiereChecador)
                .HasDefaultValue(true)
                .HasColumnName("requiere_checador");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.OrigenesChecadaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_origenes_checada_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.OrigenesChecadaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_origenes_checada_usuario_modificacion");
        });

        modelBuilder.Entity<PensionesAlimenticias>(entity =>
        {
            entity.HasKey(e => e.IdPension).HasName("tb_bi_pensiones_alimenticias_pkey");

            entity.ToTable("tb_bi_pensiones_alimenticias", "nomina");

            entity.Property(e => e.IdPension)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_pension");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Beneficiario)
                .HasMaxLength(200)
                .HasColumnName("beneficiario");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdTipoDescuento).HasColumnName("id_tipo_descuento");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.OficioJuzgado)
                .HasMaxLength(100)
                .HasColumnName("oficio_juzgado");
            entity.Property(e => e.Valor)
                .HasPrecision(10, 4)
                .HasColumnName("valor");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.PensionesAlimenticias)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_pensiones_alimenticias_id_empleado_fkey");

            entity.HasOne(d => d.IdTipoDescuentoNavigation).WithMany(p => p.PensionesAlimenticias)
                .HasForeignKey(d => d.IdTipoDescuento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_pensiones_alimenticias_id_tipo_descuento_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PensionesAlimenticiasIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_pensiones_alimenticias_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PensionesAlimenticiasIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_pensiones_alimenticias_usuario_modificacion");
        });

        modelBuilder.Entity<Perfiles>(entity =>
        {
            entity.HasKey(e => e.IdPerfil).HasName("tb_bi_perfiles_pkey");

            entity.ToTable("tb_bi_perfiles", "recursos_humanos");

            entity.HasIndex(e => e.NombrePerfil, "tb_bi_perfiles_nombre_perfil_key").IsUnique();

            entity.Property(e => e.IdPerfil)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_perfil");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombrePerfil)
                .HasMaxLength(60)
                .HasColumnName("nombre_perfil");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PerfilesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_perfiles_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PerfilesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_perfiles_usuario_modificacion");
        });

        modelBuilder.Entity<PeriodicidadesPago>(entity =>
        {
            entity.HasKey(e => e.IdPeriodicidadPago).HasName("tb_bi_periodicidades_pago_pkey");

            entity.ToTable("tb_bi_periodicidades_pago", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_periodicidades_pago_clave_key").IsUnique();

            entity.Property(e => e.IdPeriodicidadPago)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_periodicidad_pago");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(15)
                .HasColumnName("clave");
            entity.Property(e => e.ClaveSat)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("clave_sat");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.Dias)
                .HasPrecision(5, 2)
                .HasColumnName("dias");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PeriodicidadesPagoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_periodicidades_pago_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PeriodicidadesPagoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_periodicidades_pago_usuario_modificacion");
        });

        modelBuilder.Entity<PeriodosAcumulacion>(entity =>
        {
            entity.HasKey(e => e.IdPeriodoAcumulacion).HasName("tb_bi_periodos_acumulacion_pkey");

            entity.ToTable("tb_bi_periodos_acumulacion", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_periodos_acumulacion_clave_key").IsUnique();

            entity.Property(e => e.IdPeriodoAcumulacion)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_periodo_acumulacion");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PeriodosAcumulacionIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_periodos_acumulacion_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PeriodosAcumulacionIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_periodos_acumulacion_usuario_modificacion");
        });

        modelBuilder.Entity<PeriodosEscolares>(entity =>
        {
            entity.HasKey(e => e.IdPeriodo).HasName("tb_bi_periodos_escolares_pkey");

            entity.ToTable("tb_bi_periodos_escolares", "recursos_humanos");

            entity.HasIndex(e => e.NombrePeriodo, "tb_bi_periodos_escolares_nombre_periodo_key").IsUnique();

            entity.Property(e => e.IdPeriodo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_periodo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombrePeriodo)
                .HasMaxLength(100)
                .HasColumnName("nombre_periodo");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PeriodosEscolaresIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_periodos_escolares_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PeriodosEscolaresIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_periodos_escolares_usuario_modificacion");
        });

        modelBuilder.Entity<PeriodosNomina>(entity =>
        {
            entity.HasKey(e => e.IdPeriodoNomina).HasName("tb_bi_periodos_nomina_pkey");

            entity.ToTable("tb_bi_periodos_nomina", "nomina");

            entity.HasIndex(e => new { e.IdEmpresa, e.IdTipoNomina, e.IdPeriodicidadPago, e.Ejercicio, e.NumeroPeriodo }, "ux_periodos_numero").IsUnique();

            entity.Property(e => e.IdPeriodoNomina)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_periodo_nomina");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.DiasPago)
                .HasPrecision(5, 2)
                .HasDefaultValue(15m)
                .HasColumnName("dias_pago");
            entity.Property(e => e.Ejercicio).HasColumnName("ejercicio");
            entity.Property(e => e.EstatusCierre)
                .HasMaxLength(15)
                .HasDefaultValueSql("'abierto'::character varying")
                .HasColumnName("estatus_cierre");
            entity.Property(e => e.FechaAutorizacion).HasColumnName("fecha_autorizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");
            entity.Property(e => e.IdAutorizadorNomina).HasColumnName("id_autorizador_nomina");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdPeriodicidadPago)
                .HasDefaultValue(3)
                .HasColumnName("id_periodicidad_pago");
            entity.Property(e => e.IdTipoNomina)
                .HasDefaultValue(1)
                .HasColumnName("id_tipo_nomina");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NumeroPeriodo).HasColumnName("numero_periodo");

            entity.HasOne(d => d.IdAutorizadorNominaNavigation).WithMany(p => p.PeriodosNomina)
                .HasForeignKey(d => d.IdAutorizadorNomina)
                .HasConstraintName("tb_bi_periodos_nomina_id_autorizador_nomina_fkey");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.PeriodosNomina)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_periodos_nomina_id_empresa_fkey");

            entity.HasOne(d => d.IdPeriodicidadPagoNavigation).WithMany(p => p.PeriodosNomina)
                .HasForeignKey(d => d.IdPeriodicidadPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_periodos_nomina_id_periodicidad_pago_fkey");

            entity.HasOne(d => d.IdTipoNominaNavigation).WithMany(p => p.PeriodosNomina)
                .HasForeignKey(d => d.IdTipoNomina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_periodos_nomina_id_tipo_nomina_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PeriodosNominaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_periodos_nomina_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PeriodosNominaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_periodos_nomina_usuario_modificacion");
        });

        modelBuilder.Entity<Permisos>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("tb_bi_permisos_pkey");

            entity.ToTable("tb_bi_permisos", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_permisos_clave_key").IsUnique();

            entity.Property(e => e.IdPermiso)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_permiso");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(50)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Modulo)
                .HasMaxLength(50)
                .HasColumnName("modulo");
            entity.Property(e => e.UsaAlcance).HasColumnName("usa_alcance");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PermisosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_permisos_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PermisosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_permisos_usuario_modificacion");
        });

        modelBuilder.Entity<Personas>(entity =>
        {
            entity.HasKey(e => e.IdPersona).HasName("tb_bi_personas_pkey");

            entity.ToTable("tb_bi_personas", "recursos_humanos");

            entity.HasIndex(e => e.Curp, "tb_bi_personas_curp_key").IsUnique();

            entity.HasIndex(e => e.Rfc, "tb_bi_personas_rfc_key").IsUnique();

            entity.Property(e => e.IdPersona)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_persona");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(100)
                .HasColumnName("apellido_materno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(100)
                .HasColumnName("apellido_paterno");
            entity.Property(e => e.CorreoElectronico)
                .HasMaxLength(150)
                .HasColumnName("correo_electronico");
            entity.Property(e => e.Curp)
                .HasMaxLength(18)
                .IsFixedLength()
                .HasColumnName("curp");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .HasColumnName("rfc");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PersonasIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_personas_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PersonasIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_personas_usuario_modificacion");
        });

        modelBuilder.Entity<PrimasRiesgoTrabajo>(entity =>
        {
            entity.HasKey(e => e.IdPrimaRiesgo).HasName("tb_bi_primas_riesgo_trabajo_pkey");

            entity.ToTable("tb_bi_primas_riesgo_trabajo", "nomina");

            entity.HasIndex(e => new { e.IdRegistroPatronal, e.FechaInicio }, "ux_primas_riesgo").IsUnique();

            entity.Property(e => e.IdPrimaRiesgo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_prima_riesgo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdRegistroPatronal).HasColumnName("id_registro_patronal");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Prima)
                .HasPrecision(8, 5)
                .HasColumnName("prima");

            entity.HasOne(d => d.IdRegistroPatronalNavigation).WithMany(p => p.PrimasRiesgoTrabajo)
                .HasForeignKey(d => d.IdRegistroPatronal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_primas_riesgo_trabajo_id_registro_patronal_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PrimasRiesgoTrabajoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_primas_riesgo_trabajo_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PrimasRiesgoTrabajoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_primas_riesgo_trabajo_usuario_modificacion");
        });

        modelBuilder.Entity<ProveedoresAutenticacion>(entity =>
        {
            entity.HasKey(e => e.IdProveedorAutenticacion).HasName("tb_bi_proveedores_autenticacion_pkey");

            entity.ToTable("tb_bi_proveedores_autenticacion", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_proveedores_autenticacion_clave_key").IsUnique();

            entity.Property(e => e.IdProveedorAutenticacion)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_proveedor_autenticacion");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.RequierePassword).HasColumnName("requiere_password");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ProveedoresAutenticacionIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_proveedores_autenticacion_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ProveedoresAutenticacionIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_proveedores_autenticacion_usuario_modificacion");
        });

        modelBuilder.Entity<Ptu>(entity =>
        {
            entity.HasKey(e => e.IdPtu).HasName("tb_bi_ptu_pkey");

            entity.ToTable("tb_bi_ptu", "nomina");

            entity.HasIndex(e => new { e.IdEmpresa, e.Ejercicio }, "ux_ptu").IsUnique();

            entity.Property(e => e.IdPtu)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_ptu");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Ejercicio).HasColumnName("ejercicio");
            entity.Property(e => e.Estatus)
                .HasMaxLength(15)
                .HasDefaultValueSql("'borrador'::character varying")
                .HasColumnName("estatus");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdPeriodoNomina).HasColumnName("id_periodo_nomina");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.MontoRepartir)
                .HasPrecision(16, 2)
                .HasComputedColumnSql("round((renta_gravable * 0.10), 2)", true)
                .HasColumnName("monto_repartir");
            entity.Property(e => e.RentaGravable)
                .HasPrecision(16, 2)
                .HasColumnName("renta_gravable");
            entity.Property(e => e.TopeSalarioConfianza)
                .HasPrecision(12, 2)
                .HasColumnName("tope_salario_confianza");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Ptu)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_ptu_id_empresa_fkey");

            entity.HasOne(d => d.IdPeriodoNominaNavigation).WithMany(p => p.Ptu)
                .HasForeignKey(d => d.IdPeriodoNomina)
                .HasConstraintName("tb_bi_ptu_id_periodo_nomina_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PtuIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_ptu_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PtuIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_ptu_usuario_modificacion");
        });

        modelBuilder.Entity<PtuEmpleados>(entity =>
        {
            entity.HasKey(e => e.IdPtuEmpleado).HasName("tb_bi_ptu_empleados_pkey");

            entity.ToTable("tb_bi_ptu_empleados", "nomina");

            entity.HasIndex(e => new { e.IdPtu, e.IdEmpleado }, "ux_ptu_empleados").IsUnique();

            entity.Property(e => e.IdPtuEmpleado)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_ptu_empleado");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.DiasTrabajados).HasColumnName("dias_trabajados");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdPtu).HasColumnName("id_ptu");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.PromedioTresAnios)
                .HasPrecision(14, 2)
                .HasColumnName("promedio_tres_anios");
            entity.Property(e => e.PtuCalculada)
                .HasPrecision(14, 2)
                .HasColumnName("ptu_calculada");
            entity.Property(e => e.PtuFinal)
                .HasPrecision(14, 2)
                .HasColumnName("ptu_final");
            entity.Property(e => e.PtuPorDias)
                .HasPrecision(14, 2)
                .HasColumnName("ptu_por_dias");
            entity.Property(e => e.PtuPorSalarios)
                .HasPrecision(14, 2)
                .HasColumnName("ptu_por_salarios");
            entity.Property(e => e.SalarioAnual)
                .HasPrecision(14, 2)
                .HasColumnName("salario_anual");
            entity.Property(e => e.TopeTresMeses)
                .HasPrecision(14, 2)
                .HasColumnName("tope_tres_meses");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.PtuEmpleados)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_ptu_empleados_id_empleado_fkey");

            entity.HasOne(d => d.IdPtuNavigation).WithMany(p => p.PtuEmpleados)
                .HasForeignKey(d => d.IdPtu)
                .HasConstraintName("tb_bi_ptu_empleados_id_ptu_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PtuEmpleadosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_ptu_empleados_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PtuEmpleadosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_ptu_empleados_usuario_modificacion");
        });

        modelBuilder.Entity<Puestos>(entity =>
        {
            entity.HasKey(e => e.IdPuesto).HasName("tb_bi_puestos_pkey");

            entity.ToTable("tb_bi_puestos", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_puestos_clave_key").IsUnique();

            entity.HasIndex(e => e.NombrePuesto, "tb_bi_puestos_nombre_puesto_key").IsUnique();

            entity.Property(e => e.IdPuesto)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_puesto");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombrePuesto)
                .HasMaxLength(100)
                .HasColumnName("nombre_puesto");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PuestosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_puestos_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PuestosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_puestos_usuario_modificacion");
        });

        modelBuilder.Entity<RegimenesFiscales>(entity =>
        {
            entity.HasKey(e => e.IdRegimenFiscal).HasName("tb_bi_regimenes_fiscales_pkey");

            entity.ToTable("tb_bi_regimenes_fiscales", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_regimenes_fiscales_clave_key").IsUnique();

            entity.Property(e => e.IdRegimenFiscal)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_regimen_fiscal");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.PersonaFisica).HasColumnName("persona_fisica");
            entity.Property(e => e.PersonaMoral).HasColumnName("persona_moral");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.RegimenesFiscalesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_regimenes_fiscales_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.RegimenesFiscalesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_regimenes_fiscales_usuario_modificacion");
        });

        modelBuilder.Entity<RegistrosPatronales>(entity =>
        {
            entity.HasKey(e => e.IdRegistroPatronal).HasName("tb_bi_registros_patronales_pkey");

            entity.ToTable("tb_bi_registros_patronales", "nomina");

            entity.HasIndex(e => e.RegistroPatronal, "tb_bi_registros_patronales_registro_patronal_key").IsUnique();

            entity.Property(e => e.IdRegistroPatronal)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_registro_patronal");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdClaseRiesgo).HasColumnName("id_clase_riesgo");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.IdZonaSalarioMinimo)
                .HasDefaultValue(1)
                .HasColumnName("id_zona_salario_minimo");
            entity.Property(e => e.RegistroPatronal)
                .HasMaxLength(11)
                .HasColumnName("registro_patronal");

            entity.HasOne(d => d.IdClaseRiesgoNavigation).WithMany(p => p.RegistrosPatronales)
                .HasForeignKey(d => d.IdClaseRiesgo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_registros_patronales_id_clase_riesgo_fkey");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.RegistrosPatronales)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_registros_patronales_id_empresa_fkey");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.RegistrosPatronales)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_registros_patronales_id_estado_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.RegistrosPatronalesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_registros_patronales_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.RegistrosPatronalesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_registros_patronales_usuario_modificacion");

            entity.HasOne(d => d.IdZonaSalarioMinimoNavigation).WithMany(p => p.RegistrosPatronales)
                .HasForeignKey(d => d.IdZonaSalarioMinimo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_registros_patronales_id_zona_salario_minimo_fkey");
        });

        modelBuilder.Entity<ReglasAsistencia>(entity =>
        {
            entity.HasKey(e => e.IdReglaAsistencia).HasName("tb_bi_reglas_asistencia_pkey");

            entity.ToTable("tb_bi_reglas_asistencia", "recursos_humanos");

            entity.Property(e => e.IdReglaAsistencia)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_regla_asistencia");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AplicaSancionFalta)
                .HasDefaultValue(true)
                .HasColumnName("aplica_sancion_falta");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFinVigencia).HasColumnName("fecha_fin_vigencia");
            entity.Property(e => e.FechaInicioVigencia).HasColumnName("fecha_inicio_vigencia");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdModoChecada)
                .HasDefaultValue(1)
                .HasColumnName("id_modo_checada");
            entity.Property(e => e.IdPeriodoAcumulacion)
                .HasDefaultValue(1)
                .HasColumnName("id_periodo_acumulacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.MaximoRetardosPermitidos)
                .HasDefaultValue(3)
                .HasColumnName("maximo_retardos_permitidos");
            entity.Property(e => e.MinutosLimiteFalta)
                .HasDefaultValue(15)
                .HasColumnName("minutos_limite_falta");
            entity.Property(e => e.MinutosTolerancia)
                .HasDefaultValue(10)
                .HasColumnName("minutos_tolerancia");
            entity.Property(e => e.NombreRegla)
                .HasMaxLength(100)
                .HasColumnName("nombre_regla");

            entity.HasOne(d => d.IdModoChecadaNavigation).WithMany(p => p.ReglasAsistencia)
                .HasForeignKey(d => d.IdModoChecada)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_reglas_asistencia_id_modo_checada_fkey");

            entity.HasOne(d => d.IdPeriodoAcumulacionNavigation).WithMany(p => p.ReglasAsistencia)
                .HasForeignKey(d => d.IdPeriodoAcumulacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_reglas_asistencia_id_periodo_acumulacion_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ReglasAsistenciaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reglas_asistencia_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ReglasAsistenciaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_reglas_asistencia_usuario_modificacion");
        });

        modelBuilder.Entity<ReglasPrimaAntiguedad>(entity =>
        {
            entity.HasKey(e => e.IdReglaPrimaAntiguedad).HasName("tb_bi_reglas_prima_antiguedad_pkey");

            entity.ToTable("tb_bi_reglas_prima_antiguedad", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_reglas_prima_antiguedad_clave_key").IsUnique();

            entity.Property(e => e.IdReglaPrimaAntiguedad)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_regla_prima_antiguedad");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AniosMinimos).HasColumnName("anios_minimos");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ReglasPrimaAntiguedadIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reglas_prima_antiguedad_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ReglasPrimaAntiguedadIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_reglas_prima_antiguedad_usuario_modificacion");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("tb_bi_roles_pkey");

            entity.ToTable("tb_bi_roles", "recursos_humanos");

            entity.HasIndex(e => e.NombreRol, "tb_bi_roles_nombre_rol_key").IsUnique();

            entity.HasIndex(e => e.Clave, "ux_roles_clave").IsUnique();

            entity.Property(e => e.IdRol)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_rol");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(30)
                .HasColumnName("clave");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(60)
                .HasColumnName("nombre_rol");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.RolesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_roles_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.RolesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_roles_usuario_modificacion");
        });

        modelBuilder.Entity<RolesGruposDirectorio>(entity =>
        {
            entity.HasKey(e => e.IdRolGrupoDirectorio).HasName("tb_bi_roles_grupos_directorio_pkey");

            entity.ToTable("tb_bi_roles_grupos_directorio", "recursos_humanos");

            entity.HasIndex(e => new { e.IdRol, e.NombreGrupo }, "ux_roles_grupos").IsUnique();

            entity.Property(e => e.IdRolGrupoDirectorio)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_rol_grupo_directorio");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NombreGrupo)
                .HasMaxLength(100)
                .HasColumnName("nombre_grupo");
            entity.Property(e => e.ObjectGuidGrupo).HasColumnName("object_guid_grupo");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.RolesGruposDirectorio)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_roles_grupos_directorio_id_rol_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.RolesGruposDirectorioIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_roles_grupos_directorio_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.RolesGruposDirectorioIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_roles_grupos_directorio_usuario_modificacion");
        });

        modelBuilder.Entity<RolesPermisos>(entity =>
        {
            entity.HasKey(e => e.IdRolPermiso).HasName("tb_bi_roles_permisos_pkey");

            entity.ToTable("tb_bi_roles_permisos", "recursos_humanos");

            entity.HasIndex(e => new { e.IdRol, e.IdPermiso }, "ux_roles_permisos").IsUnique();

            entity.Property(e => e.IdRolPermiso)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_rol_permiso");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Alcance)
                .HasMaxLength(10)
                .HasColumnName("alcance");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdPermiso).HasColumnName("id_permiso");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdPermisoNavigation).WithMany(p => p.RolesPermisos)
                .HasForeignKey(d => d.IdPermiso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_roles_permisos_id_permiso_fkey");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.RolesPermisos)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_roles_permisos_id_rol_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.RolesPermisosIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_roles_permisos_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.RolesPermisosIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_roles_permisos_usuario_modificacion");
        });

        modelBuilder.Entity<SaldosVacaciones>(entity =>
        {
            entity.HasKey(e => e.IdSaldoVacacion).HasName("tb_bi_saldos_vacaciones_pkey");

            entity.ToTable("tb_bi_saldos_vacaciones", "recursos_humanos");

            entity.HasIndex(e => new { e.IdEmpleado, e.AnioServicio }, "ux_saldos_vacaciones").IsUnique();

            entity.Property(e => e.IdSaldoVacacion)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_saldo_vacacion");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AnioServicio).HasColumnName("anio_servicio");
            entity.Property(e => e.DiasDisfrutados).HasColumnName("dias_disfrutados");
            entity.Property(e => e.DiasExcepcion).HasColumnName("dias_excepcion");
            entity.Property(e => e.DiasOtorgados).HasColumnName("dias_otorgados");
            entity.Property(e => e.FechaCaducidad).HasColumnName("fecha_caducidad");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdLftVacacion).HasColumnName("id_lft_vacacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.PrimaPagada).HasColumnName("prima_pagada");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.SaldosVacaciones)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_saldos_vacaciones_id_empleado_fkey");

            entity.HasOne(d => d.IdLftVacacionNavigation).WithMany(p => p.SaldosVacaciones)
                .HasForeignKey(d => d.IdLftVacacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_saldos_vacaciones_id_lft_vacacion_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.SaldosVacacionesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_saldos_vacaciones_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.SaldosVacacionesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_saldos_vacaciones_usuario_modificacion");
        });

        modelBuilder.Entity<SincronizacionDirectorioLogs>(entity =>
        {
            entity.HasKey(e => e.IdSincronizacionLog).HasName("tb_bi_sincronizacion_directorio_logs_pkey");

            entity.ToTable("tb_bi_sincronizacion_directorio_logs", "recursos_humanos");

            entity.HasIndex(e => new { e.IdEmpleado, e.FechaSincronizacion }, "ix_sincronizacion_logs_empleado").IsDescending(false, true);

            entity.Property(e => e.IdSincronizacionLog)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_sincronizacion_log");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AtributosEnviados)
                .HasColumnType("jsonb")
                .HasColumnName("atributos_enviados");
            entity.Property(e => e.Exitoso).HasColumnName("exitoso");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaSincronizacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_sincronizacion");
            entity.Property(e => e.IdAccionDirectorio).HasColumnName("id_accion_directorio");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(1000)
                .HasColumnName("mensaje");

            entity.HasOne(d => d.IdAccionDirectorioNavigation).WithMany(p => p.SincronizacionDirectorioLogs)
                .HasForeignKey(d => d.IdAccionDirectorio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_sincronizacion_directorio_logs_id_accion_directorio_fkey");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.SincronizacionDirectorioLogs)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_sincronizacion_directorio_logs_id_empleado_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.SincronizacionDirectorioLogsIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sincronizacion_directorio_logs_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.SincronizacionDirectorioLogsIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_sincronizacion_directorio_logs_usuario_modificacion");
        });

        modelBuilder.Entity<Solicitudes>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("tb_bi_solicitudes_pkey");

            entity.ToTable("tb_bi_solicitudes", "recursos_humanos");

            entity.HasIndex(e => new { e.IdEmpleado, e.FechaInicio, e.FechaFin }, "ix_solicitudes_empleado_fechas");

            entity.HasIndex(e => e.EstatusAprobacion, "ix_solicitudes_pendientes").HasFilter("((estatus_aprobacion)::text = ANY ((ARRAY['pendiente'::character varying, 'aprobada_jefe'::character varying])::text[]))");

            entity.Property(e => e.IdSolicitud)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_solicitud");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.DiasSolicitados)
                .HasPrecision(5, 1)
                .HasColumnName("dias_solicitados");
            entity.Property(e => e.EstatusAprobacion)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pendiente'::character varying")
                .HasColumnName("estatus_aprobacion");
            entity.Property(e => e.FechaAprobacionJefe).HasColumnName("fecha_aprobacion_jefe");
            entity.Property(e => e.FechaAprobacionRh).HasColumnName("fecha_aprobacion_rh");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.HoraFinPermiso).HasColumnName("hora_fin_permiso");
            entity.Property(e => e.HoraInicioPermiso).HasColumnName("hora_inicio_permiso");
            entity.Property(e => e.HorasSolicitadas)
                .HasPrecision(5, 2)
                .HasColumnName("horas_solicitadas");
            entity.Property(e => e.IdAsistenciaOmision).HasColumnName("id_asistencia_omision");
            entity.Property(e => e.IdAutorizadorJefe).HasColumnName("id_autorizador_jefe");
            entity.Property(e => e.IdAutorizadorRh).HasColumnName("id_autorizador_rh");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdTipoPermiso).HasColumnName("id_tipo_permiso");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.MotivoRechazo)
                .HasMaxLength(500)
                .HasColumnName("motivo_rechazo");
            entity.Property(e => e.RutaComprobanteAdjunto)
                .HasMaxLength(500)
                .HasColumnName("ruta_comprobante_adjunto");

            entity.HasOne(d => d.IdAsistenciaOmisionNavigation).WithMany(p => p.Solicitudes)
                .HasForeignKey(d => d.IdAsistenciaOmision)
                .HasConstraintName("tb_bi_solicitudes_id_asistencia_omision_fkey");

            entity.HasOne(d => d.IdAutorizadorJefeNavigation).WithMany(p => p.SolicitudesIdAutorizadorJefeNavigation)
                .HasForeignKey(d => d.IdAutorizadorJefe)
                .HasConstraintName("tb_bi_solicitudes_id_autorizador_jefe_fkey");

            entity.HasOne(d => d.IdAutorizadorRhNavigation).WithMany(p => p.SolicitudesIdAutorizadorRhNavigation)
                .HasForeignKey(d => d.IdAutorizadorRh)
                .HasConstraintName("tb_bi_solicitudes_id_autorizador_rh_fkey");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.SolicitudesIdEmpleadoNavigation)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_solicitudes_id_empleado_fkey");

            entity.HasOne(d => d.IdTipoPermisoNavigation).WithMany(p => p.Solicitudes)
                .HasForeignKey(d => d.IdTipoPermiso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_solicitudes_id_tipo_permiso_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.SolicitudesIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_solicitudes_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.SolicitudesIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_solicitudes_usuario_modificacion");
        });

        modelBuilder.Entity<SubsidioEmpleo>(entity =>
        {
            entity.HasKey(e => e.IdSubsidio).HasName("tb_bi_subsidio_empleo_pkey");

            entity.ToTable("tb_bi_subsidio_empleo", "nomina");

            entity.HasIndex(e => e.FechaInicio, "tb_bi_subsidio_empleo_fecha_inicio_key").IsUnique();

            entity.Property(e => e.IdSubsidio)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_subsidio");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CuotaMensual)
                .HasPrecision(12, 2)
                .HasComputedColumnSql("round(((uma_mensual_base * porcentaje_uma) / (100)::numeric), 2)", true)
                .HasColumnName("cuota_mensual");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.IngresoTopeMensual)
                .HasPrecision(12, 2)
                .HasColumnName("ingreso_tope_mensual");
            entity.Property(e => e.PorcentajeUma)
                .HasPrecision(6, 3)
                .HasColumnName("porcentaje_uma");
            entity.Property(e => e.UmaMensualBase)
                .HasPrecision(12, 2)
                .HasColumnName("uma_mensual_base");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.SubsidioEmpleoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_subsidio_empleo_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.SubsidioEmpleoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_subsidio_empleo_usuario_modificacion");
        });

        modelBuilder.Entity<Suplencias>(entity =>
        {
            entity.HasKey(e => e.IdSuplencia).HasName("tb_bi_suplencias_pkey");

            entity.ToTable("tb_bi_suplencias", "recursos_humanos");

            entity.HasIndex(e => new { e.IdHistorialHorario, e.FechaSuplencia }, "ux_suplencias_clase").IsUnique();

            entity.Property(e => e.IdSuplencia)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_suplencia");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.EstatusAprobacion)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pendiente'::character varying")
                .HasColumnName("estatus_aprobacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaSuplencia).HasColumnName("fecha_suplencia");
            entity.Property(e => e.IdAutorizador).HasColumnName("id_autorizador");
            entity.Property(e => e.IdEmpleadoSuplente).HasColumnName("id_empleado_suplente");
            entity.Property(e => e.IdEmpleadoTitular).HasColumnName("id_empleado_titular");
            entity.Property(e => e.IdHistorialHorario).HasColumnName("id_historial_horario");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdAutorizadorNavigation).WithMany(p => p.SuplenciasIdAutorizadorNavigation)
                .HasForeignKey(d => d.IdAutorizador)
                .HasConstraintName("tb_bi_suplencias_id_autorizador_fkey");

            entity.HasOne(d => d.IdEmpleadoSuplenteNavigation).WithMany(p => p.SuplenciasIdEmpleadoSuplenteNavigation)
                .HasForeignKey(d => d.IdEmpleadoSuplente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_suplencias_id_empleado_suplente_fkey");

            entity.HasOne(d => d.IdEmpleadoTitularNavigation).WithMany(p => p.SuplenciasIdEmpleadoTitularNavigation)
                .HasForeignKey(d => d.IdEmpleadoTitular)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_suplencias_id_empleado_titular_fkey");

            entity.HasOne(d => d.IdHistorialHorarioNavigation).WithMany(p => p.Suplencias)
                .HasForeignKey(d => d.IdHistorialHorario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_suplencias_id_historial_horario_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.SuplenciasIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_suplencias_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.SuplenciasIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_suplencias_usuario_modificacion");
        });

        modelBuilder.Entity<TarifasIsr>(entity =>
        {
            entity.HasKey(e => e.IdTarifaIsr).HasName("tb_bi_tarifas_isr_pkey");

            entity.ToTable("tb_bi_tarifas_isr", "nomina");

            entity.HasIndex(e => new { e.Anio, e.Periodicidad, e.LimiteInferior }, "ux_tarifas_isr").IsUnique();

            entity.Property(e => e.IdTarifaIsr)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tarifa_isr");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.CuotaFija)
                .HasPrecision(14, 2)
                .HasColumnName("cuota_fija");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.LimiteInferior)
                .HasPrecision(14, 2)
                .HasColumnName("limite_inferior");
            entity.Property(e => e.LimiteSuperior)
                .HasPrecision(14, 2)
                .HasColumnName("limite_superior");
            entity.Property(e => e.Periodicidad)
                .HasMaxLength(15)
                .HasColumnName("periodicidad");
            entity.Property(e => e.PorcentajeExcedente)
                .HasPrecision(6, 2)
                .HasColumnName("porcentaje_excedente");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TarifasIsrIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tarifas_isr_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TarifasIsrIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tarifas_isr_usuario_modificacion");
        });

        modelBuilder.Entity<TiposConcepto>(entity =>
        {
            entity.HasKey(e => e.IdTipoConcepto).HasName("tb_bi_tipos_concepto_pkey");

            entity.ToTable("tb_bi_tipos_concepto", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_concepto_clave_key").IsUnique();

            entity.Property(e => e.IdTipoConcepto)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_concepto");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CatalogoSat)
                .HasMaxLength(30)
                .HasColumnName("catalogo_sat");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposConceptoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_concepto_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposConceptoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_concepto_usuario_modificacion");
        });

        modelBuilder.Entity<TiposContrato>(entity =>
        {
            entity.HasKey(e => e.IdTipoContrato).HasName("tb_bi_tipos_contrato_pkey");

            entity.ToTable("tb_bi_tipos_contrato", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_contrato_clave_key").IsUnique();

            entity.Property(e => e.IdTipoContrato)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_contrato");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposContratoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_contrato_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposContratoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_contrato_usuario_modificacion");
        });

        modelBuilder.Entity<TiposDescuento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDescuento).HasName("tb_bi_tipos_descuento_pkey");

            entity.ToTable("tb_bi_tipos_descuento", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_descuento_clave_key").IsUnique();

            entity.Property(e => e.IdTipoDescuento)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_descuento");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AplicaInfonavit).HasColumnName("aplica_infonavit");
            entity.Property(e => e.AplicaPension).HasColumnName("aplica_pension");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposDescuentoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_descuento_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposDescuentoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_descuento_usuario_modificacion");
        });

        modelBuilder.Entity<TiposDuracionPermiso>(entity =>
        {
            entity.HasKey(e => e.IdTipoDuracionPermiso).HasName("tb_bi_tipos_duracion_permiso_pkey");

            entity.ToTable("tb_bi_tipos_duracion_permiso", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_duracion_permiso_clave_key").IsUnique();

            entity.Property(e => e.IdTipoDuracionPermiso)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_duracion_permiso");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposDuracionPermisoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tipos_duracion_permiso_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposDuracionPermisoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_tipos_duracion_permiso_usuario_modificacion");
        });

        modelBuilder.Entity<TiposExencion>(entity =>
        {
            entity.HasKey(e => e.IdTipoExencion).HasName("tb_bi_tipos_exencion_pkey");

            entity.ToTable("tb_bi_tipos_exencion", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_exencion_clave_key").IsUnique();

            entity.Property(e => e.IdTipoExencion)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_exencion");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AcumulaAnual).HasColumnName("acumula_anual");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.Fundamento)
                .HasMaxLength(100)
                .HasColumnName("fundamento");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.Multiplicador)
                .HasMaxLength(20)
                .HasDefaultValueSql("'ninguno'::character varying")
                .HasColumnName("multiplicador");
            entity.Property(e => e.PorcentajeExento)
                .HasPrecision(6, 2)
                .HasColumnName("porcentaje_exento");
            entity.Property(e => e.TopeVecesUma)
                .HasPrecision(8, 2)
                .HasColumnName("tope_veces_uma");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposExencionIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_exencion_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposExencionIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_exencion_usuario_modificacion");
        });

        modelBuilder.Entity<TiposIncapacidad>(entity =>
        {
            entity.HasKey(e => e.IdTipoIncapacidad).HasName("tb_bi_tipos_incapacidad_pkey");

            entity.ToTable("tb_bi_tipos_incapacidad", "recursos_humanos");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_incapacidad_clave_key").IsUnique();

            entity.Property(e => e.IdTipoIncapacidad)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_incapacidad");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposIncapacidadIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tipos_incapacidad_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposIncapacidadIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_tipos_incapacidad_usuario_modificacion");
        });

        modelBuilder.Entity<TiposIndicador>(entity =>
        {
            entity.HasKey(e => e.IdTipoIndicador).HasName("tb_bi_tipos_indicador_pkey");

            entity.ToTable("tb_bi_tipos_indicador", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_indicador_clave_key").IsUnique();

            entity.Property(e => e.IdTipoIndicador)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_indicador");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(30)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposIndicadorIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_indicador_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposIndicadorIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_indicador_usuario_modificacion");
        });

        modelBuilder.Entity<TiposJornada>(entity =>
        {
            entity.HasKey(e => e.IdTipoJornada).HasName("tb_bi_tipos_jornada_pkey");

            entity.ToTable("tb_bi_tipos_jornada", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_jornada_clave_key").IsUnique();

            entity.Property(e => e.IdTipoJornada)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_jornada");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposJornadaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_jornada_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposJornadaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_jornada_usuario_modificacion");
        });

        modelBuilder.Entity<TiposMovimientoImss>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimientoImss).HasName("tb_bi_tipos_movimiento_imss_pkey");

            entity.ToTable("tb_bi_tipos_movimiento_imss", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_movimiento_imss_clave_key").IsUnique();

            entity.Property(e => e.IdTipoMovimientoImss)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_movimiento_imss");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.ClaveIdse)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("clave_idse");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposMovimientoImssIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_movimiento_imss_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposMovimientoImssIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_movimiento_imss_usuario_modificacion");
        });

        modelBuilder.Entity<TiposNomina>(entity =>
        {
            entity.HasKey(e => e.IdTipoNomina).HasName("tb_bi_tipos_nomina_pkey");

            entity.ToTable("tb_bi_tipos_nomina", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_nomina_clave_key").IsUnique();

            entity.Property(e => e.IdTipoNomina)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_nomina");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.ClaveSat)
                .HasMaxLength(1)
                .HasColumnName("clave_sat");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposNominaIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_nomina_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposNominaIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_nomina_usuario_modificacion");
        });

        modelBuilder.Entity<TiposPermiso>(entity =>
        {
            entity.HasKey(e => e.IdTipoPermiso).HasName("tb_bi_tipos_permiso_pkey");

            entity.ToTable("tb_bi_tipos_permiso", "recursos_humanos");

            entity.HasIndex(e => e.Descripcion, "tb_bi_tipos_permiso_descripcion_key").IsUnique();

            entity.Property(e => e.IdTipoPermiso)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_permiso");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.DescuentaSaldo).HasColumnName("descuenta_saldo");
            entity.Property(e => e.DiasPorAnio).HasColumnName("dias_por_anio");
            entity.Property(e => e.EsConGoce)
                .HasDefaultValue(true)
                .HasColumnName("es_con_goce");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEstatusAsistencia)
                .HasDefaultValue(4)
                .HasColumnName("id_estatus_asistencia");
            entity.Property(e => e.IdTipoDuracionPermiso)
                .HasDefaultValue(1)
                .HasColumnName("id_tipo_duracion_permiso");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.RequiereComprobante).HasColumnName("requiere_comprobante");

            entity.HasOne(d => d.IdEstatusAsistenciaNavigation).WithMany(p => p.TiposPermiso)
                .HasForeignKey(d => d.IdEstatusAsistencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_tipos_permiso_id_estatus_asistencia_fkey");

            entity.HasOne(d => d.IdTipoDuracionPermisoNavigation).WithMany(p => p.TiposPermiso)
                .HasForeignKey(d => d.IdTipoDuracionPermiso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_tipos_permiso_id_tipo_duracion_permiso_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposPermisoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tipos_permiso_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposPermisoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_tipos_permiso_usuario_modificacion");
        });

        modelBuilder.Entity<TiposRegimen>(entity =>
        {
            entity.HasKey(e => e.IdTipoRegimen).HasName("tb_bi_tipos_regimen_pkey");

            entity.ToTable("tb_bi_tipos_regimen", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_tipos_regimen_clave_key").IsUnique();

            entity.Property(e => e.IdTipoRegimen)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_regimen");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TiposRegimenIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_tipos_regimen_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TiposRegimenIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_tipos_regimen_usuario_modificacion");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("tb_bi_usuarios_pkey");

            entity.ToTable("tb_bi_usuarios", "recursos_humanos");

            entity.HasIndex(e => e.IdEmpleado, "tb_bi_usuarios_id_empleado_key").IsUnique();

            entity.HasIndex(e => e.IdExterno, "tb_bi_usuarios_id_externo_key").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "tb_bi_usuarios_nombre_usuario_key").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.BloqueadoHasta).HasColumnName("bloqueado_hasta");
            entity.Property(e => e.DebeCambiarContrasena).HasColumnName("debe_cambiar_contrasena");
            entity.Property(e => e.EstatusUsuario)
                .HasMaxLength(20)
                .HasDefaultValueSql("'activo'::character varying")
                .HasColumnName("estatus_usuario");
            entity.Property(e => e.FechaCambioContrasena).HasColumnName("fecha_cambio_contrasena");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdExterno)
                .HasMaxLength(100)
                .HasColumnName("id_externo");
            entity.Property(e => e.IdProveedorAutenticacion)
                .HasDefaultValue(1)
                .HasColumnName("id_proveedor_autenticacion");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.IntentosFallidos).HasColumnName("intentos_fallidos");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(100)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.UltimoAcceso).HasColumnName("ultimo_acceso");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithOne(p => p.Usuarios)
                .HasForeignKey<Usuarios>(d => d.IdEmpleado)
                .HasConstraintName("tb_bi_usuarios_id_empleado_fkey");

            entity.HasOne(d => d.IdProveedorAutenticacionNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdProveedorAutenticacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_usuarios_id_proveedor_autenticacion_fkey");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_usuarios_id_rol_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.InverseIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usuarios_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.InverseIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_usuarios_usuario_modificacion");
        });

        modelBuilder.Entity<ZonasSalarioMinimo>(entity =>
        {
            entity.HasKey(e => e.IdZonaSalarioMinimo).HasName("tb_bi_zonas_salario_minimo_pkey");

            entity.ToTable("tb_bi_zonas_salario_minimo", "nomina");

            entity.HasIndex(e => e.Clave, "tb_bi_zonas_salario_minimo_clave_key").IsUnique();

            entity.Property(e => e.IdZonaSalarioMinimo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_zona_salario_minimo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdTipoIndicador).HasColumnName("id_tipo_indicador");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasDefaultValue(1)
                .HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");

            entity.HasOne(d => d.IdTipoIndicadorNavigation).WithMany(p => p.ZonasSalarioMinimo)
                .HasForeignKey(d => d.IdTipoIndicador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tb_bi_zonas_salario_minimo_id_tipo_indicador_fkey");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.ZonasSalarioMinimoIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nom_zonas_salario_minimo_usuario_creacion");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.ZonasSalarioMinimoIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("fk_nom_zonas_salario_minimo_usuario_modificacion");
        });

        modelBuilder.Entity<VwBiAcumuladosAnuales>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_acumulados_anuales", "nomina");

            entity.Property(e => e.ClaveConcepto)
                .HasMaxLength(10)
                .HasColumnName("clave_concepto");
            entity.Property(e => e.ClaveSat)
                .HasMaxLength(5)
                .HasColumnName("clave_sat");
            entity.Property(e => e.Ejercicio).HasColumnName("ejercicio");
            entity.Property(e => e.Exento).HasColumnName("exento");
            entity.Property(e => e.Gravado).HasColumnName("gravado");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.NombreConcepto)
                .HasMaxLength(100)
                .HasColumnName("nombre_concepto");
            entity.Property(e => e.TipoConcepto)
                .HasMaxLength(20)
                .HasColumnName("tipo_concepto");
            entity.Property(e => e.Total).HasColumnName("total");
        });

        modelBuilder.Entity<VwBiAsistenciaDiaria>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_asistencia_diaria", "recursos_humanos");

            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.ColorHex)
                .HasMaxLength(7)
                .IsFixedLength()
                .HasColumnName("color_hex");
            entity.Property(e => e.DescripcionEstatus)
                .HasMaxLength(100)
                .HasColumnName("descripcion_estatus");
            entity.Property(e => e.EstatusAsistencia)
                .HasMaxLength(30)
                .HasColumnName("estatus_asistencia");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaDia).HasColumnName("fecha_dia");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.HorasCubiertas)
                .HasPrecision(5, 2)
                .HasColumnName("horas_cubiertas");
            entity.Property(e => e.IdAsistenciaDiaria).HasColumnName("id_asistencia_diaria");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdEstatusAsistencia).HasColumnName("id_estatus_asistencia");
            entity.Property(e => e.IdHistorialHorario).HasColumnName("id_historial_horario");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.IdUsuarioCreacion).HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.MinutosRetardo).HasColumnName("minutos_retardo");
        });

        modelBuilder.Entity<VwBiConceptosNomina>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_conceptos_nomina", "nomina");

            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.AcumulaAnual).HasColumnName("acumula_anual");
            entity.Property(e => e.ClaveConcepto)
                .HasMaxLength(10)
                .HasColumnName("clave_concepto");
            entity.Property(e => e.ClaveSat)
                .HasMaxLength(5)
                .HasColumnName("clave_sat");
            entity.Property(e => e.EsCalculado).HasColumnName("es_calculado");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdClaveSatConcepto).HasColumnName("id_clave_sat_concepto");
            entity.Property(e => e.IdConcepto).HasColumnName("id_concepto");
            entity.Property(e => e.IdTipoConcepto).HasColumnName("id_tipo_concepto");
            entity.Property(e => e.IdTipoExencion).HasColumnName("id_tipo_exencion");
            entity.Property(e => e.IdUsuarioCreacion).HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.IntegraSdi).HasColumnName("integra_sdi");
            entity.Property(e => e.Multiplicador)
                .HasMaxLength(20)
                .HasColumnName("multiplicador");
            entity.Property(e => e.NombreConcepto)
                .HasMaxLength(100)
                .HasColumnName("nombre_concepto");
            entity.Property(e => e.PorcentajeExento)
                .HasPrecision(6, 2)
                .HasColumnName("porcentaje_exento");
            entity.Property(e => e.TipoConcepto)
                .HasMaxLength(20)
                .HasColumnName("tipo_concepto");
            entity.Property(e => e.TipoExencion)
                .HasMaxLength(20)
                .HasColumnName("tipo_exencion");
            entity.Property(e => e.TopeVecesUma)
                .HasPrecision(8, 2)
                .HasColumnName("tope_veces_uma");
        });

        modelBuilder.Entity<VwBiCostoPatronal>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_costo_patronal", "nomina");

            entity.Property(e => e.CostoTotal).HasColumnName("costo_total");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdPeriodoNomina).HasColumnName("id_periodo_nomina");
            entity.Property(e => e.ImssPatronal).HasColumnName("imss_patronal");
            entity.Property(e => e.InfonavitPatronal).HasColumnName("infonavit_patronal");
            entity.Property(e => e.TotalPercepciones)
                .HasPrecision(14, 2)
                .HasColumnName("total_percepciones");
        });

        modelBuilder.Entity<VwBiDirectorioEmpleados>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_directorio_empleados", "recursos_humanos");

            entity.Property(e => e.Company)
                .HasMaxLength(100)
                .HasColumnName("company");
            entity.Property(e => e.CuentaExistenteM365).HasColumnName("cuenta_existente_m365");
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .HasColumnName("department");
            entity.Property(e => e.DisplayName).HasColumnName("displayName");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(30)
                .HasColumnName("employeeID");
            entity.Property(e => e.GivenName)
                .HasMaxLength(100)
                .HasColumnName("givenName");
            entity.Property(e => e.Habilitada).HasColumnName("habilitada");
            entity.Property(e => e.HashDatos)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("hash_datos");
            entity.Property(e => e.HashSincronizado)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("hash_sincronizado");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.L)
                .HasMaxLength(100)
                .HasColumnName("l");
            entity.Property(e => e.Mail)
                .HasMaxLength(150)
                .HasColumnName("mail");
            entity.Property(e => e.Manager)
                .HasMaxLength(20)
                .HasColumnName("manager");
            entity.Property(e => e.Mobile)
                .HasMaxLength(20)
                .HasColumnName("mobile");
            entity.Property(e => e.ObjectGuid).HasColumnName("object_guid");
            entity.Property(e => e.OuDestino)
                .HasMaxLength(300)
                .HasColumnName("ou_destino");
            entity.Property(e => e.PhysicalDeliveryOfficeName)
                .HasMaxLength(100)
                .HasColumnName("physicalDeliveryOfficeName");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("postalCode");
            entity.Property(e => e.RequiereSincronizacion).HasColumnName("requiere_sincronizacion");
            entity.Property(e => e.SAmaccountName)
                .HasMaxLength(20)
                .HasColumnName("sAMAccountName");
            entity.Property(e => e.Sn).HasColumnName("sn");
            entity.Property(e => e.St)
                .HasMaxLength(100)
                .HasColumnName("st");
            entity.Property(e => e.StreetAddress)
                .HasMaxLength(200)
                .HasColumnName("streetAddress");
            entity.Property(e => e.TelephoneNumber).HasColumnName("telephoneNumber");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.UserPrincipalName)
                .HasMaxLength(150)
                .HasColumnName("userPrincipalName");
        });

        modelBuilder.Entity<VwBiDirectorioPendientes>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_directorio_pendientes", "recursos_humanos");

            entity.Property(e => e.Empleado).HasColumnName("empleado");
            entity.Property(e => e.Empresa)
                .HasMaxLength(100)
                .HasColumnName("empresa");
            entity.Property(e => e.Faltantes).HasColumnName("faltantes");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
        });

        modelBuilder.Entity<VwBiMovimientosImss>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_movimientos_imss", "nomina");

            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.AcuseIdse)
                .HasMaxLength(100)
                .HasColumnName("acuse_idse");
            entity.Property(e => e.ClaveIdse)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("clave_idse");
            entity.Property(e => e.EnviadoIdse).HasColumnName("enviado_idse");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaEnvioIdse).HasColumnName("fecha_envio_idse");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaMovimiento).HasColumnName("fecha_movimiento");
            entity.Property(e => e.IdCausaBajaImss).HasColumnName("id_causa_baja_imss");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdMovimientoImss).HasColumnName("id_movimiento_imss");
            entity.Property(e => e.IdTipoMovimientoImss).HasColumnName("id_tipo_movimiento_imss");
            entity.Property(e => e.IdUsuarioCreacion).HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.SalarioBaseCotizacion)
                .HasPrecision(12, 2)
                .HasColumnName("salario_base_cotizacion");
            entity.Property(e => e.SalarioDiario)
                .HasPrecision(12, 2)
                .HasColumnName("salario_diario");
            entity.Property(e => e.SalarioDiarioIntegrado)
                .HasPrecision(12, 2)
                .HasColumnName("salario_diario_integrado");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(20)
                .HasColumnName("tipo_movimiento");
        });

        modelBuilder.Entity<VwBiPeriodosNomina>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_periodos_nomina", "nomina");

            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.DiasPago)
                .HasPrecision(5, 2)
                .HasColumnName("dias_pago");
            entity.Property(e => e.Ejercicio).HasColumnName("ejercicio");
            entity.Property(e => e.EstatusCierre)
                .HasMaxLength(15)
                .HasColumnName("estatus_cierre");
            entity.Property(e => e.FechaAutorizacion).HasColumnName("fecha_autorizacion");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");
            entity.Property(e => e.IdAutorizadorNomina).HasColumnName("id_autorizador_nomina");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdPeriodicidadPago).HasColumnName("id_periodicidad_pago");
            entity.Property(e => e.IdPeriodoNomina).HasColumnName("id_periodo_nomina");
            entity.Property(e => e.IdTipoNomina).HasColumnName("id_tipo_nomina");
            entity.Property(e => e.IdUsuarioCreacion).HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.NumeroPeriodo).HasColumnName("numero_periodo");
            entity.Property(e => e.Periodicidad)
                .HasMaxLength(15)
                .HasColumnName("periodicidad");
            entity.Property(e => e.PeriodicidadSat)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("periodicidad_sat");
            entity.Property(e => e.TipoNomina)
                .HasMaxLength(20)
                .HasColumnName("tipo_nomina");
            entity.Property(e => e.TipoNominaSat)
                .HasMaxLength(1)
                .HasColumnName("tipo_nomina_sat");
        });

        modelBuilder.Entity<VwBiRecibos>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_recibos", "nomina");

            entity.Property(e => e.Curp)
                .HasMaxLength(18)
                .IsFixedLength()
                .HasColumnName("curp");
            entity.Property(e => e.Departamento)
                .HasMaxLength(100)
                .HasColumnName("departamento");
            entity.Property(e => e.DiasPagados)
                .HasPrecision(6, 2)
                .HasColumnName("dias_pagados");
            entity.Property(e => e.Empleado).HasColumnName("empleado");
            entity.Property(e => e.Empresa)
                .HasMaxLength(100)
                .HasColumnName("empresa");
            entity.Property(e => e.EstatusRecibo)
                .HasMaxLength(15)
                .HasColumnName("estatus_recibo");
            entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdNomina).HasColumnName("id_nomina");
            entity.Property(e => e.IdPeriodoNomina).HasColumnName("id_periodo_nomina");
            entity.Property(e => e.ImssObrero)
                .HasPrecision(12, 2)
                .HasColumnName("imss_obrero");
            entity.Property(e => e.ImssPatronal)
                .HasPrecision(12, 2)
                .HasColumnName("imss_patronal");
            entity.Property(e => e.IsrCausado)
                .HasPrecision(12, 2)
                .HasColumnName("isr_causado");
            entity.Property(e => e.IsrRetenido)
                .HasPrecision(12, 2)
                .HasColumnName("isr_retenido");
            entity.Property(e => e.Matricula)
                .HasMaxLength(30)
                .HasColumnName("matricula");
            entity.Property(e => e.NetoAPagar)
                .HasPrecision(14, 2)
                .HasColumnName("neto_a_pagar");
            entity.Property(e => e.Nss)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasColumnName("nss");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .HasColumnName("observaciones");
            entity.Property(e => e.Periodo)
                .HasMaxLength(100)
                .HasColumnName("periodo");
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .HasColumnName("rfc");
            entity.Property(e => e.SalarioDiario)
                .HasPrecision(12, 2)
                .HasColumnName("salario_diario");
            entity.Property(e => e.SalarioDiarioIntegrado)
                .HasPrecision(12, 2)
                .HasColumnName("salario_diario_integrado");
            entity.Property(e => e.SubsidioCausado)
                .HasPrecision(12, 2)
                .HasColumnName("subsidio_causado");
            entity.Property(e => e.TipoNomina)
                .HasMaxLength(20)
                .HasColumnName("tipo_nomina");
            entity.Property(e => e.TotalDeducciones)
                .HasPrecision(14, 2)
                .HasColumnName("total_deducciones");
            entity.Property(e => e.TotalExento)
                .HasPrecision(14, 2)
                .HasColumnName("total_exento");
            entity.Property(e => e.TotalGravado)
                .HasPrecision(14, 2)
                .HasColumnName("total_gravado");
            entity.Property(e => e.TotalOtrosPagos)
                .HasPrecision(14, 2)
                .HasColumnName("total_otros_pagos");
            entity.Property(e => e.TotalPercepciones)
                .HasPrecision(14, 2)
                .HasColumnName("total_percepciones");
            entity.Property(e => e.Uuid).HasColumnName("uuid");
        });

        modelBuilder.Entity<VwBiRegistrosPatronales>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_registros_patronales", "nomina");

            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.AreaSalarioMinimo)
                .HasMaxLength(20)
                .HasColumnName("area_salario_minimo");
            entity.Property(e => e.ClaseRiesgo)
                .HasMaxLength(2)
                .HasColumnName("clase_riesgo");
            entity.Property(e => e.EntidadFederativa)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("entidad_federativa");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdClaseRiesgo).HasColumnName("id_clase_riesgo");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdRegistroPatronal).HasColumnName("id_registro_patronal");
            entity.Property(e => e.IdUsuarioCreacion).HasColumnName("id_usuario_creacion");
            entity.Property(e => e.IdUsuarioModificacion).HasColumnName("id_usuario_modificacion");
            entity.Property(e => e.IdZonaSalarioMinimo).HasColumnName("id_zona_salario_minimo");
            entity.Property(e => e.RegistroPatronal)
                .HasMaxLength(11)
                .HasColumnName("registro_patronal");
        });

        modelBuilder.Entity<VwBiResumenAsistenciaPeriodo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_resumen_asistencia_periodo", "nomina");

            entity.Property(e => e.ATiempo).HasColumnName("a_tiempo");
            entity.Property(e => e.FaltasInjustificadas).HasColumnName("faltas_injustificadas");
            entity.Property(e => e.FaltasJustificadas).HasColumnName("faltas_justificadas");
            entity.Property(e => e.HorasCubiertas).HasColumnName("horas_cubiertas");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdPeriodoNomina).HasColumnName("id_periodo_nomina");
            entity.Property(e => e.Incapacidad).HasColumnName("incapacidad");
            entity.Property(e => e.JornadasProgramadas).HasColumnName("jornadas_programadas");
            entity.Property(e => e.PermisosSinGoce).HasColumnName("permisos_sin_goce");
            entity.Property(e => e.Retardos).HasColumnName("retardos");
            entity.Property(e => e.Suplidas).HasColumnName("suplidas");
            entity.Property(e => e.Vacaciones).HasColumnName("vacaciones");
        });

        modelBuilder.Entity<VwBiVacacionesDisponibles>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_bi_vacaciones_disponibles", "recursos_humanos");

            entity.Property(e => e.DiasDisponibles).HasColumnName("dias_disponibles");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.ProximaCaducidad).HasColumnName("proxima_caducidad");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
