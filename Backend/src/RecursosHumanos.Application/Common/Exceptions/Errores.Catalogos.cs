namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Catalogos
    {
        public static NoEncontradoException TipoPermisoNoEncontrado(int id) =>
            new("CAT-001", $"No existe el tipo de permiso {id}.");

        public static ReglaNegocioException VacacionesSinTope() =>
            new("CAT-002", "Las vacaciones no llevan tope por año: se controlan con los saldos de vacaciones.");

        public static NoEncontradoException NoEncontrado(string catalogo, int id) =>
            new("CAT-003", $"No existe {catalogo} {id}.");

        public static ReglaNegocioException EnUso(string catalogo) =>
            new("CAT-004", $"No se puede desactivar {catalogo}: lo usan empleados u horarios activos.");

        public static ReglaNegocioException RegimenNoAplica() =>
            new("CAT-005", "El régimen fiscal no corresponde al RFC (12 caracteres = persona moral, 13 = persona física).");

        public static ReglaNegocioException ResponsableNoValido() =>
            new("CAT-006", "El responsable del departamento debe ser un empleado activo.");

        public static ReglaNegocioException FechaEnNominaCerrada() =>
            new("CAT-007", "Esa fecha ya entró a una nómina autorizada o cerrada; el día festivo no se puede modificar.");

        public static ReglaNegocioException IpNoValida() =>
            new("CAT-008", "La IP del checador no es válida.");

        public static ReglaNegocioException FechasInvalidas() =>
            new("CAT-009", "La fecha final no puede ser anterior a la inicial.");
    }
}
