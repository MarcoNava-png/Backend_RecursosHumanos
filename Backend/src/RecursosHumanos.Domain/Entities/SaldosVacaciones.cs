using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class SaldosVacaciones
{
    public int IdSaldoVacacion { get; set; }

    public int IdEmpleado { get; set; }

    public int IdLftVacacion { get; set; }

    public int AnioServicio { get; set; }

    public int DiasOtorgados { get; set; }

    public int DiasExcepcion { get; set; }

    public int DiasDisfrutados { get; set; }

    public DateOnly FechaCaducidad { get; set; }

    public bool PrimaPagada { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual LftVacaciones IdLftVacacionNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
