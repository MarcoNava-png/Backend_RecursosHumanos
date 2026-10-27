using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiPeriodosNomina
{
    public int? IdPeriodoNomina { get; set; }

    public int? IdEmpresa { get; set; }

    public int? IdTipoNomina { get; set; }

    public int? IdPeriodicidadPago { get; set; }

    public int? Ejercicio { get; set; }

    public int? NumeroPeriodo { get; set; }

    public string? Descripcion { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public DateOnly? FechaPago { get; set; }

    public decimal? DiasPago { get; set; }

    public string? EstatusCierre { get; set; }

    public int? IdAutorizadorNomina { get; set; }

    public DateTime? FechaAutorizacion { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool? Activo { get; set; }

    public string? TipoNomina { get; set; }

    public char? TipoNominaSat { get; set; }

    public string? Periodicidad { get; set; }

    public string? PeriodicidadSat { get; set; }
}
