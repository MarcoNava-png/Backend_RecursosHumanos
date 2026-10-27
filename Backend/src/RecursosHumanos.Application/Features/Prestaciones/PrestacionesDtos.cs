using System.ComponentModel.DataAnnotations;

namespace RecursosHumanos.Application.Features.Prestaciones;

public record NuevaBaja(int IdEmpleado, int IdMotivoBaja, DateOnly FechaBaja);

public record BajaDto(int IdBaja, int IdEmpleado, string Empleado, int IdMotivoBaja, string Motivo, DateOnly FechaBaja,
    decimal? MontoFiniquito, int? IdNominaFiniquito, string EstatusPago);

public record CalcularFiniquito(DateOnly? FechaPago);

public record FiniquitoDto(int IdBaja, int IdNomina, int IdPeriodoNomina, decimal Neto);

public record NuevoPtu(int IdEmpresa, [Range(2000, 2100)] int Ejercicio, [Range(0.01, 9999999999)] decimal RentaGravable,
    [Range(0.01, 999999)] decimal? TopeSalarioConfianza, DateOnly FechaPago);

public record ActualizarPtu([Range(0.01, 9999999999)] decimal RentaGravable, [Range(0.01, 999999)] decimal? TopeSalarioConfianza);

public record PtuDto(int IdPtu, int IdEmpresa, int Ejercicio, decimal RentaGravable, decimal MontoRepartir,
    decimal? TopeSalarioConfianza, int? IdPeriodoNomina, string Estatus, int Empleados, decimal TotalRepartido);

public record PtuEmpleadoDto(int IdEmpleado, string Empleado, int DiasTrabajados, decimal SalarioAnual, decimal PtuPorDias,
    decimal PtuPorSalarios, decimal PtuCalculada, decimal TopeTresMeses, decimal PromedioTresAnios, decimal PtuFinal);

public record PromedioPtu([Range(0, 9999999)] decimal PromedioTresAnios);

public record CalculoPtuDto(int EmpleadosCalculados);
