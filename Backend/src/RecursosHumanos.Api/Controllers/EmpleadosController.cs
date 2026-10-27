using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Features.Empleados;
using RecursosHumanos.Application.Features.Seguridad;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/empleados")]
public class EmpleadosController(EmpleadosService empleados, UsuariosService usuarios) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<EmpleadoDto>> Listar([FromQuery] FiltroEmpleados filtro, CancellationToken ct) =>
        empleados.ListarAsync(filtro, ct);

    [HttpGet("{id:int}")]
    public Task<EmpleadoDetalleDto> Obtener(int id, CancellationToken ct) => empleados.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<CreatedAtActionResult> Crear(GuardarEmpleadoSolicitud solicitud, CancellationToken ct)
    {
        var creado = await empleados.CrearAsync(solicitud, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdEmpleado }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<NoContentResult> Actualizar(int id, GuardarEmpleadoSolicitud solicitud, CancellationToken ct)
    {
        await empleados.ActualizarAsync(id, solicitud, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/acceso")]
    public Task<AccesoEmpleadoDto> CrearAcceso(int id, int? idRol, CancellationToken ct) => usuarios.ReintentarAccesoAsync(id, idRol, ct);
}
