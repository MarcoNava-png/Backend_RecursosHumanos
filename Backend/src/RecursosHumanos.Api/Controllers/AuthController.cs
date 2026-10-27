using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Application.Features.Auth;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    public const string LimiteLogin = "login";

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(LimiteLogin)]
    public Task<LoginRespuesta> Login(LoginSolicitud solicitud, CancellationToken ct) =>
        auth.IniciarSesionAsync(solicitud, ct);

    [HttpPost("entra")]
    [AllowAnonymous]
    [EnableRateLimiting(LimiteLogin)]
    public Task<LoginRespuesta> LoginEntra(LoginEntraSolicitud solicitud, CancellationToken ct) =>
        auth.IniciarSesionEntraAsync(solicitud, ct);

    [HttpGet("entra/configuracion")]
    [AllowAnonymous]
    public ConfiguracionEntraId ConfiguracionEntra() => auth.ConfiguracionEntra();

    [HttpPost("cambiar-contrasena")]
    public async Task<NoContentResult> CambiarContrasena(CambioContrasena cambio, CancellationToken ct)
    {
        await auth.CambiarContrasenaAsync(cambio, ct);
        return NoContent();
    }

    [HttpGet("yo")]
    public Task<SesionDto> Yo(CancellationToken ct) => auth.YoAsync(ct);
}
