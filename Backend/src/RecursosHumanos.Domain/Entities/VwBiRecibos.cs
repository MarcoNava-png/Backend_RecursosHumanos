using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiRecibos
{
    public int? IdNomina { get; set; }

    public int? IdPeriodoNomina { get; set; }

    public int? IdEmpresa { get; set; }

    public string? Empresa { get; set; }

    public string? TipoNomina { get; set; }

    public string? Periodo { get; set; }

    public DateOnly? FechaPago { get; set; }

    public int? IdEmpleado { get; set; }

    public string? Matricula { get; set; }

    public string? Empleado { get; set; }

    public string? Rfc { get; set; }

    public string? Curp { get; set; }

    public string? Nss { get; set; }

    public string? Departamento { get; set; }

    public decimal? DiasPagados { get; set; }

    public decimal? SalarioDiario { get; set; }

    public decimal? SalarioDiarioIntegrado { get; set; }

    public decimal? TotalPercepciones { get; set; }

    public decimal? TotalGravado { get; set; }

    public decimal? TotalExento { get; set; }

    public decimal? TotalDeducciones { get; set; }

    public decimal? TotalOtrosPagos { get; set; }

    public decimal? IsrCausado { get; set; }

    public decimal? SubsidioCausado { get; set; }

    public decimal? IsrRetenido { get; set; }

    public decimal? ImssObrero { get; set; }

    public decimal? NetoAPagar { get; set; }

    public decimal? ImssPatronal { get; set; }

    public string? EstatusRecibo { get; set; }

    public Guid? Uuid { get; set; }

    public string? Observaciones { get; set; }
}
