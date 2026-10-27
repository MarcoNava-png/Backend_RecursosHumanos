using System.ComponentModel.DataAnnotations;

namespace RecursosHumanos.Application.Features.Asistencia;

public record ChecadaSolicitud(
    [StringLength(60)] string NumeroSerieChecador,
    int NumeroChecador,
    DateTimeOffset FechaHora,
    long NumeroEvento);

public record LoteChecadasSolicitud(string Origen, [Length(1, 5000)] List<ChecadaSolicitud> Checadas);

public record ChecadaRechazadaDto(string NumeroSerieChecador, long NumeroEvento, string Motivo);

public record ResultadoLoteDto(int Recibidas, int Registradas, int Duplicadas, List<ChecadaRechazadaDto> Rechazadas,
    int JornadasRecalculadas);

public record ProcesarSolicitud(DateOnly FechaInicio, DateOnly FechaFin);

public record ProcesarResultadoDto(int JornadasEvaluadas);

public record FiltroAsistencia(DateOnly FechaInicio, DateOnly FechaFin, int? IdEmpleado,
    int NumeroPagina = 1, [Range(1, 500)] int Tamano = 100);

public record AsistenciaDiariaDto(long IdAsistenciaDiaria, int IdEmpleado, string Empleado, DateOnly Fecha,
    TimeOnly HoraEntrada, TimeOnly HoraSalida, string? Materia, string Estatus, string DescripcionEstatus,
    string? ColorHex, int MinutosRetardo, decimal HorasCubiertas, int? IdSolicitud);

public record ChecadaDto(long IdAsistencia, DateTime FechaHora, string? Checador, string Origen);
