using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class PrimasRiesgoTrabajo
{
    public int IdPrimaRiesgo { get; set; }

    public int IdRegistroPatronal { get; set; }

    public decimal Prima { get; set; }

    public DateOnly FechaInicio { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual RegistrosPatronales IdRegistroPatronalNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
