using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Features.Auditoria;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/auditoria")]
public class AuditoriaController(AuditoriaService auditoria) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<MovimientoDto>> Listar([FromQuery] FiltroAuditoria filtro, CancellationToken ct) => auditoria.ListarAsync(filtro, ct);

    [HttpGet("tablas")]
    public Task<List<string>> Tablas(CancellationToken ct) => auditoria.TablasAsync(ct);
}
