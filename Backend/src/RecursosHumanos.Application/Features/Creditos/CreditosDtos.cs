using System.ComponentModel.DataAnnotations;

namespace RecursosHumanos.Application.Features.Creditos;

public record InfonavitDto(int IdCreditoInfonavit, string NumeroCredito, int IdTipoDescuento, string TipoDescuento,
    decimal ValorDescuento, bool IncluyeSeguroVivienda, DateOnly FechaInicio, DateOnly? FechaSuspension);

public record FonacotDto(int IdCreditoFonacot, string NumeroCredito, decimal ImporteTotal, decimal RetencionMensual,
    decimal Saldo, DateOnly FechaInicio);

public record PensionDto(int IdPension, string Beneficiario, string? OficioJuzgado, int IdTipoDescuento, string TipoDescuento,
    decimal Valor, DateOnly FechaInicio, DateOnly? FechaFin);

public record PrestamoDto(int IdPrestamo, int IdConcepto, string Concepto, decimal MontoTotal, int TotalParcialidades,
    int ParcialidadesPagadas, decimal SaldoRestante, DateOnly FechaInicioDescuento);

public record RecurrenteDto(int IdConceptoRecurrente, int IdConcepto, string Concepto, string TipoConcepto, decimal Importe,
    DateOnly FechaInicio, DateOnly? FechaFin);

public record CreditosEmpleadoDto(int IdEmpleado, List<InfonavitDto> Infonavit, List<FonacotDto> Fonacot,
    List<PensionDto> Pensiones, List<PrestamoDto> Prestamos, List<RecurrenteDto> Recurrentes);

public record NuevoInfonavit(int IdEmpleado, [StringLength(10, MinimumLength = 1)] string NumeroCredito, int IdTipoDescuento,
    [Range(0.0001, 999999)] decimal ValorDescuento, bool IncluyeSeguroVivienda, DateOnly FechaInicio);

public record NuevoFonacot(int IdEmpleado, [StringLength(20, MinimumLength = 1)] string NumeroCredito,
    [Range(0.01, 9999999)] decimal ImporteTotal, [Range(0.01, 999999)] decimal RetencionMensual, DateOnly FechaInicio);

public record NuevaPension(int IdEmpleado, [StringLength(200, MinimumLength = 1)] string Beneficiario,
    [StringLength(100)] string? OficioJuzgado, int IdTipoDescuento, [Range(0.0001, 999999)] decimal Valor,
    DateOnly FechaInicio, DateOnly? FechaFin);

public record NuevoPrestamo(int IdEmpleado, int IdConcepto, [Range(0.01, 9999999)] decimal MontoTotal,
    [Range(1, 520)] int TotalParcialidades, DateOnly FechaInicioDescuento);

public record NuevoRecurrente(int IdEmpleado, int IdConcepto, [Range(0.01, 9999999)] decimal Importe,
    DateOnly FechaInicio, DateOnly? FechaFin);

public record FechaTermino(DateOnly Fecha);

public record CreadoDto(int Id);
