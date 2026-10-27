using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Asistencias
{
    public long IdAsistencia { get; set; }

    public int IdEmpleado { get; set; }

    public int? IdChecador { get; set; }

    public DateTime FechaHoraRegistro { get; set; }

    public string TipoRegistro { get; set; } = null!;

    public int IdOrigenChecada { get; set; }

    public long? NumeroEventoHikvision { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Checadores? IdChecadorNavigation { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual OrigenesChecada IdOrigenChecadaNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Solicitudes> Solicitudes { get; set; } = new List<Solicitudes>();
}
