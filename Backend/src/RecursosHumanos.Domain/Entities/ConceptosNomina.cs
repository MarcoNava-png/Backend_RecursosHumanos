using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class ConceptosNomina
{
    public int IdConcepto { get; set; }

    public string ClaveConcepto { get; set; } = null!;

    public string NombreConcepto { get; set; } = null!;

    public int IdTipoConcepto { get; set; }

    public int? IdClaveSatConcepto { get; set; }

    public int IdTipoExencion { get; set; }

    public bool IntegraSdi { get; set; }

    public bool EsCalculado { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual ClavesSatConcepto? IdClaveSatConceptoNavigation { get; set; }

    public virtual TiposConcepto IdTipoConceptoNavigation { get; set; } = null!;

    public virtual TiposExencion IdTipoExencionNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<ConceptosHist> ConceptosHist { get; set; } = new List<ConceptosHist>();

    public virtual ICollection<ConceptosRecurrentes> ConceptosRecurrentes { get; set; } = new List<ConceptosRecurrentes>();

    public virtual ICollection<IncidenciasNomina> IncidenciasNomina { get; set; } = new List<IncidenciasNomina>();

    public virtual ICollection<NominaDetalle> NominaDetalle { get; set; } = new List<NominaDetalle>();
}
