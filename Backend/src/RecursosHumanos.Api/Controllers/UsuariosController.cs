using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Features.Seguridad;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(UsuariosService usuarios) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<UsuarioDto>> Listar([FromQuery] FiltroUsuarios filtro, CancellationToken ct) => usuarios.ListarAsync(filtro, ct);

    [HttpPost]
    public Task<UsuarioCreadoDto> Crear(NuevoUsuario nuevo, CancellationToken ct) => usuarios.CrearAsync(nuevo, ct);

    [HttpPut("{id:int}")]
    public async Task<NoContentResult> Actualizar(int id, ActualizarUsuario datos, CancellationToken ct)
    {
        await usuarios.ActualizarAsync(id, datos, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/restablecer-contrasena")]
    public Task<ContrasenaTemporalDto> RestablecerContrasena(int id, CancellationToken ct) => usuarios.RestablecerContrasenaAsync(id, ct);
}
