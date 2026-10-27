using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class PensionesAlimenticias
{
    public int IdPension { get; set; }

    public int IdEmpleado { get; set; }

    public string Beneficiario { get; set; } = null!;

    public string? OficioJuzgado { get; set; }

    public int IdTipoDescuento { get; set; }

    public decimal Valor { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual TiposDescuento IdTipoDescuentoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<NominaDetalle> NominaDetalle { get; set; } = new List<NominaDetalle>();
}
