using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class IncapacidadesImss
{
    public int IdIncapacidad { get; set; }

    public int IdEmpleado { get; set; }

    public string FolioImss { get; set; } = null!;

    public int DiasAutorizados { get; set; }

    public int IdTipoIncapacidad { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual TiposIncapacidad IdTipoIncapacidadNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
