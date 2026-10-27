using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class AsistenciaDiaria
{
    public long IdAsistenciaDiaria { get; set; }

    public int IdEmpleado { get; set; }

    public int IdHistorialHorario { get; set; }

    public int? IdSolicitud { get; set; }

    public DateOnly FechaDia { get; set; }

    public int IdEstatusAsistencia { get; set; }

    public int MinutosRetardo { get; set; }

    public decimal HorasCubiertas { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual EstatusAsistencia IdEstatusAsistenciaNavigation { get; set; } = null!;

    public virtual HistorialHorarios IdHistorialHorarioNavigation { get; set; } = null!;

    public virtual Solicitudes? IdSolicitudNavigation { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
