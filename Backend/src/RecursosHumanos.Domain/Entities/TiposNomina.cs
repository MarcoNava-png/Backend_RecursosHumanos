using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class TiposNomina
{
    public int IdTipoNomina { get; set; }

    public string Clave { get; set; } = null!;

    public char ClaveSat { get; set; }

    public string Descripcion { get; set; } = null!;

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<PeriodosNomina> PeriodosNomina { get; set; } = new List<PeriodosNomina>();
}
