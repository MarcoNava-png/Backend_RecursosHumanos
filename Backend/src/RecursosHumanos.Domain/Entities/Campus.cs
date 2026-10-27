using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Campus
{
    public int IdCampus { get; set; }

    public int IdEmpresa { get; set; }

    public string Clave { get; set; } = null!;

    public string NombreCampus { get; set; } = null!;

    public string? CalleNumero { get; set; }

    public string? Ciudad { get; set; }

    public int? IdEstado { get; set; }

    public string? CodigoPostal { get; set; }

    public string? Telefono { get; set; }

    public string? OuDirectorio { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empresas IdEmpresaNavigation { get; set; } = null!;

    public virtual Estados? IdEstadoNavigation { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Empleados> Empleados { get; set; } = new List<Empleados>();
}
