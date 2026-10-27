namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Auth
    {
        public const string CodigoDemasiadosIntentos = "AUTH-002";

        public static NoAutenticadoException CredencialesInvalidas() =>
            new("AUTH-001", "Usuario o contraseña incorrectos.");

        public static NoAutenticadoException Bloqueado(int minutos) =>
            new("AUTH-003", $"Demasiados intentos fallidos. Intenta de nuevo en {minutos} minuto(s).");

        public static NoAutenticadoException SinAccesoEntra() =>
            new("AUTH-004", "Tu cuenta de Microsoft no tiene acceso a RecursosHumanos. Pide a Recursos Humanos que te den de alta.");

        public static ReglaNegocioException EntraNoConfigurado() =>
            new("AUTH-005", "El inicio de sesión con Microsoft no está configurado.");

        public static NoAutenticadoException TokenEntraInvalido() =>
            new("AUTH-006", "La sesión de Microsoft no es válida o ya venció. Vuelve a iniciar sesión.");

        public static ReglaNegocioException SinConexionEntra() =>
            new("AUTH-007", "No se pudo conectar con Microsoft. Intenta de nuevo en unos minutos.");
    }
}
