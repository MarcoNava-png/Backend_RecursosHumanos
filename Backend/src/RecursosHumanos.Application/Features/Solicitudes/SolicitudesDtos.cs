using System.ComponentModel.DataAnnotations;

namespace RecursosHumanos.Application.Features.Solicitudes;

public record NuevaSolicitud(int? IdEmpleado, int IdTipoPermiso, DateOnly FechaInicio, DateOnly FechaFin,
    TimeOnly? HoraInicio, TimeOnly? HoraFin);

public record RechazoSolicitud([StringLength(500, MinimumLength = 3)] string Motivo);

public record FiltroSolicitudes(int? IdEmpleado, string? Estatus, DateOnly? Desde, DateOnly? Hasta,
    int NumeroPagina = 1, [Range(1, 200)] int Tamano = 50);

public record SolicitudDto(int IdSolicitud, int IdEmpleado, string Empleado, int IdTipoPermiso, string TipoPermiso,
    DateOnly FechaInicio, DateOnly FechaFin, TimeOnly? HoraInicio, TimeOnly? HoraFin, decimal? DiasSolicitados,
    decimal? HorasSolicitadas, string Estatus, string? MotivoRechazo, DateTime? FechaAprobacionJefe,
    DateTime? FechaAprobacionRh, bool RequiereComprobante, bool TieneComprobante, DateTime FechaCreacion);

public record TopeAnualDto(int IdTipoPermiso, string TipoPermiso, int Tope, int Usados);

public record ArchivoDto(Stream Contenido, string TipoContenido, string Nombre);
