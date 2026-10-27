using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Departamentos
{
    public int IdDepartamento { get; set; }

    public string NombreDepartamento { get; set; } = null!;

    public int? IdResponsable { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public string? GrupoDirectorio { get; set; }

    public virtual Empleados? IdResponsableNavigation { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Empleados> Empleados { get; set; } = new List<Empleados>();
}
