using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Empleados
{
    public int IdEmpleado { get; set; }

    public int IdPersona { get; set; }

    public int IdEmpresa { get; set; }

    public int IdDepartamento { get; set; }

    public int IdPerfil { get; set; }

    public int? IdJefeDirecto { get; set; }

    public int? NumeroChecador { get; set; }

    public string? Matricula { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public int IdEstatusLaboral { get; set; }

    public bool AvisoPrivacidadBiometricos { get; set; }

    public DateTime? FechaFirmaAviso { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public int? IdPuesto { get; set; }

    public int? IdCampus { get; set; }

    public string? ExtensionTelefono { get; set; }

    public virtual Campus? IdCampusNavigation { get; set; }

    public virtual Departamentos IdDepartamentoNavigation { get; set; } = null!;

    public virtual Empresas IdEmpresaNavigation { get; set; } = null!;

    public virtual EstatusLaborales IdEstatusLaboralNavigation { get; set; } = null!;

    public virtual Empleados? IdJefeDirectoNavigation { get; set; }

    public virtual Perfiles IdPerfilNavigation { get; set; } = null!;

    public virtual Personas IdPersonaNavigation { get; set; } = null!;

    public virtual Puestos? IdPuestoNavigation { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Empleados> InverseIdJefeDirectoNavigation { get; set; } = new List<Empleados>();

    public virtual ICollection<AsistenciaDiaria> AsistenciaDiaria { get; set; } = new List<AsistenciaDiaria>();

    public virtual ICollection<Asistencias> Asistencias { get; set; } = new List<Asistencias>();

    public virtual ICollection<BajasLaborales> BajasLaborales { get; set; } = new List<BajasLaborales>();

    public virtual ICollection<ConceptosHist> ConceptosHist { get; set; } = new List<ConceptosHist>();

    public virtual ICollection<ConceptosRecurrentes> ConceptosRecurrentes { get; set; } = new List<ConceptosRecurrentes>();

    public virtual ICollection<CreditosFonacot> CreditosFonacot { get; set; } = new List<CreditosFonacot>();

    public virtual ICollection<CreditosInfonavit> CreditosInfonavit { get; set; } = new List<CreditosInfonavit>();

    public virtual CuentasDirectorio? CuentasDirectorio { get; set; }

    public virtual ICollection<Departamentos> Departamentos { get; set; } = new List<Departamentos>();

    public virtual EmpleadosNomina? EmpleadosNomina { get; set; }

    public virtual ICollection<HistorialHorarios> HistorialHorarios { get; set; } = new List<HistorialHorarios>();

    public virtual ICollection<HistorialSueldos> HistorialSueldos { get; set; } = new List<HistorialSueldos>();

    public virtual ICollection<IncapacidadesImss> IncapacidadesImss { get; set; } = new List<IncapacidadesImss>();

    public virtual ICollection<IncidenciasNomina> IncidenciasNomina { get; set; } = new List<IncidenciasNomina>();

    public virtual ICollection<MovimientosImss> MovimientosImss { get; set; } = new List<MovimientosImss>();

    public virtual ICollection<Nominas> Nominas { get; set; } = new List<Nominas>();

    public virtual ICollection<PensionesAlimenticias> PensionesAlimenticias { get; set; } = new List<PensionesAlimenticias>();

    public virtual ICollection<PeriodosNomina> PeriodosNomina { get; set; } = new List<PeriodosNomina>();

    public virtual ICollection<PtuEmpleados> PtuEmpleados { get; set; } = new List<PtuEmpleados>();

    public virtual ICollection<SaldosVacaciones> SaldosVacaciones { get; set; } = new List<SaldosVacaciones>();

    public virtual ICollection<SincronizacionDirectorioLogs> SincronizacionDirectorioLogs { get; set; } = new List<SincronizacionDirectorioLogs>();

    public virtual ICollection<Solicitudes> SolicitudesIdAutorizadorJefeNavigation { get; set; } = new List<Solicitudes>();

    public virtual ICollection<Solicitudes> SolicitudesIdAutorizadorRhNavigation { get; set; } = new List<Solicitudes>();

    public virtual ICollection<Solicitudes> SolicitudesIdEmpleadoNavigation { get; set; } = new List<Solicitudes>();

    public virtual ICollection<Suplencias> SuplenciasIdAutorizadorNavigation { get; set; } = new List<Suplencias>();

    public virtual ICollection<Suplencias> SuplenciasIdEmpleadoSuplenteNavigation { get; set; } = new List<Suplencias>();

    public virtual ICollection<Suplencias> SuplenciasIdEmpleadoTitularNavigation { get; set; } = new List<Suplencias>();

    public virtual Usuarios? Usuarios { get; set; }
}
