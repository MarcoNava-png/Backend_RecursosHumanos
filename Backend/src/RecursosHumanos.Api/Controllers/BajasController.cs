using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Prestaciones;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/bajas")]
public class BajasController(BajasService bajas) : ControllerBase
{
    [HttpGet]
    public Task<List<BajaDto>> Listar(string? estatusPago, CancellationToken ct) => bajas.ListarAsync(estatusPago, ct);

    [HttpPost]
    public async Task<CreatedResult> Registrar(NuevaBaja nueva, CancellationToken ct)
    {
        var id = await bajas.RegistrarAsync(nueva, ct);
        return Created("/api/bajas", new { idBaja = id });
    }

    [HttpPost("{id:int}/finiquito")]
    public Task<FiniquitoDto> CalcularFiniquito(int id, CalcularFiniquito calcular, CancellationToken ct) =>
        bajas.CalcularFiniquitoAsync(id, calcular, ct);

    [HttpPost("{id:int}/pagada")]
    public async Task<NoContentResult> MarcarPagada(int id, CancellationToken ct)
    {
        await bajas.MarcarPagadaAsync(id, ct);
        return NoContent();
    }
}
