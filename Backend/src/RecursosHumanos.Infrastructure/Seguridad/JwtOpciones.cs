using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RecursosHumanos.Infrastructure.Seguridad;

public class JwtOpciones
{
    public string Emisor { get; set; } = "";
    public string Audiencia { get; set; } = "";
    public string Clave { get; set; } = "";
    public int MinutosExpiracion { get; set; }

    public SymmetricSecurityKey LlaveFirma() => new(Encoding.UTF8.GetBytes(Clave));
}
