using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiVacacionesDisponibles
{
    public int? IdEmpleado { get; set; }

    public long? DiasDisponibles { get; set; }

    public DateOnly? ProximaCaducidad { get; set; }
}
