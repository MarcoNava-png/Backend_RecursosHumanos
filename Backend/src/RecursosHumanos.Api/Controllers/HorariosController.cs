using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Horarios;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/horarios")]
public class HorariosController(HorariosService horarios) : ControllerBase
{
    [HttpGet]
    public Task<List<HorarioDto>> Listar(int idEmpleado, CancellationToken ct, bool soloVigentes = true) =>
        horarios.ListarAsync(idEmpleado, soloVigentes, ct);

    [HttpPost]
    public async Task<CreatedResult> Crear(NuevoHorario nuevo, CancellationToken ct)
    {
        var id = await horarios.CrearAsync(nuevo, ct);
        return Created($"/api/horarios?idEmpleado={nuevo.IdEmpleado}", new { idHistorialHorario = id });
    }

    [HttpPost("{id:int}/terminar")]
    public async Task<NoContentResult> Terminar(int id, TerminarHorario terminar, CancellationToken ct)
    {
        await horarios.TerminarAsync(id, terminar, ct);
        return NoContent();
    }
}
