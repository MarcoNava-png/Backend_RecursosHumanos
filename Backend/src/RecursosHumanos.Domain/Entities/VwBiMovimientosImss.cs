using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiMovimientosImss
{
    public int? IdMovimientoImss { get; set; }

    public int? IdEmpleado { get; set; }

    public int? IdTipoMovimientoImss { get; set; }

    public DateOnly? FechaMovimiento { get; set; }

    public decimal? SalarioDiario { get; set; }

    public decimal? SalarioDiarioIntegrado { get; set; }

    public decimal? SalarioBaseCotizacion { get; set; }

    public int? IdCausaBajaImss { get; set; }

    public bool? EnviadoIdse { get; set; }

    public DateTime? FechaEnvioIdse { get; set; }

    public string? AcuseIdse { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool? Activo { get; set; }

    public string? TipoMovimiento { get; set; }

    public string? ClaveIdse { get; set; }
}
