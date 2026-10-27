using System.ComponentModel.DataAnnotations;

namespace RecursosHumanos.Application.Features.Nomina;

public record DatosNominaDto(int IdEmpleado, int IdRegistroPatronal, int IdEsquemaPrestaciones, string? Nss,
    DateOnly? FechaAltaImss, int IdPeriodicidadPago, int IdTipoContrato, int IdTipoRegimen, int IdTipoJornada,
    int IdClaseRiesgoPuesto, bool Sindicalizado, bool ExcluidoPtu, int IdMetodoPago, int? IdBanco, string? Clabe);

public record GuardarDatosNomina(
    int IdRegistroPatronal,
    int IdEsquemaPrestaciones,
    [RegularExpression("^[0-9]{11}$", ErrorMessage = "El NSS debe tener 11 dígitos.")] string? Nss,
    DateOnly? FechaAltaImss,
    int IdPeriodicidadPago,
    int IdTipoContrato,
    int IdTipoRegimen,
    int IdTipoJornada,
    int IdClaseRiesgoPuesto,
    bool Sindicalizado,
    bool ExcluidoPtu,
    int IdMetodoPago,
    int? IdBanco,
    [RegularExpression("^[0-9]{18}$", ErrorMessage = "La CLABE debe tener 18 dígitos.")] string? Clabe);

public record SueldoDto(int IdHistorialSueldo, decimal? SalarioDiario, decimal? TarifaPorHora,
    DateOnly FechaInicioVigencia, DateOnly? FechaFinVigencia, bool Vigente);

public record NuevoSueldo([Range(0.01, 999999)] decimal? SalarioDiario, [Range(0.01, 99999)] decimal? TarifaPorHora,
    DateOnly FechaInicioVigencia);
