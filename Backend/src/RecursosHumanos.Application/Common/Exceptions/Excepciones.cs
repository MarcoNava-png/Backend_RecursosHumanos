namespace RecursosHumanos.Application.Common.Exceptions;

public abstract class ErrorAplicacionException(string codigo, string mensaje) : Exception(mensaje)
{
    public string Codigo { get; } = codigo;
}

public class ReglaNegocioException(string codigo, string mensaje) : ErrorAplicacionException(codigo, mensaje);

public class NoAutenticadoException(string codigo, string mensaje) : ErrorAplicacionException(codigo, mensaje);

public class SinPermisoException(string codigo, string mensaje) : ErrorAplicacionException(codigo, mensaje);

public class NoEncontradoException(string codigo, string mensaje) : ErrorAplicacionException(codigo, mensaje);
