using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class PtuEmpleados
{
    public int IdPtuEmpleado { get; set; }

    public int IdPtu { get; set; }

    public int IdEmpleado { get; set; }

    public int DiasTrabajados { get; set; }

    public decimal SalarioAnual { get; set; }

    public decimal PtuPorDias { get; set; }

    public decimal PtuPorSalarios { get; set; }

    public decimal PtuCalculada { get; set; }

    public decimal TopeTresMeses { get; set; }

    public decimal PromedioTresAnios { get; set; }

    public decimal PtuFinal { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual Ptu IdPtuNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
