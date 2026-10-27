using System.Security.Cryptography;

namespace RecursosHumanos.Application.Common;

public record PoliticaAcceso(int IntentosMaximos, int MinutosBloqueo);

public static class Contrasenas
{
    private const string Caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!#$%*+-=?@";

    public static bool CumplePolitica(string contrasena) =>
        contrasena.Length >= 10 && contrasena.Any(char.IsUpper) && contrasena.Any(char.IsLower) && contrasena.Any(char.IsDigit);

    public static string Temporal()
    {
        string contrasena;
        do contrasena = new string(Enumerable.Range(0, 14).Select(_ => Caracteres[RandomNumberGenerator.GetInt32(Caracteres.Length)]).ToArray());
        while (!CumplePolitica(contrasena));
        return contrasena;
    }

    public static string Hash(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena, workFactor: 12);
}
