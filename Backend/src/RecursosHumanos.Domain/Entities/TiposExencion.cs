using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class TiposExencion
{
    public int IdTipoExencion { get; set; }

    public string Clave { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal PorcentajeExento { get; set; }

    public decimal? TopeVecesUma { get; set; }

    public string Multiplicador { get; set; } = null!;

    public bool AcumulaAnual { get; set; }

    public string? Fundamento { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<ConceptosNomina> ConceptosNomina { get; set; } = new List<ConceptosNomina>();
}
