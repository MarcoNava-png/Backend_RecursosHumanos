using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiResumenAsistenciaPeriodo
{
    public int? IdPeriodoNomina { get; set; }

    public int? IdEmpleado { get; set; }

    public long? JornadasProgramadas { get; set; }

    public long? ATiempo { get; set; }

    public long? Retardos { get; set; }

    public long? FaltasInjustificadas { get; set; }

    public long? FaltasJustificadas { get; set; }

    public long? PermisosSinGoce { get; set; }

    public long? Vacaciones { get; set; }

    public long? Incapacidad { get; set; }

    public long? Suplidas { get; set; }

    public decimal? HorasCubiertas { get; set; }
}
