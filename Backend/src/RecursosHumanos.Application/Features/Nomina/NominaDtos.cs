using System.ComponentModel.DataAnnotations;

namespace RecursosHumanos.Application.Features.Nomina;

public record FiltroPeriodos(int? IdEmpresa, int? Ejercicio, string? TipoNomina, string? Estatus);

public record PeriodoDto(int IdPeriodoNomina, int IdEmpresa, string TipoNomina, string Periodicidad, int Ejercicio,
    int NumeroPeriodo, string Descripcion, DateOnly FechaInicio, DateOnly FechaFin, DateOnly FechaPago, string Estatus,
    int Recibos, decimal TotalNeto);

public record GenerarPeriodos(int IdEmpresa, [Range(2000, 2100)] int Ejercicio, string Periodicidad = "quincenal");

public record PeriodosGeneradosDto(int PeriodosCreados);

public record NuevoPeriodoAguinaldo(int IdEmpresa, [Range(2000, 2100)] int Ejercicio, DateOnly FechaPago);

public record PeriodoCreadoDto(int IdPeriodoNomina);

public record CalculoDto(int RecibosCalculados);

public record IncidenciaNominaDto(int IdIncidenciaNomina, int IdEmpleado, string Empleado, int IdConcepto, string ClaveConcepto,
    string Concepto, string TipoConcepto, decimal Cantidad, decimal Importe, string? Observaciones);

public record NuevaIncidenciaNomina(int IdEmpleado, int IdConcepto, [Range(0.01, 9999)] decimal Cantidad,
    [Range(0.01, 9999999)] decimal Importe, [StringLength(300)] string? Observaciones);

public record IncidenciaGuardadaDto(int IdIncidenciaNomina, bool RequiereRecalculo);

public record ReciboDto(int IdNomina, int IdPeriodoNomina, string Periodo, string TipoNomina, DateOnly FechaPago,
    int IdEmpleado, string? Matricula, string Empleado, string Departamento, decimal DiasPagados,
    decimal TotalPercepciones, decimal TotalDeducciones, decimal TotalOtrosPagos, decimal IsrRetenido,
    decimal ImssObrero, decimal NetoAPagar, string Estatus);

public record ConceptoReciboDto(string Clave, string Concepto, string Tipo, decimal Cantidad, decimal Gravado,
    decimal Exento, decimal Total);

public record ReciboDetalleDto(ReciboDto Recibo, string Empresa, string? Rfc, string? Curp, string? Nss,
    decimal SalarioDiario, decimal SalarioDiarioIntegrado, string? Observaciones, List<ConceptoReciboDto> Conceptos);
