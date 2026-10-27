using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Nomina;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/nomina")]
public class NominaController(PeriodosNominaService periodos, RecibosService recibos) : ControllerBase
{
    [HttpGet("periodos")]
    public Task<List<PeriodoDto>> Periodos([FromQuery] FiltroPeriodos filtro, CancellationToken ct) => periodos.ListarAsync(filtro, ct);

    [HttpPost("periodos/generar")]
    public Task<PeriodosGeneradosDto> GenerarPeriodos(GenerarPeriodos generar, CancellationToken ct) => periodos.GenerarAsync(generar, ct);

    [HttpPost("periodos/aguinaldo")]
    public Task<PeriodoCreadoDto> CrearAguinaldo(NuevoPeriodoAguinaldo nuevo, CancellationToken ct) => periodos.CrearAguinaldoAsync(nuevo, ct);

    [HttpPost("periodos/{id:int}/calcular")]
    public Task<CalculoDto> Calcular(int id, CancellationToken ct) => periodos.CalcularAsync(id, ct);

    [HttpPost("periodos/{id:int}/autorizar")]
    public async Task<NoContentResult> Autorizar(int id, CancellationToken ct)
    {
        await periodos.AutorizarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("periodos/{id:int}/cerrar")]
    public async Task<NoContentResult> Cerrar(int id, CancellationToken ct)
    {
        await periodos.CerrarAsync(id, ct);
        return NoContent();
    }

    [HttpGet("periodos/{id:int}/incidencias")]
    public Task<List<IncidenciaNominaDto>> Incidencias(int id, CancellationToken ct) => periodos.ListarIncidenciasAsync(id, ct);

    [HttpPost("periodos/{id:int}/incidencias")]
    public Task<IncidenciaGuardadaDto> CrearIncidencia(int id, NuevaIncidenciaNomina nueva, CancellationToken ct) =>
        periodos.CrearIncidenciaAsync(id, nueva, ct);

    [HttpDelete("periodos/{id:int}/incidencias/{idIncidencia:int}")]
    public Task<IncidenciaGuardadaDto> EliminarIncidencia(int id, int idIncidencia, CancellationToken ct) =>
        periodos.EliminarIncidenciaAsync(id, idIncidencia, ct);

    [HttpGet("periodos/{id:int}/recibos")]
    public Task<List<ReciboDto>> RecibosDelPeriodo(int id, CancellationToken ct) => recibos.ListarPorPeriodoAsync(id, ct);

    [HttpGet("recibos/mios")]
    public Task<List<ReciboDto>> MisRecibos(int? ejercicio, CancellationToken ct) => recibos.MisRecibosAsync(ejercicio, ct);

    [HttpGet("recibos/{idNomina:int}")]
    public Task<ReciboDetalleDto> Recibo(int idNomina, CancellationToken ct) => recibos.ObtenerAsync(idNomina, ct);
}
