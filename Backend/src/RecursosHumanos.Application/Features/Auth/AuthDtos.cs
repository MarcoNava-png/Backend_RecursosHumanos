using RecursosHumanos.Application.Features.Seguridad;

namespace RecursosHumanos.Application.Features.Auth;

public record LoginSolicitud(string NombreUsuario, string Contrasena);

public record LoginEntraSolicitud(string Token);

public record SesionDto(int IdUsuario, int? IdEmpleado, string NombreUsuario, string Rol, string NombreRol,
    bool DebeCambiarContrasena, List<PermisoAsignadoDto> Permisos);

public record LoginRespuesta(string Token, DateTime ExpiraUtc, SesionDto Sesion);

public record CambioContrasena(string Actual, string Nueva);
