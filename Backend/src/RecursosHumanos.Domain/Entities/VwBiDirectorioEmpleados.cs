using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class VwBiDirectorioEmpleados
{
    public int? IdEmpleado { get; set; }

    public string? SAmaccountName { get; set; }

    public string? UserPrincipalName { get; set; }

    public string? Mail { get; set; }

    public string? GivenName { get; set; }

    public string? Sn { get; set; }

    public string? DisplayName { get; set; }

    public string? Title { get; set; }

    public string? Department { get; set; }

    public string? Company { get; set; }

    public string? PhysicalDeliveryOfficeName { get; set; }

    public string? StreetAddress { get; set; }

    public string? L { get; set; }

    public string? St { get; set; }

    public string? PostalCode { get; set; }

    public string? EmployeeId { get; set; }

    public string? TelephoneNumber { get; set; }

    public string? Mobile { get; set; }

    public string? Manager { get; set; }

    public string? OuDestino { get; set; }

    public bool? Habilitada { get; set; }

    public bool? CuentaExistenteM365 { get; set; }

    public Guid? ObjectGuid { get; set; }

    public string? HashSincronizado { get; set; }

    public string? HashDatos { get; set; }

    public bool? RequiereSincronizacion { get; set; }
}
