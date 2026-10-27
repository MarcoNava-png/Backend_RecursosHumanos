using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Catalogos;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/catalogos")]
public class CatalogosController(CatalogosService catalogos) : ControllerBase
{
    [HttpGet("departamentos")]
    public Task<List<CatalogoDto>> Departamentos(CancellationToken ct) => catalogos.DepartamentosAsync(ct);

    [HttpGet("perfiles")]
    public Task<List<CatalogoDto>> Perfiles(CancellationToken ct) => catalogos.PerfilesAsync(ct);

    [HttpGet("puestos")]
    public Task<List<CatalogoDto>> Puestos(CancellationToken ct) => catalogos.PuestosAsync(ct);

    [HttpGet("campus")]
    public Task<List<CatalogoDto>> Campus(int? idEmpresa, CancellationToken ct) => catalogos.CampusAsync(idEmpresa, ct);

    [HttpGet("empresas")]
    public Task<List<CatalogoDto>> Empresas(CancellationToken ct) => catalogos.EmpresasAsync(ct);

    [HttpGet("estatus-laborales")]
    public Task<List<CatalogoDto>> EstatusLaborales(CancellationToken ct) => catalogos.EstatusLaboralesAsync(ct);

    [HttpGet("tipos-permiso")]
    public Task<List<TipoPermisoDto>> TiposPermiso(CancellationToken ct) => catalogos.TiposPermisoAsync(ct);

    [HttpPut("tipos-permiso/{id:int}/tope")]
    public async Task<NoContentResult> ConfigurarTope(int id, TopeTipoPermiso tope, CancellationToken ct)
    {
        await catalogos.ConfigurarTopeAsync(id, tope, ct);
        return NoContent();
    }

    [HttpGet("nomina/registros-patronales")]
    public Task<List<CatalogoDto>> RegistrosPatronales(int? idEmpresa, CancellationToken ct) => catalogos.RegistrosPatronalesAsync(idEmpresa, ct);

    [HttpGet("nomina/esquemas-prestaciones")]
    public Task<List<CatalogoDto>> EsquemasPrestaciones(int? idEmpresa, CancellationToken ct) => catalogos.EsquemasPrestacionesAsync(idEmpresa, ct);

    [HttpGet("nomina/periodicidades-pago")]
    public Task<List<CatalogoDto>> PeriodicidadesPago(CancellationToken ct) => catalogos.PeriodicidadesPagoAsync(ct);

    [HttpGet("nomina/tipos-contrato")]
    public Task<List<CatalogoDto>> TiposContrato(CancellationToken ct) => catalogos.TiposContratoAsync(ct);

    [HttpGet("nomina/tipos-regimen")]
    public Task<List<CatalogoDto>> TiposRegimen(CancellationToken ct) => catalogos.TiposRegimenAsync(ct);

    [HttpGet("nomina/tipos-jornada")]
    public Task<List<CatalogoDto>> TiposJornada(CancellationToken ct) => catalogos.TiposJornadaAsync(ct);

    [HttpGet("nomina/clases-riesgo")]
    public Task<List<CatalogoDto>> ClasesRiesgo(CancellationToken ct) => catalogos.ClasesRiesgoAsync(ct);

    [HttpGet("nomina/bancos")]
    public Task<List<CatalogoDto>> Bancos(CancellationToken ct) => catalogos.BancosAsync(ct);

    [HttpGet("nomina/metodos-pago")]
    public Task<List<MetodoPagoDto>> MetodosPago(CancellationToken ct) => catalogos.MetodosPagoAsync(ct);

    [HttpGet("nomina/conceptos-capturables")]
    public Task<List<ConceptoNominaDto>> ConceptosCapturables(CancellationToken ct) => catalogos.ConceptosCapturablesAsync(ct);

    [HttpGet("motivos-baja")]
    public Task<List<CatalogoDto>> MotivosBaja(CancellationToken ct) => catalogos.MotivosBajaAsync(ct);

    [HttpGet("nomina/tipos-descuento")]
    public Task<List<CatalogoDto>> TiposDescuento(string? aplicaA, CancellationToken ct) => catalogos.TiposDescuentoAsync(aplicaA, ct);

    [HttpGet("regimenes-fiscales")]
    public Task<List<CatalogoDto>> RegimenesFiscales(CancellationToken ct) => catalogos.RegimenesFiscalesAsync(ct);

    [HttpGet("estados")]
    public Task<List<CatalogoDto>> Estados(CancellationToken ct) => catalogos.EstadosAsync(ct);

    [HttpGet("zonas-salario-minimo")]
    public Task<List<CatalogoDto>> ZonasSalarioMinimo(CancellationToken ct) => catalogos.ZonasSalarioMinimoAsync(ct);

    [HttpGet("periodos-acumulacion")]
    public Task<List<CatalogoDto>> PeriodosAcumulacion(CancellationToken ct) => catalogos.PeriodosAcumulacionAsync(ct);

    [HttpGet("modos-checada")]
    public Task<List<CatalogoDto>> ModosChecada(CancellationToken ct) => catalogos.ModosChecadaAsync(ct);

    [HttpGet("tipos-incapacidad")]
    public Task<List<CatalogoDto>> TiposIncapacidad(CancellationToken ct) => catalogos.TiposIncapacidadAsync(ct);

    [HttpGet("reglas-asistencia")]
    public Task<List<CatalogoDto>> ReglasAsistencia(CancellationToken ct) => catalogos.ReglasAsistenciaAsync(ct);

    [HttpGet("materias")]
    public Task<List<CatalogoDto>> Materias(CancellationToken ct) => catalogos.MateriasAsync(ct);

    [HttpGet("periodos-escolares")]
    public Task<List<CatalogoDto>> PeriodosEscolares(CancellationToken ct) => catalogos.PeriodosEscolaresAsync(ct);
}
