using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class TiposPermiso
{
    public int IdTipoPermiso { get; set; }

    public string Descripcion { get; set; } = null!;

    public bool RequiereComprobante { get; set; }

    public bool EsConGoce { get; set; }

    public bool DescuentaSaldo { get; set; }

    public int IdTipoDuracionPermiso { get; set; }

    public int IdEstatusAsistencia { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public int? DiasPorAnio { get; set; }

    public virtual EstatusAsistencia IdEstatusAsistenciaNavigation { get; set; } = null!;

    public virtual TiposDuracionPermiso IdTipoDuracionPermisoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Solicitudes> Solicitudes { get; set; } = new List<Solicitudes>();
}
