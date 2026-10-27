using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Checadores
{
    public int IdChecador { get; set; }

    public string NombreChecador { get; set; } = null!;

    public string NumeroSerie { get; set; } = null!;

    public string? IpDispositivo { get; set; }

    public string? Ubicacion { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Asistencias> Asistencias { get; set; } = new List<Asistencias>();
}
