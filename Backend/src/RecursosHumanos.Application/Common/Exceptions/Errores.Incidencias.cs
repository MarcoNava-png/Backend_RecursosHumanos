namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Incidencias
    {
        public static NoEncontradoException IncapacidadNoEncontrada(int id) =>
            new("INC-001", $"No existe la incapacidad {id}.");

        public static ReglaNegocioException FechasInvalidas() =>
            new("INC-002", "La fecha final no puede ser anterior a la inicial.");

        public static ReglaNegocioException IncapacidadSeTraslapa() =>
            new("INC-003", "El empleado ya tiene otra incapacidad en esas fechas.");

        public static ReglaNegocioException NominaCerrada() =>
            new("INC-004", "Esas fechas ya entraron a una nómina autorizada o cerrada; no se pueden modificar.");

        public static NoEncontradoException SuplenciaNoEncontrada(int id) =>
            new("INC-005", $"No existe la suplencia {id}.");

        public static ReglaNegocioException HorarioNoAplica() =>
            new("INC-006", "Ese horario no aplica en la fecha indicada (día de la semana o vigencia).");

        public static ReglaNegocioException SuplenteNoValido() =>
            new("INC-007", "El suplente debe ser otro empleado activo.");

        public static ReglaNegocioException SuplenteOcupado() =>
            new("INC-008", "El suplente tiene otra clase a esa hora.");

        public static ReglaNegocioException EstatusNoPermite(string accion) =>
            new("INC-009", $"La suplencia ya no se puede {accion} en su estatus actual.");

        public static ReglaNegocioException ClaseYaTieneSuplente() =>
            new("INC-010", "Esa clase ya tiene suplente ese día.");
    }
}
