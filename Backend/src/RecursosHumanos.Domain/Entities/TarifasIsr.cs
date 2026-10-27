using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class TarifasIsr
{
    public int IdTarifaIsr { get; set; }

    public int Anio { get; set; }

    public string Periodicidad { get; set; } = null!;

    public decimal LimiteInferior { get; set; }

    public decimal? LimiteSuperior { get; set; }

    public decimal CuotaFija { get; set; }

    public decimal PorcentajeExcedente { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
