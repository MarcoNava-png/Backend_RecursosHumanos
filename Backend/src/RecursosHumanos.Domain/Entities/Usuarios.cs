using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Usuarios
{
    public int IdUsuario { get; set; }

    public int? IdEmpleado { get; set; }

    public int IdRol { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string EstatusUsuario { get; set; } = null!;

    public int IdProveedorAutenticacion { get; set; }

    public string? IdExterno { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public int IntentosFallidos { get; set; }

    public DateTime? BloqueadoHasta { get; set; }

    public bool DebeCambiarContrasena { get; set; }

    public DateTime? FechaCambioContrasena { get; set; }

    public virtual Empleados? IdEmpleadoNavigation { get; set; }

    public virtual ProveedoresAutenticacion IdProveedorAutenticacionNavigation { get; set; } = null!;

    public virtual Roles IdRolNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Usuarios> InverseIdUsuarioCreacionNavigation { get; set; } = new List<Usuarios>();

    public virtual ICollection<Usuarios> InverseIdUsuarioModificacionNavigation { get; set; } = new List<Usuarios>();

    public virtual ICollection<AccionesDirectorio> AccionesDirectorioIdUsuarioCreacionNavigation { get; set; } = new List<AccionesDirectorio>();

    public virtual ICollection<AccionesDirectorio> AccionesDirectorioIdUsuarioModificacionNavigation { get; set; } = new List<AccionesDirectorio>();

    public virtual ICollection<AsistenciaDiaria> AsistenciaDiariaIdUsuarioCreacionNavigation { get; set; } = new List<AsistenciaDiaria>();

    public virtual ICollection<AsistenciaDiaria> AsistenciaDiariaIdUsuarioModificacionNavigation { get; set; } = new List<AsistenciaDiaria>();

    public virtual ICollection<Asistencias> AsistenciasIdUsuarioCreacionNavigation { get; set; } = new List<Asistencias>();

    public virtual ICollection<Asistencias> AsistenciasIdUsuarioModificacionNavigation { get; set; } = new List<Asistencias>();

    public virtual ICollection<AuditoriaLogs> AuditoriaLogs { get; set; } = new List<AuditoriaLogs>();

    public virtual ICollection<BajasLaborales> BajasLaboralesIdUsuarioCreacionNavigation { get; set; } = new List<BajasLaborales>();

    public virtual ICollection<BajasLaborales> BajasLaboralesIdUsuarioModificacionNavigation { get; set; } = new List<BajasLaborales>();

    public virtual ICollection<Bancos> BancosIdUsuarioCreacionNavigation { get; set; } = new List<Bancos>();

    public virtual ICollection<Bancos> BancosIdUsuarioModificacionNavigation { get; set; } = new List<Bancos>();

    public virtual ICollection<Campus> CampusIdUsuarioCreacionNavigation { get; set; } = new List<Campus>();

    public virtual ICollection<Campus> CampusIdUsuarioModificacionNavigation { get; set; } = new List<Campus>();

    public virtual ICollection<CausasBajaImss> CausasBajaImssIdUsuarioCreacionNavigation { get; set; } = new List<CausasBajaImss>();

    public virtual ICollection<CausasBajaImss> CausasBajaImssIdUsuarioModificacionNavigation { get; set; } = new List<CausasBajaImss>();

    public virtual ICollection<CesantiaVejezPatron> CesantiaVejezPatronIdUsuarioCreacionNavigation { get; set; } = new List<CesantiaVejezPatron>();

    public virtual ICollection<CesantiaVejezPatron> CesantiaVejezPatronIdUsuarioModificacionNavigation { get; set; } = new List<CesantiaVejezPatron>();

    public virtual ICollection<Checadores> ChecadoresIdUsuarioCreacionNavigation { get; set; } = new List<Checadores>();

    public virtual ICollection<Checadores> ChecadoresIdUsuarioModificacionNavigation { get; set; } = new List<Checadores>();

    public virtual ICollection<ClasesRiesgo> ClasesRiesgoIdUsuarioCreacionNavigation { get; set; } = new List<ClasesRiesgo>();

    public virtual ICollection<ClasesRiesgo> ClasesRiesgoIdUsuarioModificacionNavigation { get; set; } = new List<ClasesRiesgo>();

    public virtual ICollection<ClavesSatConcepto> ClavesSatConceptoIdUsuarioCreacionNavigation { get; set; } = new List<ClavesSatConcepto>();

    public virtual ICollection<ClavesSatConcepto> ClavesSatConceptoIdUsuarioModificacionNavigation { get; set; } = new List<ClavesSatConcepto>();

    public virtual ICollection<ConceptosHist> ConceptosHistIdUsuarioCreacionNavigation { get; set; } = new List<ConceptosHist>();

    public virtual ICollection<ConceptosHist> ConceptosHistIdUsuarioModificacionNavigation { get; set; } = new List<ConceptosHist>();

    public virtual ICollection<ConceptosNomina> ConceptosNominaIdUsuarioCreacionNavigation { get; set; } = new List<ConceptosNomina>();

    public virtual ICollection<ConceptosNomina> ConceptosNominaIdUsuarioModificacionNavigation { get; set; } = new List<ConceptosNomina>();

    public virtual ICollection<ConceptosRecurrentes> ConceptosRecurrentesIdUsuarioCreacionNavigation { get; set; } = new List<ConceptosRecurrentes>();

    public virtual ICollection<ConceptosRecurrentes> ConceptosRecurrentesIdUsuarioModificacionNavigation { get; set; } = new List<ConceptosRecurrentes>();

    public virtual ICollection<CreditosFonacot> CreditosFonacotIdUsuarioCreacionNavigation { get; set; } = new List<CreditosFonacot>();

    public virtual ICollection<CreditosFonacot> CreditosFonacotIdUsuarioModificacionNavigation { get; set; } = new List<CreditosFonacot>();

    public virtual ICollection<CreditosInfonavit> CreditosInfonavitIdUsuarioCreacionNavigation { get; set; } = new List<CreditosInfonavit>();

    public virtual ICollection<CreditosInfonavit> CreditosInfonavitIdUsuarioModificacionNavigation { get; set; } = new List<CreditosInfonavit>();

    public virtual ICollection<CuentasDirectorio> CuentasDirectorioIdUsuarioCreacionNavigation { get; set; } = new List<CuentasDirectorio>();

    public virtual ICollection<CuentasDirectorio> CuentasDirectorioIdUsuarioModificacionNavigation { get; set; } = new List<CuentasDirectorio>();

    public virtual ICollection<CuotasImss> CuotasImssIdUsuarioCreacionNavigation { get; set; } = new List<CuotasImss>();

    public virtual ICollection<CuotasImss> CuotasImssIdUsuarioModificacionNavigation { get; set; } = new List<CuotasImss>();

    public virtual ICollection<Departamentos> DepartamentosIdUsuarioCreacionNavigation { get; set; } = new List<Departamentos>();

    public virtual ICollection<Departamentos> DepartamentosIdUsuarioModificacionNavigation { get; set; } = new List<Departamentos>();

    public virtual ICollection<DiasFestivos> DiasFestivosIdUsuarioCreacionNavigation { get; set; } = new List<DiasFestivos>();

    public virtual ICollection<DiasFestivos> DiasFestivosIdUsuarioModificacionNavigation { get; set; } = new List<DiasFestivos>();

    public virtual ICollection<Empleados> EmpleadosIdUsuarioCreacionNavigation { get; set; } = new List<Empleados>();

    public virtual ICollection<Empleados> EmpleadosIdUsuarioModificacionNavigation { get; set; } = new List<Empleados>();

    public virtual ICollection<EmpleadosNomina> EmpleadosNominaIdUsuarioCreacionNavigation { get; set; } = new List<EmpleadosNomina>();

    public virtual ICollection<EmpleadosNomina> EmpleadosNominaIdUsuarioModificacionNavigation { get; set; } = new List<EmpleadosNomina>();

    public virtual ICollection<Empresas> EmpresasIdUsuarioCreacionNavigation { get; set; } = new List<Empresas>();

    public virtual ICollection<Empresas> EmpresasIdUsuarioModificacionNavigation { get; set; } = new List<Empresas>();

    public virtual ICollection<EsquemasPrestaciones> EsquemasPrestacionesIdUsuarioCreacionNavigation { get; set; } = new List<EsquemasPrestaciones>();

    public virtual ICollection<EsquemasPrestaciones> EsquemasPrestacionesIdUsuarioModificacionNavigation { get; set; } = new List<EsquemasPrestaciones>();

    public virtual ICollection<Estados> EstadosIdUsuarioCreacionNavigation { get; set; } = new List<Estados>();

    public virtual ICollection<Estados> EstadosIdUsuarioModificacionNavigation { get; set; } = new List<Estados>();

    public virtual ICollection<EstatusAsistencia> EstatusAsistenciaIdUsuarioCreacionNavigation { get; set; } = new List<EstatusAsistencia>();

    public virtual ICollection<EstatusAsistencia> EstatusAsistenciaIdUsuarioModificacionNavigation { get; set; } = new List<EstatusAsistencia>();

    public virtual ICollection<EstatusLaborales> EstatusLaboralesIdUsuarioCreacionNavigation { get; set; } = new List<EstatusLaborales>();

    public virtual ICollection<EstatusLaborales> EstatusLaboralesIdUsuarioModificacionNavigation { get; set; } = new List<EstatusLaborales>();

    public virtual ICollection<HistorialHorarios> HistorialHorariosIdUsuarioCreacionNavigation { get; set; } = new List<HistorialHorarios>();

    public virtual ICollection<HistorialHorarios> HistorialHorariosIdUsuarioModificacionNavigation { get; set; } = new List<HistorialHorarios>();

    public virtual ICollection<HistorialSueldos> HistorialSueldosIdUsuarioCreacionNavigation { get; set; } = new List<HistorialSueldos>();

    public virtual ICollection<HistorialSueldos> HistorialSueldosIdUsuarioModificacionNavigation { get; set; } = new List<HistorialSueldos>();

    public virtual ICollection<IncapacidadesImss> IncapacidadesImssIdUsuarioCreacionNavigation { get; set; } = new List<IncapacidadesImss>();

    public virtual ICollection<IncapacidadesImss> IncapacidadesImssIdUsuarioModificacionNavigation { get; set; } = new List<IncapacidadesImss>();

    public virtual ICollection<IncidenciasNomina> IncidenciasNominaIdUsuarioCreacionNavigation { get; set; } = new List<IncidenciasNomina>();

    public virtual ICollection<IncidenciasNomina> IncidenciasNominaIdUsuarioModificacionNavigation { get; set; } = new List<IncidenciasNomina>();

    public virtual ICollection<Indicadores> IndicadoresIdUsuarioCreacionNavigation { get; set; } = new List<Indicadores>();

    public virtual ICollection<Indicadores> IndicadoresIdUsuarioModificacionNavigation { get; set; } = new List<Indicadores>();

    public virtual ICollection<LftVacaciones> LftVacacionesIdUsuarioCreacionNavigation { get; set; } = new List<LftVacaciones>();

    public virtual ICollection<LftVacaciones> LftVacacionesIdUsuarioModificacionNavigation { get; set; } = new List<LftVacaciones>();

    public virtual ICollection<Materias> MateriasIdUsuarioCreacionNavigation { get; set; } = new List<Materias>();

    public virtual ICollection<Materias> MateriasIdUsuarioModificacionNavigation { get; set; } = new List<Materias>();

    public virtual ICollection<MetodosPago> MetodosPagoIdUsuarioCreacionNavigation { get; set; } = new List<MetodosPago>();

    public virtual ICollection<MetodosPago> MetodosPagoIdUsuarioModificacionNavigation { get; set; } = new List<MetodosPago>();

    public virtual ICollection<ModosChecada> ModosChecadaIdUsuarioCreacionNavigation { get; set; } = new List<ModosChecada>();

    public virtual ICollection<ModosChecada> ModosChecadaIdUsuarioModificacionNavigation { get; set; } = new List<ModosChecada>();

    public virtual ICollection<MotivosBaja> MotivosBajaIdUsuarioCreacionNavigation { get; set; } = new List<MotivosBaja>();

    public virtual ICollection<MotivosBaja> MotivosBajaIdUsuarioModificacionNavigation { get; set; } = new List<MotivosBaja>();

    public virtual ICollection<MovimientosImss> MovimientosImssIdUsuarioCreacionNavigation { get; set; } = new List<MovimientosImss>();

    public virtual ICollection<MovimientosImss> MovimientosImssIdUsuarioModificacionNavigation { get; set; } = new List<MovimientosImss>();

    public virtual ICollection<NominaCuotasImss> NominaCuotasImssIdUsuarioCreacionNavigation { get; set; } = new List<NominaCuotasImss>();

    public virtual ICollection<NominaCuotasImss> NominaCuotasImssIdUsuarioModificacionNavigation { get; set; } = new List<NominaCuotasImss>();

    public virtual ICollection<NominaDetalle> NominaDetalleIdUsuarioCreacionNavigation { get; set; } = new List<NominaDetalle>();

    public virtual ICollection<NominaDetalle> NominaDetalleIdUsuarioModificacionNavigation { get; set; } = new List<NominaDetalle>();

    public virtual ICollection<Nominas> NominasIdUsuarioCreacionNavigation { get; set; } = new List<Nominas>();

    public virtual ICollection<Nominas> NominasIdUsuarioModificacionNavigation { get; set; } = new List<Nominas>();

    public virtual ICollection<OrigenesChecada> OrigenesChecadaIdUsuarioCreacionNavigation { get; set; } = new List<OrigenesChecada>();

    public virtual ICollection<OrigenesChecada> OrigenesChecadaIdUsuarioModificacionNavigation { get; set; } = new List<OrigenesChecada>();

    public virtual ICollection<PensionesAlimenticias> PensionesAlimenticiasIdUsuarioCreacionNavigation { get; set; } = new List<PensionesAlimenticias>();

    public virtual ICollection<PensionesAlimenticias> PensionesAlimenticiasIdUsuarioModificacionNavigation { get; set; } = new List<PensionesAlimenticias>();

    public virtual ICollection<Perfiles> PerfilesIdUsuarioCreacionNavigation { get; set; } = new List<Perfiles>();

    public virtual ICollection<Perfiles> PerfilesIdUsuarioModificacionNavigation { get; set; } = new List<Perfiles>();

    public virtual ICollection<PeriodicidadesPago> PeriodicidadesPagoIdUsuarioCreacionNavigation { get; set; } = new List<PeriodicidadesPago>();

    public virtual ICollection<PeriodicidadesPago> PeriodicidadesPagoIdUsuarioModificacionNavigation { get; set; } = new List<PeriodicidadesPago>();

    public virtual ICollection<PeriodosAcumulacion> PeriodosAcumulacionIdUsuarioCreacionNavigation { get; set; } = new List<PeriodosAcumulacion>();

    public virtual ICollection<PeriodosAcumulacion> PeriodosAcumulacionIdUsuarioModificacionNavigation { get; set; } = new List<PeriodosAcumulacion>();

    public virtual ICollection<PeriodosEscolares> PeriodosEscolaresIdUsuarioCreacionNavigation { get; set; } = new List<PeriodosEscolares>();

    public virtual ICollection<PeriodosEscolares> PeriodosEscolaresIdUsuarioModificacionNavigation { get; set; } = new List<PeriodosEscolares>();

    public virtual ICollection<PeriodosNomina> PeriodosNominaIdUsuarioCreacionNavigation { get; set; } = new List<PeriodosNomina>();

    public virtual ICollection<PeriodosNomina> PeriodosNominaIdUsuarioModificacionNavigation { get; set; } = new List<PeriodosNomina>();

    public virtual ICollection<Permisos> PermisosIdUsuarioCreacionNavigation { get; set; } = new List<Permisos>();

    public virtual ICollection<Permisos> PermisosIdUsuarioModificacionNavigation { get; set; } = new List<Permisos>();

    public virtual ICollection<Personas> PersonasIdUsuarioCreacionNavigation { get; set; } = new List<Personas>();

    public virtual ICollection<Personas> PersonasIdUsuarioModificacionNavigation { get; set; } = new List<Personas>();

    public virtual ICollection<PrimasRiesgoTrabajo> PrimasRiesgoTrabajoIdUsuarioCreacionNavigation { get; set; } = new List<PrimasRiesgoTrabajo>();

    public virtual ICollection<PrimasRiesgoTrabajo> PrimasRiesgoTrabajoIdUsuarioModificacionNavigation { get; set; } = new List<PrimasRiesgoTrabajo>();

    public virtual ICollection<ProveedoresAutenticacion> ProveedoresAutenticacionIdUsuarioCreacionNavigation { get; set; } = new List<ProveedoresAutenticacion>();

    public virtual ICollection<ProveedoresAutenticacion> ProveedoresAutenticacionIdUsuarioModificacionNavigation { get; set; } = new List<ProveedoresAutenticacion>();

    public virtual ICollection<PtuEmpleados> PtuEmpleadosIdUsuarioCreacionNavigation { get; set; } = new List<PtuEmpleados>();

    public virtual ICollection<PtuEmpleados> PtuEmpleadosIdUsuarioModificacionNavigation { get; set; } = new List<PtuEmpleados>();

    public virtual ICollection<Ptu> PtuIdUsuarioCreacionNavigation { get; set; } = new List<Ptu>();

    public virtual ICollection<Ptu> PtuIdUsuarioModificacionNavigation { get; set; } = new List<Ptu>();

    public virtual ICollection<Puestos> PuestosIdUsuarioCreacionNavigation { get; set; } = new List<Puestos>();

    public virtual ICollection<Puestos> PuestosIdUsuarioModificacionNavigation { get; set; } = new List<Puestos>();

    public virtual ICollection<RegimenesFiscales> RegimenesFiscalesIdUsuarioCreacionNavigation { get; set; } = new List<RegimenesFiscales>();

    public virtual ICollection<RegimenesFiscales> RegimenesFiscalesIdUsuarioModificacionNavigation { get; set; } = new List<RegimenesFiscales>();

    public virtual ICollection<RegistrosPatronales> RegistrosPatronalesIdUsuarioCreacionNavigation { get; set; } = new List<RegistrosPatronales>();

    public virtual ICollection<RegistrosPatronales> RegistrosPatronalesIdUsuarioModificacionNavigation { get; set; } = new List<RegistrosPatronales>();

    public virtual ICollection<ReglasAsistencia> ReglasAsistenciaIdUsuarioCreacionNavigation { get; set; } = new List<ReglasAsistencia>();

    public virtual ICollection<ReglasAsistencia> ReglasAsistenciaIdUsuarioModificacionNavigation { get; set; } = new List<ReglasAsistencia>();

    public virtual ICollection<ReglasPrimaAntiguedad> ReglasPrimaAntiguedadIdUsuarioCreacionNavigation { get; set; } = new List<ReglasPrimaAntiguedad>();

    public virtual ICollection<ReglasPrimaAntiguedad> ReglasPrimaAntiguedadIdUsuarioModificacionNavigation { get; set; } = new List<ReglasPrimaAntiguedad>();

    public virtual ICollection<RolesGruposDirectorio> RolesGruposDirectorioIdUsuarioCreacionNavigation { get; set; } = new List<RolesGruposDirectorio>();

    public virtual ICollection<RolesGruposDirectorio> RolesGruposDirectorioIdUsuarioModificacionNavigation { get; set; } = new List<RolesGruposDirectorio>();

    public virtual ICollection<Roles> RolesIdUsuarioCreacionNavigation { get; set; } = new List<Roles>();

    public virtual ICollection<Roles> RolesIdUsuarioModificacionNavigation { get; set; } = new List<Roles>();

    public virtual ICollection<RolesPermisos> RolesPermisosIdUsuarioCreacionNavigation { get; set; } = new List<RolesPermisos>();

    public virtual ICollection<RolesPermisos> RolesPermisosIdUsuarioModificacionNavigation { get; set; } = new List<RolesPermisos>();

    public virtual ICollection<SaldosVacaciones> SaldosVacacionesIdUsuarioCreacionNavigation { get; set; } = new List<SaldosVacaciones>();

    public virtual ICollection<SaldosVacaciones> SaldosVacacionesIdUsuarioModificacionNavigation { get; set; } = new List<SaldosVacaciones>();

    public virtual ICollection<SincronizacionDirectorioLogs> SincronizacionDirectorioLogsIdUsuarioCreacionNavigation { get; set; } = new List<SincronizacionDirectorioLogs>();

    public virtual ICollection<SincronizacionDirectorioLogs> SincronizacionDirectorioLogsIdUsuarioModificacionNavigation { get; set; } = new List<SincronizacionDirectorioLogs>();

    public virtual ICollection<Solicitudes> SolicitudesIdUsuarioCreacionNavigation { get; set; } = new List<Solicitudes>();

    public virtual ICollection<Solicitudes> SolicitudesIdUsuarioModificacionNavigation { get; set; } = new List<Solicitudes>();

    public virtual ICollection<SubsidioEmpleo> SubsidioEmpleoIdUsuarioCreacionNavigation { get; set; } = new List<SubsidioEmpleo>();

    public virtual ICollection<SubsidioEmpleo> SubsidioEmpleoIdUsuarioModificacionNavigation { get; set; } = new List<SubsidioEmpleo>();

    public virtual ICollection<Suplencias> SuplenciasIdUsuarioCreacionNavigation { get; set; } = new List<Suplencias>();

    public virtual ICollection<Suplencias> SuplenciasIdUsuarioModificacionNavigation { get; set; } = new List<Suplencias>();

    public virtual ICollection<TarifasIsr> TarifasIsrIdUsuarioCreacionNavigation { get; set; } = new List<TarifasIsr>();

    public virtual ICollection<TarifasIsr> TarifasIsrIdUsuarioModificacionNavigation { get; set; } = new List<TarifasIsr>();

    public virtual ICollection<TiposConcepto> TiposConceptoIdUsuarioCreacionNavigation { get; set; } = new List<TiposConcepto>();

    public virtual ICollection<TiposConcepto> TiposConceptoIdUsuarioModificacionNavigation { get; set; } = new List<TiposConcepto>();

    public virtual ICollection<TiposContrato> TiposContratoIdUsuarioCreacionNavigation { get; set; } = new List<TiposContrato>();

    public virtual ICollection<TiposContrato> TiposContratoIdUsuarioModificacionNavigation { get; set; } = new List<TiposContrato>();

    public virtual ICollection<TiposDescuento> TiposDescuentoIdUsuarioCreacionNavigation { get; set; } = new List<TiposDescuento>();

    public virtual ICollection<TiposDescuento> TiposDescuentoIdUsuarioModificacionNavigation { get; set; } = new List<TiposDescuento>();

    public virtual ICollection<TiposDuracionPermiso> TiposDuracionPermisoIdUsuarioCreacionNavigation { get; set; } = new List<TiposDuracionPermiso>();

    public virtual ICollection<TiposDuracionPermiso> TiposDuracionPermisoIdUsuarioModificacionNavigation { get; set; } = new List<TiposDuracionPermiso>();

    public virtual ICollection<TiposExencion> TiposExencionIdUsuarioCreacionNavigation { get; set; } = new List<TiposExencion>();

    public virtual ICollection<TiposExencion> TiposExencionIdUsuarioModificacionNavigation { get; set; } = new List<TiposExencion>();

    public virtual ICollection<TiposIncapacidad> TiposIncapacidadIdUsuarioCreacionNavigation { get; set; } = new List<TiposIncapacidad>();

    public virtual ICollection<TiposIncapacidad> TiposIncapacidadIdUsuarioModificacionNavigation { get; set; } = new List<TiposIncapacidad>();

    public virtual ICollection<TiposIndicador> TiposIndicadorIdUsuarioCreacionNavigation { get; set; } = new List<TiposIndicador>();

    public virtual ICollection<TiposIndicador> TiposIndicadorIdUsuarioModificacionNavigation { get; set; } = new List<TiposIndicador>();

    public virtual ICollection<TiposJornada> TiposJornadaIdUsuarioCreacionNavigation { get; set; } = new List<TiposJornada>();

    public virtual ICollection<TiposJornada> TiposJornadaIdUsuarioModificacionNavigation { get; set; } = new List<TiposJornada>();

    public virtual ICollection<TiposMovimientoImss> TiposMovimientoImssIdUsuarioCreacionNavigation { get; set; } = new List<TiposMovimientoImss>();

    public virtual ICollection<TiposMovimientoImss> TiposMovimientoImssIdUsuarioModificacionNavigation { get; set; } = new List<TiposMovimientoImss>();

    public virtual ICollection<TiposNomina> TiposNominaIdUsuarioCreacionNavigation { get; set; } = new List<TiposNomina>();

    public virtual ICollection<TiposNomina> TiposNominaIdUsuarioModificacionNavigation { get; set; } = new List<TiposNomina>();

    public virtual ICollection<TiposPermiso> TiposPermisoIdUsuarioCreacionNavigation { get; set; } = new List<TiposPermiso>();

    public virtual ICollection<TiposPermiso> TiposPermisoIdUsuarioModificacionNavigation { get; set; } = new List<TiposPermiso>();

    public virtual ICollection<TiposRegimen> TiposRegimenIdUsuarioCreacionNavigation { get; set; } = new List<TiposRegimen>();

    public virtual ICollection<TiposRegimen> TiposRegimenIdUsuarioModificacionNavigation { get; set; } = new List<TiposRegimen>();

    public virtual ICollection<ZonasSalarioMinimo> ZonasSalarioMinimoIdUsuarioCreacionNavigation { get; set; } = new List<ZonasSalarioMinimo>();

    public virtual ICollection<ZonasSalarioMinimo> ZonasSalarioMinimoIdUsuarioModificacionNavigation { get; set; } = new List<ZonasSalarioMinimo>();
}
