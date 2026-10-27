namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Empleados
    {
        public static NoEncontradoException NoEncontrado(int id) =>
            new("EMP-001", $"No existe el empleado {id}.");

        public static ReglaNegocioException YaRegistradoEnEmpresa() =>
            new("EMP-002", "Esta persona ya está registrada como empleado en esa empresa.");

        public static ReglaNegocioException ChecadorSinAviso() =>
            new("EMP-003", "Para asignar número de checador primero debe firmar el aviso de privacidad de biométricos.");

        public static ReglaNegocioException JefeNoValido() =>
            new("EMP-004", "El jefe directo debe ser otro empleado activo.");

        public static ReglaNegocioException CambioDeEmpresa() =>
            new("EMP-005", "No se puede cambiar la empresa de un empleado: registra su baja y una nueva alta.");
    }
}
