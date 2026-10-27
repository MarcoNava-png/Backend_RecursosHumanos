using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Indicadores
{
    public int IdIndicador { get; set; }

    public int IdTipoIndicador { get; set; }

    public decimal ValorDiario { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Fuente { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual TiposIndicador IdTipoIndicadorNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
