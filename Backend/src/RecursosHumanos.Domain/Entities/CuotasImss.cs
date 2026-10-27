using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class CuotasImss
{
    public int IdCuotaImss { get; set; }

    public string ClaveRamo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal? PorcentajePatron { get; set; }

    public decimal PorcentajeObrero { get; set; }

    public string BaseCalculo { get; set; } = null!;

    public string TipoDias { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
