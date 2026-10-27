namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class General
    {
        public const string CodigoInesperado = "GEN-001";

        public static SinPermisoException SinPermiso() =>
            new("GEN-002", "No tienes permiso para realizar esta operación.");
    }
}
