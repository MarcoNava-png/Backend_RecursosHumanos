using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Features.Incidencias;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/incapacidades")]
public class IncapacidadesController(IncapacidadesService incapacidades) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<IncapacidadDto>> Listar([FromQuery] FiltroIncidencias filtro, CancellationToken ct) =>
        incapacidades.ListarAsync(filtro, ct);

    [HttpPost]
    public async Task<CreatedResult> Crear(NuevaIncapacidad nueva, CancellationToken ct)
    {
        var id = await incapacidades.CrearAsync(nueva, ct);
        return Created($"/api/incapacidades?idEmpleado={nueva.IdEmpleado}", new { idIncapacidad = id });
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<NoContentResult> Cancelar(int id, CancellationToken ct)
    {
        await incapacidades.CancelarAsync(id, ct);
        return NoContent();
    }
}
