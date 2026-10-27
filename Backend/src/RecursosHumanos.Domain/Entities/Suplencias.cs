using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Suplencias
{
    public int IdSuplencia { get; set; }

    public int IdEmpleadoSuplente { get; set; }

    public int IdEmpleadoTitular { get; set; }

    public int IdHistorialHorario { get; set; }

    public int? IdAutorizador { get; set; }

    public DateOnly FechaSuplencia { get; set; }

    public string EstatusAprobacion { get; set; } = null!;

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados? IdAutorizadorNavigation { get; set; }

    public virtual Empleados IdEmpleadoSuplenteNavigation { get; set; } = null!;

    public virtual Empleados IdEmpleadoTitularNavigation { get; set; } = null!;

    public virtual HistorialHorarios IdHistorialHorarioNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
