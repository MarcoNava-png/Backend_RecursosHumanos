using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Seguridad;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api")]
public class RolesController(RolesService roles) : ControllerBase
{
    [HttpGet("permisos")]
    public Task<List<PermisoDto>> Permisos(CancellationToken ct) => roles.PermisosAsync(ct);

    [HttpGet("roles")]
    public Task<List<RolDto>> Roles(CancellationToken ct) => roles.RolesAsync(ct);

    [HttpPost("roles")]
    public async Task<CreatedResult> Crear(NuevoRol nuevo, CancellationToken ct)
    {
        var id = await roles.CrearAsync(nuevo, ct);
        return Created("/api/roles", new { idRol = id });
    }

    [HttpPut("roles/{id:int}/permisos")]
    public async Task<NoContentResult> AsignarPermisos(int id, List<PermisoAsignadoDto> permisos, CancellationToken ct)
    {
        await roles.AsignarPermisosAsync(id, permisos, ct);
        return NoContent();
    }
}
