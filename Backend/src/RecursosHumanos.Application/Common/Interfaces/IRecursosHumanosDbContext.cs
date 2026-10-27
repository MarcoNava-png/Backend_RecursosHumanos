using RecursosHumanos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace RecursosHumanos.Application.Common.Interfaces;

public interface IRecursosHumanosDbContext
{
    DbSet<Empleados> Empleados { get; }
    DbSet<Personas> Personas { get; }
    DbSet<Usuarios> Usuarios { get; }
    DbSet<Departamentos> Departamentos { get; }
    DbSet<Perfiles> Perfiles { get; }
    DbSet<Puestos> Puestos { get; }
    DbSet<Campus> Campus { get; }
    DbSet<Empresas> Empresas { get; }
    DbSet<EstatusLaborales> EstatusLaborales { get; }
    DbSet<Asistencias> Asistencias { get; }
    DbSet<AsistenciaDiaria> AsistenciaDiaria { get; }
    DbSet<Checadores> Checadores { get; }
    DbSet<OrigenesChecada> OrigenesChecada { get; }
    DbSet<Solicitudes> Solicitudes { get; }
    DbSet<TiposPermiso> TiposPermiso { get; }
    DbSet<SaldosVacaciones> SaldosVacaciones { get; }
    DbSet<DiasFestivos> DiasFestivos { get; }
    DbSet<HistorialHorarios> HistorialHorarios { get; }
    DbSet<ReglasAsistencia> ReglasAsistencia { get; }
    DbSet<Materias> Materias { get; }
    DbSet<PeriodosEscolares> PeriodosEscolares { get; }
    DbSet<IncapacidadesImss> IncapacidadesImss { get; }
    DbSet<TiposIncapacidad> TiposIncapacidad { get; }
    DbSet<Suplencias> Suplencias { get; }
    DbSet<VwBiPeriodosNomina> VwBiPeriodosNomina { get; }
    DbSet<PeriodosNomina> PeriodosNomina { get; }
    DbSet<Nominas> Nominas { get; }
    DbSet<NominaDetalle> NominaDetalle { get; }
    DbSet<ConceptosNomina> ConceptosNomina { get; }
    DbSet<IncidenciasNomina> IncidenciasNomina { get; }
    DbSet<EmpleadosNomina> EmpleadosNomina { get; }
    DbSet<VwBiRecibos> VwBiRecibos { get; }
    DbSet<HistorialSueldos> HistorialSueldos { get; }
    DbSet<RegistrosPatronales> RegistrosPatronales { get; }
    DbSet<EsquemasPrestaciones> EsquemasPrestaciones { get; }
    DbSet<PeriodicidadesPago> PeriodicidadesPago { get; }
    DbSet<TiposContrato> TiposContrato { get; }
    DbSet<TiposRegimen> TiposRegimen { get; }
    DbSet<TiposJornada> TiposJornada { get; }
    DbSet<ClasesRiesgo> ClasesRiesgo { get; }
    DbSet<MetodosPago> MetodosPago { get; }
    DbSet<Bancos> Bancos { get; }
    DbSet<BajasLaborales> BajasLaborales { get; }
    DbSet<MotivosBaja> MotivosBaja { get; }
    DbSet<TiposNomina> TiposNomina { get; }
    DbSet<Ptu> Ptu { get; }
    DbSet<PtuEmpleados> PtuEmpleados { get; }
    DbSet<CreditosInfonavit> CreditosInfonavit { get; }
    DbSet<CreditosFonacot> CreditosFonacot { get; }
    DbSet<PensionesAlimenticias> PensionesAlimenticias { get; }
    DbSet<ConceptosHist> ConceptosHist { get; }
    DbSet<ConceptosRecurrentes> ConceptosRecurrentes { get; }
    DbSet<TiposDescuento> TiposDescuento { get; }
    DbSet<AuditoriaLogs> AuditoriaLogs { get; }
    DbSet<Roles> Roles { get; }
    DbSet<Permisos> Permisos { get; }
    DbSet<RolesPermisos> RolesPermisos { get; }
    DbSet<CuentasDirectorio> CuentasDirectorio { get; }
    DbSet<ProveedoresAutenticacion> ProveedoresAutenticacion { get; }
    DbSet<RegimenesFiscales> RegimenesFiscales { get; }
    DbSet<Estados> Estados { get; }
    DbSet<ZonasSalarioMinimo> ZonasSalarioMinimo { get; }
    DbSet<PeriodosAcumulacion> PeriodosAcumulacion { get; }
    DbSet<ModosChecada> ModosChecada { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
