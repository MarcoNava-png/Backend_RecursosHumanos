using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class BajasLaborales
{
    public int IdBaja { get; set; }

    public int IdEmpleado { get; set; }

    public int IdMotivoBaja { get; set; }

    public DateOnly FechaBaja { get; set; }

    public decimal? MontoFiniquitoCalculado { get; set; }

    public string EstatusPago { get; set; } = null!;

    public int? IdNominaFiniquito { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual MotivosBaja IdMotivoBajaNavigation { get; set; } = null!;

    public virtual Nominas? IdNominaFiniquitoNavigation { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
