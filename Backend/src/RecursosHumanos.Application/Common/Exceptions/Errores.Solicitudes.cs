namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Solicitudes
    {
        public static NoEncontradoException NoEncontrada(int id) =>
            new("SOL-001", $"No existe la solicitud {id}.");

        public static ReglaNegocioException TipoNoValido() =>
            new("SOL-002", "El tipo de permiso seleccionado no existe.");

        public static ReglaNegocioException FechasInvalidas() =>
            new("SOL-003", "La fecha final no puede ser anterior a la inicial.");

        public static ReglaNegocioException RangoMuyGrande(int maximoDias) =>
            new("SOL-004", $"Una solicitud no puede abarcar más de {maximoDias} días.");

        public static ReglaNegocioException HorasInvalidas() =>
            new("SOL-005", "Un permiso por horas debe ser de un solo día, con hora de inicio menor a la de fin.");

        public static ReglaNegocioException SinDiasLaborables() =>
            new("SOL-006", "En esas fechas el empleado no tiene días laborables según su horario.");

        public static ReglaNegocioException Traslape() =>
            new("SOL-007", "Ya hay otra solicitud activa en esas fechas.");

        public static ReglaNegocioException EstatusNoPermite(string accion) =>
            new("SOL-008", $"La solicitud ya no se puede {accion} en su estatus actual.");

        public static SinPermisoException PropiaSolicitud() =>
            new("SOL-009", "No puedes aprobar ni rechazar tus propias solicitudes.");

        public static ReglaNegocioException FaltaComprobante() =>
            new("SOL-010", "Este tipo de permiso requiere comprobante. Adjúntalo antes de aprobar.");

        public static ReglaNegocioException ArchivoNoValido(int maximoMb) =>
            new("SOL-011", $"El comprobante debe ser PDF, JPG o PNG de máximo {maximoMb} MB.");

        public static NoEncontradoException SinComprobante() =>
            new("SOL-012", "La solicitud no tiene comprobante.");

        public static ReglaNegocioException TopeAnual(string tipo, int tope, int usados, int anio) =>
            new("SOL-013", $"{tipo}: el tope es de {tope} días en {anio} y ya lleva {usados}.");

        public static ReglaNegocioException NominaCerrada() =>
            new("SOL-014", "Esas fechas ya entraron a una nómina autorizada o cerrada; la solicitud no se puede modificar.");
    }
}
