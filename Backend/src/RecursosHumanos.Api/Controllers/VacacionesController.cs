using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Vacaciones;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/vacaciones")]
public class VacacionesController(VacacionesService vacaciones) : ControllerBase
{
    [HttpGet("{idEmpleado:int}")]
    public Task<ResumenVacacionesDto> Resumen(int idEmpleado, CancellationToken ct) => vacaciones.ResumenAsync(idEmpleado, ct);

    [HttpPost("generar-saldos")]
    public Task<SaldosGeneradosDto> GenerarSaldos(CancellationToken ct) => vacaciones.GenerarSaldosAsync(ct);
}
