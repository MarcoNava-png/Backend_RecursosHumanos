using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class ConceptosHist
{
    public int IdHistConcepto { get; set; }

    public int IdEmpleado { get; set; }

    public int IdConcepto { get; set; }

    public decimal MontoTotalDeuda { get; set; }

    public int TotalParcialidades { get; set; }

    public int ParcialidadesPagadas { get; set; }

    public decimal SaldoRestante { get; set; }

    public DateOnly FechaInicioDescuento { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual ConceptosNomina IdConceptoNavigation { get; set; } = null!;

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<NominaDetalle> NominaDetalle { get; set; } = new List<NominaDetalle>();
}
