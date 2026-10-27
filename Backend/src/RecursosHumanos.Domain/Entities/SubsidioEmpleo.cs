using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class SubsidioEmpleo
{
    public int IdSubsidio { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public decimal PorcentajeUma { get; set; }

    public decimal UmaMensualBase { get; set; }

    public decimal IngresoTopeMensual { get; set; }

    public decimal? CuotaMensual { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
