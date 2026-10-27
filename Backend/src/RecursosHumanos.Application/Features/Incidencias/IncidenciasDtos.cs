using System.ComponentModel.DataAnnotations;

namespace RecursosHumanos.Application.Features.Incidencias;

public record NuevaIncapacidad(int IdEmpleado, [StringLength(30, MinimumLength = 1)] string FolioImss,
    int IdTipoIncapacidad, DateOnly FechaInicio, DateOnly FechaFin);

public record IncapacidadDto(int IdIncapacidad, int IdEmpleado, string Empleado, string FolioImss, int IdTipoIncapacidad,
    string TipoIncapacidad, DateOnly FechaInicio, DateOnly FechaFin, int DiasAutorizados);

public record FiltroIncidencias(int? IdEmpleado, DateOnly? Desde, DateOnly? Hasta, string? Estatus,
    int NumeroPagina = 1, [Range(1, 200)] int Tamano = 50);

public record NuevaSuplencia(int IdHistorialHorario, DateOnly FechaSuplencia, int IdEmpleadoSuplente);

public record SuplenciaDto(int IdSuplencia, DateOnly FechaSuplencia, int IdHistorialHorario, TimeOnly HoraEntrada,
    TimeOnly HoraSalida, string? Materia, int IdEmpleadoTitular, string Titular, int IdEmpleadoSuplente, string Suplente,
    string Estatus);
