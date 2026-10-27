using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiConceptosNomina
{
    public int? IdConcepto { get; set; }

    public string? ClaveConcepto { get; set; }

    public string? NombreConcepto { get; set; }

    public int? IdTipoConcepto { get; set; }

    public int? IdClaveSatConcepto { get; set; }

    public int? IdTipoExencion { get; set; }

    public bool? IntegraSdi { get; set; }

    public bool? EsCalculado { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool? Activo { get; set; }

    public string? TipoConcepto { get; set; }

    public string? TipoExencion { get; set; }

    public string? ClaveSat { get; set; }

    public decimal? PorcentajeExento { get; set; }

    public decimal? TopeVecesUma { get; set; }

    public string? Multiplicador { get; set; }

    public bool? AcumulaAnual { get; set; }
}
