using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class TiposDescuento
{
    public int IdTipoDescuento { get; set; }

    public string Clave { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public bool AplicaInfonavit { get; set; }

    public bool AplicaPension { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<CreditosInfonavit> CreditosInfonavit { get; set; } = new List<CreditosInfonavit>();

    public virtual ICollection<PensionesAlimenticias> PensionesAlimenticias { get; set; } = new List<PensionesAlimenticias>();
}
