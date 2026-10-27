using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class CesantiaVejezPatron
{
    public int IdCesantiaVejez { get; set; }

    public int Anio { get; set; }

    public decimal VecesUmaDesde { get; set; }

    public decimal Porcentaje { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
