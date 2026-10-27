using Microsoft.IdentityModel.JsonWebTokens;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Infrastructure.Seguridad;

namespace RecursosHumanos.Api.Seguridad;

public class UsuarioActual(IHttpContextAccessor http) : IUsuarioActual
{
    public const string LlavePermisos = "permisos";

    private static readonly IReadOnlyDictionary<string, string?> SinPermisos = new Dictionary<string, string?>();

    public int? IdUsuario => Entero(JwtRegisteredClaimNames.Sub);
    public int? IdEmpleado => Entero(ClaimsToken.IdEmpleado);
    public string? Ip => http.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public IReadOnlyDictionary<string, string?> Permisos =>
        http.HttpContext?.Items[LlavePermisos] as IReadOnlyDictionary<string, string?> ?? SinPermisos;

    private int? Entero(string claim) =>
        int.TryParse(http.HttpContext?.User.FindFirst(claim)?.Value, out var valor) ? valor : null;
}
