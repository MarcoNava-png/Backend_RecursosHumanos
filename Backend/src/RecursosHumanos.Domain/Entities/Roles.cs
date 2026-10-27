using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Roles
{
    public int IdRol { get; set; }

    public string NombreRol { get; set; } = null!;

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public string Clave { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<RolesGruposDirectorio> RolesGruposDirectorio { get; set; } = new List<RolesGruposDirectorio>();

    public virtual ICollection<RolesPermisos> RolesPermisos { get; set; } = new List<RolesPermisos>();

    public virtual ICollection<Usuarios> Usuarios { get; set; } = new List<Usuarios>();
}
