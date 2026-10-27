using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Nomina;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/empleados/{idEmpleado:int}")]
public class DatosNominaController(DatosNominaService datos) : ControllerBase
{
    [HttpGet("nomina")]
    public Task<DatosNominaDto> Obtener(int idEmpleado, CancellationToken ct) => datos.ObtenerAsync(idEmpleado, ct);

    [HttpPut("nomina")]
    public async Task<NoContentResult> Guardar(int idEmpleado, GuardarDatosNomina datosNomina, CancellationToken ct)
    {
        await datos.GuardarAsync(idEmpleado, datosNomina, ct);
        return NoContent();
    }

    [HttpGet("sueldos")]
    public Task<List<SueldoDto>> Sueldos(int idEmpleado, CancellationToken ct) => datos.SueldosAsync(idEmpleado, ct);

    [HttpPost("sueldos")]
    public async Task<CreatedResult> RegistrarSueldo(int idEmpleado, NuevoSueldo nuevo, CancellationToken ct)
    {
        var id = await datos.RegistrarSueldoAsync(idEmpleado, nuevo, ct);
        return Created($"/api/empleados/{idEmpleado}/sueldos", new { idHistorialSueldo = id });
    }
}
