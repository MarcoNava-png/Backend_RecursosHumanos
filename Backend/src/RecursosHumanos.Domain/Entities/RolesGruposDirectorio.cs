using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class RolesGruposDirectorio
{
    public int IdRolGrupoDirectorio { get; set; }

    public int IdRol { get; set; }

    public string NombreGrupo { get; set; } = null!;

    public Guid? ObjectGuidGrupo { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Roles IdRolNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
