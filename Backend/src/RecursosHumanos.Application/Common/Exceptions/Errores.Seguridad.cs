namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Seguridad
    {
        public static NoEncontradoException RolNoEncontrado(int id) =>
            new("SEG-001", $"No existe el rol {id}.");

        public static ReglaNegocioException PermisoNoExiste(string permiso) =>
            new("SEG-002", $"No existe el permiso '{permiso}'.");

        public static ReglaNegocioException AlcanceRequerido(string permiso) =>
            new("SEG-003", $"El permiso '{permiso}' necesita alcance: todos, equipo o propio.");

        public static ReglaNegocioException AlcanceNoAplica(string permiso) =>
            new("SEG-004", $"El permiso '{permiso}' no usa alcance.");

        public static ReglaNegocioException PermisoRepetido(string permiso) =>
            new("SEG-005", $"El permiso '{permiso}' viene repetido.");

        public static ReglaNegocioException SinAdministradorDeRoles() =>
            new("SEG-006", "Al menos un rol con usuarios activos debe conservar el permiso de editar roles.");

        public static SinPermisoException RolSuperior() =>
            new("SEG-007", "No puedes asignar ni modificar un rol con más permisos que el tuyo.");

        public static ReglaNegocioException PropioUsuario() =>
            new("SEG-008", "No puedes cambiar tu propio rol, estatus o contraseña desde aquí.");

        public static NoEncontradoException UsuarioNoEncontrado(int id) =>
            new("SEG-009", $"No existe el usuario {id}.");

        public static ReglaNegocioException SoloLocal() =>
            new("SEG-010", "Solo los usuarios locales tienen contraseña en RecursosHumanos; los de Entra ID la cambian en su cuenta de Microsoft.");

        public static ReglaNegocioException ContrasenaDebil() =>
            new("SEG-011", "La contraseña nueva debe tener al menos 10 caracteres, con mayúscula, minúscula y número, y ser distinta a la actual.");

        public static ReglaNegocioException ContrasenaActualIncorrecta() =>
            new("SEG-012", "La contraseña actual no es correcta.");

        public static ReglaNegocioException EmpleadoYaTieneUsuario() =>
            new("SEG-013", "El empleado ya tiene usuario.");
    }
}
