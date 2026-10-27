using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Solicitudes
{
    public int IdSolicitud { get; set; }

    public int IdEmpleado { get; set; }

    public int IdTipoPermiso { get; set; }

    public long? IdAsistenciaOmision { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public TimeOnly? HoraInicioPermiso { get; set; }

    public TimeOnly? HoraFinPermiso { get; set; }

    public decimal? DiasSolicitados { get; set; }

    public decimal? HorasSolicitadas { get; set; }

    public string EstatusAprobacion { get; set; } = null!;

    public string? MotivoRechazo { get; set; }

    public DateTime? FechaAprobacionJefe { get; set; }

    public DateTime? FechaAprobacionRh { get; set; }

    public int? IdAutorizadorJefe { get; set; }

    public int? IdAutorizadorRh { get; set; }

    public string? RutaComprobanteAdjunto { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Asistencias? IdAsistenciaOmisionNavigation { get; set; }

    public virtual Empleados? IdAutorizadorJefeNavigation { get; set; }

    public virtual Empleados? IdAutorizadorRhNavigation { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual TiposPermiso IdTipoPermisoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<AsistenciaDiaria> AsistenciaDiaria { get; set; } = new List<AsistenciaDiaria>();
}
