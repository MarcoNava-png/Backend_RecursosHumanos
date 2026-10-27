using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiDirectorioPendientes
{
    public int? IdEmpleado { get; set; }

    public string? Empleado { get; set; }

    public string? Empresa { get; set; }

    public string? Faltantes { get; set; }
}
