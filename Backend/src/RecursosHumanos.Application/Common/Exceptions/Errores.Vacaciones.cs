namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Vacaciones
    {
        public static ReglaNegocioException SaldoInsuficiente(int disponibles) =>
            new("VAC-001", $"No hay días de vacaciones suficientes. Disponibles: {disponibles}.");
    }
}
