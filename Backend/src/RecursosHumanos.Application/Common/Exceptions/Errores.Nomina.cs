namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Nomina
    {
        public static NoEncontradoException PeriodoNoEncontrado(int id) =>
            new("NOM-001", $"No existe el periodo de nómina {id}.");

        public static ReglaNegocioException PeriodoNoEditable(string estatus) =>
            new("NOM-002", $"El periodo está {estatus}; ya no se pueden capturar ni quitar incidencias.");

        public static ReglaNegocioException SoloOrdinaria() =>
            new("NOM-003", "Las incidencias solo se capturan en periodos de nómina ordinaria.");

        public static ReglaNegocioException ConceptoNoCapturable() =>
            new("NOM-004", "Ese concepto lo calcula el sistema; no se captura como incidencia.");

        public static ReglaNegocioException EmpleadoFueraDeNomina() =>
            new("NOM-005", "El empleado no está activo en la nómina de la empresa de ese periodo.");

        public static NoEncontradoException IncidenciaNoEncontrada(int id) =>
            new("NOM-006", $"No existe la incidencia {id} en ese periodo.");

        public static NoEncontradoException ReciboNoEncontrado(int id) =>
            new("NOM-007", $"No existe el recibo {id}.");

        public static SinPermisoException SinEmpleado() =>
            new("NOM-008", "Tu usuario no está ligado a un empleado; no puede autorizar la nómina.");

        public static ReglaNegocioException RegistroPatronalDeOtraEmpresa() =>
            new("NOM-009", "El registro patronal no pertenece a la empresa del empleado.");

        public static ReglaNegocioException EsquemaDeOtraEmpresa() =>
            new("NOM-010", "El esquema de prestaciones no pertenece a la empresa del empleado.");

        public static ReglaNegocioException FaltaClabe() =>
            new("NOM-011", "Ese método de pago requiere la CLABE del empleado.");

        public static ReglaNegocioException SueldoSinMonto() =>
            new("NOM-012", "Indica el salario diario o la tarifa por hora.");

        public static ReglaNegocioException SueldoAnteriorAlVigente(DateOnly inicioVigente) =>
            new("NOM-013", $"El nuevo sueldo debe empezar después del vigente (que inicia el {inicioVigente:dd/MM/yyyy}).");

        public static ReglaNegocioException NominaCerrada() =>
            new("NOM-014", "Esa fecha ya entró a una nómina autorizada o cerrada.");

        public static NoEncontradoException SinDatosNomina(int idEmpleado) =>
            new("NOM-015", $"El empleado {idEmpleado} no tiene datos de nómina.");

        public static ReglaNegocioException FiniquitoDesdeBaja() =>
            new("NOM-016", "El finiquito se calcula desde la baja del empleado.");

        public static ReglaNegocioException PeriodoSinPtu() =>
            new("NOM-017", "Ese periodo de PTU no está ligado a ningún reparto.");
    }
}
