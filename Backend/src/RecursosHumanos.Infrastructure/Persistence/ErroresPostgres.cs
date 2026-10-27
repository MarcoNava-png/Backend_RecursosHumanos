using Npgsql;
using RecursosHumanos.Application.Common.Exceptions;

namespace RecursosHumanos.Infrastructure.Persistence;

public static class ErroresPostgres
{
    public static ErrorAplicacionException? Traducir(Exception excepcion) =>
        (excepcion as PostgresException ?? excepcion.InnerException as PostgresException) switch
        {
            { SqlState: PostgresErrorCodes.RaiseException } e => Errores.BaseDatos.ReglaEnFuncion(e.MessageText),
            { SqlState: PostgresErrorCodes.UniqueViolation } e => Errores.BaseDatos.Duplicado(e.ConstraintName),
            { SqlState: PostgresErrorCodes.ForeignKeyViolation } e => Errores.BaseDatos.ReferenciaInvalida(e.ConstraintName),
            { SqlState: PostgresErrorCodes.CheckViolation } e => Errores.BaseDatos.RestriccionIncumplida(e.ConstraintName),
            _ => null
        };
}
