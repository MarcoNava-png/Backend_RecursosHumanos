using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class CreditosFonacot
{
    public int IdCreditoFonacot { get; set; }

    public int IdEmpleado { get; set; }

    public string NumeroCredito { get; set; } = null!;

    public decimal ImporteTotal { get; set; }

    public decimal RetencionMensual { get; set; }

    public decimal Saldo { get; set; }

    public DateOnly FechaInicio { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<NominaDetalle> NominaDetalle { get; set; } = new List<NominaDetalle>();
}
