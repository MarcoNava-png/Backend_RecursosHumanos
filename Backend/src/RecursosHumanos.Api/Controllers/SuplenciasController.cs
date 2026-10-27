using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Features.Incidencias;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/suplencias")]
public class SuplenciasController(SuplenciasService suplencias) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<SuplenciaDto>> Listar([FromQuery] FiltroIncidencias filtro, CancellationToken ct) =>
        suplencias.ListarAsync(filtro, ct);

    [HttpPost]
    public async Task<CreatedResult> Crear(NuevaSuplencia nueva, CancellationToken ct)
    {
        var id = await suplencias.CrearAsync(nueva, ct);
        return Created($"/api/suplencias?desde={nueva.FechaSuplencia:yyyy-MM-dd}", new { idSuplencia = id });
    }

    [HttpPost("{id:int}/aprobar")]
    public async Task<NoContentResult> Aprobar(int id, CancellationToken ct)
    {
        await suplencias.AprobarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/rechazar")]
    public async Task<NoContentResult> Rechazar(int id, CancellationToken ct)
    {
        await suplencias.RechazarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<NoContentResult> Cancelar(int id, CancellationToken ct)
    {
        await suplencias.CancelarAsync(id, ct);
        return NoContent();
    }
}
