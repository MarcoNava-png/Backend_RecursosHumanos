using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class ClavesSatConcepto
{
    public int IdClaveSatConcepto { get; set; }

    public int IdTipoConcepto { get; set; }

    public string Clave { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual TiposConcepto IdTipoConceptoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<ConceptosNomina> ConceptosNomina { get; set; } = new List<ConceptosNomina>();
}
