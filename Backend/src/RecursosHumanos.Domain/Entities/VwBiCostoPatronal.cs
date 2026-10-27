using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiCostoPatronal
{
    public int? IdPeriodoNomina { get; set; }

    public int? IdEmpleado { get; set; }

    public decimal? TotalPercepciones { get; set; }

    public decimal? ImssPatronal { get; set; }

    public decimal? InfonavitPatronal { get; set; }

    public decimal? CostoTotal { get; set; }
}
