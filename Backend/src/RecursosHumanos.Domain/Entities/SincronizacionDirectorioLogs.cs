using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class SincronizacionDirectorioLogs
{
    public long IdSincronizacionLog { get; set; }

    public int IdEmpleado { get; set; }

    public int IdAccionDirectorio { get; set; }

    public bool Exitoso { get; set; }

    public string? AtributosEnviados { get; set; }

    public string? Mensaje { get; set; }

    public DateTime FechaSincronizacion { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual AccionesDirectorio IdAccionDirectorioNavigation { get; set; } = null!;

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
