using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Features.Asistencia;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/asistencia")]
public class AsistenciaController(AsistenciaService asistencia) : ControllerBase
{
    [HttpPost("checadas")]
    public Task<ResultadoLoteDto> RegistrarChecadas(LoteChecadasSolicitud lote, CancellationToken ct) =>
        asistencia.RegistrarChecadasAsync(lote, ct);

    [HttpPost("procesar")]
    public Task<ProcesarResultadoDto> Procesar(ProcesarSolicitud solicitud, CancellationToken ct) =>
        asistencia.ProcesarAsync(solicitud, ct);

    [HttpGet("diaria")]
    public Task<Pagina<AsistenciaDiariaDto>> Diaria([FromQuery] FiltroAsistencia filtro, CancellationToken ct) =>
        asistencia.ListarDiariaAsync(filtro, ct);

    [HttpGet("checadas")]
    public Task<List<ChecadaDto>> Checadas(int idEmpleado, DateOnly fecha, CancellationToken ct) =>
        asistencia.ListarChecadasAsync(idEmpleado, fecha, ct);
}
