using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class IncidenciasNomina
{
    public int IdIncidenciaNomina { get; set; }

    public int IdPeriodoNomina { get; set; }

    public int IdEmpleado { get; set; }

    public int IdConcepto { get; set; }

    public decimal Cantidad { get; set; }

    public decimal Importe { get; set; }

    public string? Observaciones { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual ConceptosNomina IdConceptoNavigation { get; set; } = null!;

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual PeriodosNomina IdPeriodoNominaNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
