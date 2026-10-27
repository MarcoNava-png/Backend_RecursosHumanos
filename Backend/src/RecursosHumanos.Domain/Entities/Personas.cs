using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Personas
{
    public int IdPersona { get; set; }

    public string Nombre { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string? ApellidoMaterno { get; set; }

    public string? Curp { get; set; }

    public string? Rfc { get; set; }

    public string? CorreoElectronico { get; set; }

    public string? Telefono { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Empleados> Empleados { get; set; } = new List<Empleados>();
}
