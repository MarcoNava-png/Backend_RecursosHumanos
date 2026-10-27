using Microsoft.AspNetCore.Diagnostics;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Infrastructure.Persistence;

namespace RecursosHumanos.Api;

public class ManejadorErrores(IProblemDetailsService problemDetails, ILogger<ManejadorErrores> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception excepcion, CancellationToken ct)
    {
        var error = excepcion as ErrorAplicacionException ?? ErroresPostgres.Traducir(excepcion);
        var estatus = error switch
        {
            ReglaNegocioException => StatusCodes.Status400BadRequest,
            NoAutenticadoException => StatusCodes.Status401Unauthorized,
            SinPermisoException => StatusCodes.Status403Forbidden,
            NoEncontradoException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        if (error is null)
            logger.LogError(excepcion, "Error no controlado en {Ruta}", http.Request.Path);

        http.Response.StatusCode = estatus;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = excepcion,
            ProblemDetails =
            {
                Status = estatus,
                Title = error?.Message ?? "Ocurrió un error inesperado.",
                Extensions = { ["codigo"] = error?.Codigo ?? Errores.General.CodigoInesperado }
            }
        });
    }
}
