using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Creditos;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api")]
public class CreditosController(CreditosService creditos) : ControllerBase
{
    [HttpGet("empleados/{idEmpleado:int}/creditos")]
    public Task<CreditosEmpleadoDto> Listar(int idEmpleado, CancellationToken ct) => creditos.ListarAsync(idEmpleado, ct);

    [HttpPost("creditos/infonavit")]
    public Task<CreadoDto> CrearInfonavit(NuevoInfonavit nuevo, CancellationToken ct) => creditos.CrearInfonavitAsync(nuevo, ct);

    [HttpPost("creditos/infonavit/{id:int}/suspender")]
    public async Task<NoContentResult> SuspenderInfonavit(int id, FechaTermino suspension, CancellationToken ct)
    {
        await creditos.SuspenderInfonavitAsync(id, suspension, ct);
        return NoContent();
    }

    [HttpPost("creditos/fonacot")]
    public Task<CreadoDto> CrearFonacot(NuevoFonacot nuevo, CancellationToken ct) => creditos.CrearFonacotAsync(nuevo, ct);

    [HttpPost("creditos/fonacot/{id:int}/cancelar")]
    public async Task<NoContentResult> CancelarFonacot(int id, CancellationToken ct)
    {
        await creditos.CancelarFonacotAsync(id, ct);
        return NoContent();
    }

    [HttpPost("creditos/pensiones")]
    public Task<CreadoDto> CrearPension(NuevaPension nueva, CancellationToken ct) => creditos.CrearPensionAsync(nueva, ct);

    [HttpPost("creditos/pensiones/{id:int}/terminar")]
    public async Task<NoContentResult> TerminarPension(int id, FechaTermino termino, CancellationToken ct)
    {
        await creditos.TerminarPensionAsync(id, termino, ct);
        return NoContent();
    }

    [HttpPost("creditos/prestamos")]
    public Task<CreadoDto> CrearPrestamo(NuevoPrestamo nuevo, CancellationToken ct) => creditos.CrearPrestamoAsync(nuevo, ct);

    [HttpPost("creditos/prestamos/{id:int}/cancelar")]
    public async Task<NoContentResult> CancelarPrestamo(int id, CancellationToken ct)
    {
        await creditos.CancelarPrestamoAsync(id, ct);
        return NoContent();
    }

    [HttpPost("creditos/recurrentes")]
    public Task<CreadoDto> CrearRecurrente(NuevoRecurrente nuevo, CancellationToken ct) => creditos.CrearRecurrenteAsync(nuevo, ct);

    [HttpPost("creditos/recurrentes/{id:int}/terminar")]
    public async Task<NoContentResult> TerminarRecurrente(int id, FechaTermino termino, CancellationToken ct)
    {
        await creditos.TerminarRecurrenteAsync(id, termino, ct);
        return NoContent();
    }
}
