using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Prestaciones;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/ptu")]
public class PtuController(PtuService ptu) : ControllerBase
{
    [HttpGet]
    public Task<List<PtuDto>> Listar(int? idEmpresa, CancellationToken ct) => ptu.ListarAsync(idEmpresa, ct);

    [HttpPost]
    public async Task<CreatedResult> Crear(NuevoPtu nuevo, CancellationToken ct)
    {
        var id = await ptu.CrearAsync(nuevo, ct);
        return Created($"/api/ptu/{id}/empleados", new { idPtu = id });
    }

    [HttpPut("{id:int}")]
    public async Task<NoContentResult> Actualizar(int id, ActualizarPtu datos, CancellationToken ct)
    {
        await ptu.ActualizarAsync(id, datos, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/calcular")]
    public Task<CalculoPtuDto> Calcular(int id, CancellationToken ct) => ptu.CalcularAsync(id, ct);

    [HttpGet("{id:int}/empleados")]
    public Task<List<PtuEmpleadoDto>> Empleados(int id, CancellationToken ct) => ptu.EmpleadosAsync(id, ct);

    [HttpPut("{id:int}/empleados/{idEmpleado:int}/promedio")]
    public async Task<NoContentResult> CapturarPromedio(int id, int idEmpleado, PromedioPtu promedio, CancellationToken ct)
    {
        await ptu.CapturarPromedioAsync(id, idEmpleado, promedio, ct);
        return NoContent();
    }
}
