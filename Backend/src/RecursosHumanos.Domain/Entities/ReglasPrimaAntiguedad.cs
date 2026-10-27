using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class ReglasPrimaAntiguedad
{
    public int IdReglaPrimaAntiguedad { get; set; }

    public string Clave { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int AniosMinimos { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<MotivosBaja> MotivosBaja { get; set; } = new List<MotivosBaja>();
}
