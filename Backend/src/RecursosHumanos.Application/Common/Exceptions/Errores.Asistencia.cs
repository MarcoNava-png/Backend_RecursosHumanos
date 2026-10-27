namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Asistencia
    {
        public static ReglaNegocioException OrigenNoValido(string origen) =>
            new("ASI-001", $"El origen '{origen}' no es válido para checadas de un checador (usa api_hikvision o usb_offline).");

        public static ReglaNegocioException RangoInvalido() =>
            new("ASI-002", "La fecha final no puede ser anterior a la inicial.");

        public static ReglaNegocioException RangoMuyGrande(int maximoDias) =>
            new("ASI-003", $"El rango no puede ser mayor a {maximoDias} días.");
    }
}
