using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Nominas
{
    public int IdNomina { get; set; }

    public int IdPeriodoNomina { get; set; }

    public int IdEmpleado { get; set; }

    public decimal DiasPagados { get; set; }

    public decimal DiasFalta { get; set; }

    public decimal DiasIncapacidad { get; set; }

    public decimal SalarioDiario { get; set; }

    public decimal SalarioDiarioIntegrado { get; set; }

    public decimal SalarioBaseCotizacion { get; set; }

    public decimal TotalPercepciones { get; set; }

    public decimal TotalGravado { get; set; }

    public decimal TotalExento { get; set; }

    public decimal TotalDeducciones { get; set; }

    public decimal TotalOtrosPagos { get; set; }

    public decimal IsrCausado { get; set; }

    public decimal SubsidioCausado { get; set; }

    public decimal IsrRetenido { get; set; }

    public decimal ImssObrero { get; set; }

    public decimal NetoAPagar { get; set; }

    public decimal ImssPatronal { get; set; }

    public string? Observaciones { get; set; }

    public string EstatusRecibo { get; set; } = null!;

    public string? Serie { get; set; }

    public int? Folio { get; set; }

    public Guid? Uuid { get; set; }

    public DateTime? FechaTimbrado { get; set; }

    public string? RutaXml { get; set; }

    public string? RutaPdf { get; set; }

    public Guid? UuidSustituido { get; set; }

    public string? MensajeTimbrado { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual PeriodosNomina IdPeriodoNominaNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<BajasLaborales> BajasLaborales { get; set; } = new List<BajasLaborales>();

    public virtual ICollection<NominaCuotasImss> NominaCuotasImss { get; set; } = new List<NominaCuotasImss>();

    public virtual ICollection<NominaDetalle> NominaDetalle { get; set; } = new List<NominaDetalle>();
}
