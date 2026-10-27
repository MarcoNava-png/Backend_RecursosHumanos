using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Materias
{
    public int IdMateria { get; set; }

    public string NombreMateria { get; set; } = null!;

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<HistorialHorarios> HistorialHorarios { get; set; } = new List<HistorialHorarios>();
}
