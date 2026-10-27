namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Prestaciones
    {
        public static NoEncontradoException BajaNoEncontrada(int id) =>
            new("PRE-001", $"No existe la baja {id}.");

        public static ReglaNegocioException YaTieneBaja() =>
            new("PRE-002", "El empleado ya está dado de baja.");

        public static ReglaNegocioException FechaBajaFutura() =>
            new("PRE-003", "La baja se registra el día que surte efecto, no antes.");

        public static ReglaNegocioException FechaBajaAntesDeIngreso() =>
            new("PRE-004", "La fecha de baja no puede ser anterior al ingreso.");

        public static ReglaNegocioException BajaPagada() =>
            new("PRE-005", "El finiquito de esta baja ya se pagó.");

        public static ReglaNegocioException SinFiniquito() =>
            new("PRE-006", "Primero calcula el finiquito.");

        public static ReglaNegocioException FiniquitoSinAutorizar() =>
            new("PRE-007", "El finiquito debe estar autorizado antes de marcarlo como pagado.");

        public static NoEncontradoException PtuNoEncontrado(int id) =>
            new("PRE-008", $"No existe el reparto de PTU {id}.");

        public static ReglaNegocioException PtuNoEditable(string estatus) =>
            new("PRE-009", $"El periodo de la PTU está {estatus}; ya no se puede modificar.");

        public static NoEncontradoException EmpleadoSinPtu() =>
            new("PRE-010", "El empleado no está en el reparto; calcula la PTU primero.");

        public static ReglaNegocioException PtuDuplicado(int ejercicio) =>
            new("PRE-011", $"Ya existe el reparto de PTU del ejercicio {ejercicio} para la empresa.");
    }
}
