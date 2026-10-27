using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class DiasFestivos
{
    public int IdDiaFestivo { get; set; }

    public DateOnly Fecha { get; set; }

    public string DescripcionFestividad { get; set; } = null!;

    public bool EsLaborable { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
