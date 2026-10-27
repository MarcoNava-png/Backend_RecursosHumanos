using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class HistorialSueldos
{
    public int IdHistorialSueldo { get; set; }

    public int IdEmpleado { get; set; }

    public decimal? SalarioBase { get; set; }

    public decimal? TarifaPorHora { get; set; }

    public DateOnly FechaInicioVigencia { get; set; }

    public DateOnly? FechaFinVigencia { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
