using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class NominaDetalle
{
    public int IdNominaDetalle { get; set; }

    public int IdNomina { get; set; }

    public int IdConcepto { get; set; }

    public decimal Cantidad { get; set; }

    public decimal ImporteGravado { get; set; }

    public decimal ImporteExento { get; set; }

    public decimal? ImporteTotal { get; set; }

    public int? IdHistConcepto { get; set; }

    public int? IdCreditoInfonavit { get; set; }

    public int? IdCreditoFonacot { get; set; }

    public int? IdPension { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual ConceptosNomina IdConceptoNavigation { get; set; } = null!;

    public virtual CreditosFonacot? IdCreditoFonacotNavigation { get; set; }

    public virtual CreditosInfonavit? IdCreditoInfonavitNavigation { get; set; }

    public virtual ConceptosHist? IdHistConceptoNavigation { get; set; }

    public virtual Nominas IdNominaNavigation { get; set; } = null!;

    public virtual PensionesAlimenticias? IdPensionNavigation { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
