using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class EstatusAsistencia
{
    public int IdEstatusAsistencia { get; set; }

    public string Clave { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? ColorHex { get; set; }

    public bool EsFalta { get; set; }

    public bool GoceSueldo { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<AsistenciaDiaria> AsistenciaDiaria { get; set; } = new List<AsistenciaDiaria>();

    public virtual ICollection<TiposPermiso> TiposPermiso { get; set; } = new List<TiposPermiso>();
}
