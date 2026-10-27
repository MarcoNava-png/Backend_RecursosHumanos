using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Features.Solicitudes;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/solicitudes")]
public class SolicitudesController(SolicitudesService solicitudes) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<SolicitudDto>> Listar([FromQuery] FiltroSolicitudes filtro, CancellationToken ct) =>
        solicitudes.ListarAsync(filtro, ct);

    [HttpGet("por-aprobar")]
    public Task<Pagina<SolicitudDto>> PorAprobar(CancellationToken ct, int numeroPagina = 1, int tamano = 50) =>
        solicitudes.PorAprobarAsync(numeroPagina, Math.Clamp(tamano, 1, 200), ct);

    [HttpGet("topes")]
    public Task<List<TopeAnualDto>> Topes(int idEmpleado, int anio, CancellationToken ct) => solicitudes.TopesAsync(idEmpleado, anio, ct);

    [HttpGet("{id:int}")]
    public Task<SolicitudDto> Obtener(int id, CancellationToken ct) => solicitudes.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<CreatedAtActionResult> Crear(NuevaSolicitud nueva, CancellationToken ct)
    {
        var id = await solicitudes.CrearAsync(nueva, ct);
        return CreatedAtAction(nameof(Obtener), new { id }, new { idSolicitud = id });
    }

    [HttpPost("{id:int}/aprobar")]
    public async Task<NoContentResult> Aprobar(int id, CancellationToken ct)
    {
        await solicitudes.AprobarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/rechazar")]
    public async Task<NoContentResult> Rechazar(int id, RechazoSolicitud rechazo, CancellationToken ct)
    {
        await solicitudes.RechazarAsync(id, rechazo, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<NoContentResult> Cancelar(int id, CancellationToken ct)
    {
        await solicitudes.CancelarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/comprobante")]
    public async Task<NoContentResult> AdjuntarComprobante(int id, IFormFile archivo, CancellationToken ct)
    {
        await using var contenido = archivo.OpenReadStream();
        await solicitudes.AdjuntarComprobanteAsync(id, contenido, archivo.FileName, archivo.Length, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/comprobante")]
    public async Task<FileStreamResult> Comprobante(int id, CancellationToken ct)
    {
        var archivo = await solicitudes.ComprobanteAsync(id, ct);
        return File(archivo.Contenido, archivo.TipoContenido, archivo.Nombre);
    }
}
