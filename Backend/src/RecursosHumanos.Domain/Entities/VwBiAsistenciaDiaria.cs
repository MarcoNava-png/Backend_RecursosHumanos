using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiAsistenciaDiaria
{
    public long? IdAsistenciaDiaria { get; set; }

    public int? IdEmpleado { get; set; }

    public int? IdHistorialHorario { get; set; }

    public int? IdSolicitud { get; set; }

    public DateOnly? FechaDia { get; set; }

    public int? IdEstatusAsistencia { get; set; }

    public int? MinutosRetardo { get; set; }

    public decimal? HorasCubiertas { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool? Activo { get; set; }

    public string? EstatusAsistencia { get; set; }

    public string? DescripcionEstatus { get; set; }

    public string? ColorHex { get; set; }
}
