using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Puestos
{
    public int IdPuesto { get; set; }

    public string Clave { get; set; } = null!;

    public string NombrePuesto { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Empleados> Empleados { get; set; } = new List<Empleados>();
}
