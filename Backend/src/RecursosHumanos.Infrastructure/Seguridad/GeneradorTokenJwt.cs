using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RecursosHumanos.Application.Common.Interfaces;

namespace RecursosHumanos.Infrastructure.Seguridad;

public class GeneradorTokenJwt(JwtOpciones opciones, IReloj reloj) : IGeneradorToken
{
    public (string Token, DateTime ExpiraUtc) Generar(int idUsuario, int? idEmpleado, string nombreUsuario, string rol)
    {
        var expira = reloj.AhoraUtc.AddMinutes(opciones.MinutosExpiracion);
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = idUsuario.ToString(),
            [ClaimsToken.Nombre] = nombreUsuario,
            [ClaimsToken.Rol] = rol
        };
        if (idEmpleado is not null) claims[ClaimsToken.IdEmpleado] = idEmpleado.Value;

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = opciones.Emisor,
            Audience = opciones.Audiencia,
            Expires = expira,
            Claims = claims,
            SigningCredentials = new SigningCredentials(opciones.LlaveFirma(), SecurityAlgorithms.HmacSha256)
        });
        return (token, expira);
    }
}

public static class ClaimsToken
{
    public const string Nombre = "name";
    public const string Rol = "role";
    public const string IdEmpleado = "id_empleado";
}
