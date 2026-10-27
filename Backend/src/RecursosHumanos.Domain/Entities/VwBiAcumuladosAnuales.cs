using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiAcumuladosAnuales
{
    public int? Ejercicio { get; set; }

    public int? IdEmpresa { get; set; }

    public int? IdEmpleado { get; set; }

    public string? ClaveConcepto { get; set; }

    public string? NombreConcepto { get; set; }

    public string? TipoConcepto { get; set; }

    public string? ClaveSat { get; set; }

    public decimal? Gravado { get; set; }

    public decimal? Exento { get; set; }

    public decimal? Total { get; set; }
}
