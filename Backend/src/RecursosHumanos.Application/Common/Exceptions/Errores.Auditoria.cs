namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Auditoria
    {
        public static ReglaNegocioException RangoInvalido() =>
            new("AUD-001", "La fecha final no puede ser anterior a la inicial.");

        public static ReglaNegocioException RangoMuyGrande(int maximoDias) =>
            new("AUD-002", $"Consulta como máximo {maximoDias} días de bitácora a la vez.");
    }
}
