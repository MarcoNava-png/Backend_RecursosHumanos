using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiRegistrosPatronales
{
    public int? IdRegistroPatronal { get; set; }

    public int? IdEmpresa { get; set; }

    public string? RegistroPatronal { get; set; }

    public int? IdClaseRiesgo { get; set; }

    public int? IdZonaSalarioMinimo { get; set; }

    public int? IdEstado { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool? Activo { get; set; }

    public string? AreaSalarioMinimo { get; set; }

    public string? ClaseRiesgo { get; set; }

    public string? EntidadFederativa { get; set; }
}
