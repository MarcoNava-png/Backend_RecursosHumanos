using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Application.Features.Catalogos;

namespace RecursosHumanos.Api.Controllers;

[ApiController]
[Route("api/catalogos/admin")]
public class CatalogosAdminController(EmpresasService empresas, CatalogosPersonalService personal,
    CatalogosAsistenciaService asistencia) : ControllerBase
{
    [HttpGet("empresas")]
    public Task<List<EmpresaDto>> Empresas(bool incluirInactivos, CancellationToken ct) => empresas.EmpresasAsync(incluirInactivos, ct);

    [HttpPost("empresas")]
    public async Task<CreatedResult> CrearEmpresa(GuardarEmpresa datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/empresas", new { id = await empresas.CrearEmpresaAsync(datos, ct) });

    [HttpPut("empresas/{id:int}")]
    public async Task<NoContentResult> ActualizarEmpresa(int id, GuardarEmpresa datos, CancellationToken ct)
    {
        await empresas.ActualizarEmpresaAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("empresas/{id:int}")]
    public async Task<NoContentResult> DesactivarEmpresa(int id, CancellationToken ct)
    {
        await empresas.CambiarActivoEmpresaAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("empresas/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarEmpresa(int id, CancellationToken ct)
    {
        await empresas.CambiarActivoEmpresaAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("campus")]
    public Task<List<CampusDto>> Campus(bool incluirInactivos, int? idEmpresa, CancellationToken ct) => empresas.CampusAsync(incluirInactivos, idEmpresa, ct);

    [HttpPost("campus")]
    public async Task<CreatedResult> CrearCampus(GuardarCampus datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/campus", new { id = await empresas.CrearCampusAsync(datos, ct) });

    [HttpPut("campus/{id:int}")]
    public async Task<NoContentResult> ActualizarCampus(int id, GuardarCampus datos, CancellationToken ct)
    {
        await empresas.ActualizarCampusAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("campus/{id:int}")]
    public async Task<NoContentResult> DesactivarCampus(int id, CancellationToken ct)
    {
        await empresas.CambiarActivoCampusAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("campus/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarCampus(int id, CancellationToken ct)
    {
        await empresas.CambiarActivoCampusAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("registros-patronales")]
    public Task<List<RegistroPatronalDto>> RegistrosPatronales(bool incluirInactivos, int? idEmpresa, CancellationToken ct) => empresas.RegistrosPatronalesAsync(incluirInactivos, idEmpresa, ct);

    [HttpPost("registros-patronales")]
    public async Task<CreatedResult> CrearRegistroPatronal(GuardarRegistroPatronal datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/registros-patronales", new { id = await empresas.CrearRegistroPatronalAsync(datos, ct) });

    [HttpPut("registros-patronales/{id:int}")]
    public async Task<NoContentResult> ActualizarRegistroPatronal(int id, GuardarRegistroPatronal datos, CancellationToken ct)
    {
        await empresas.ActualizarRegistroPatronalAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("registros-patronales/{id:int}")]
    public async Task<NoContentResult> DesactivarRegistroPatronal(int id, CancellationToken ct)
    {
        await empresas.CambiarActivoRegistroPatronalAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("registros-patronales/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarRegistroPatronal(int id, CancellationToken ct)
    {
        await empresas.CambiarActivoRegistroPatronalAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("esquemas-prestaciones")]
    public Task<List<EsquemaPrestacionesDto>> Esquemas(bool incluirInactivos, int? idEmpresa, CancellationToken ct) => empresas.EsquemasAsync(incluirInactivos, idEmpresa, ct);

    [HttpPost("esquemas-prestaciones")]
    public async Task<CreatedResult> CrearEsquema(GuardarEsquemaPrestaciones datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/esquemas-prestaciones", new { id = await empresas.CrearEsquemaAsync(datos, ct) });

    [HttpPut("esquemas-prestaciones/{id:int}")]
    public async Task<NoContentResult> ActualizarEsquema(int id, GuardarEsquemaPrestaciones datos, CancellationToken ct)
    {
        await empresas.ActualizarEsquemaAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("esquemas-prestaciones/{id:int}")]
    public async Task<NoContentResult> DesactivarEsquema(int id, CancellationToken ct)
    {
        await empresas.CambiarActivoEsquemaAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("esquemas-prestaciones/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarEsquema(int id, CancellationToken ct)
    {
        await empresas.CambiarActivoEsquemaAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("departamentos")]
    public Task<List<DepartamentoDto>> Departamentos(bool incluirInactivos, CancellationToken ct) => personal.DepartamentosAsync(incluirInactivos, ct);

    [HttpPost("departamentos")]
    public async Task<CreatedResult> CrearDepartamento(GuardarDepartamento datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/departamentos", new { id = await personal.CrearDepartamentoAsync(datos, ct) });

    [HttpPut("departamentos/{id:int}")]
    public async Task<NoContentResult> ActualizarDepartamento(int id, GuardarDepartamento datos, CancellationToken ct)
    {
        await personal.ActualizarDepartamentoAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("departamentos/{id:int}")]
    public async Task<NoContentResult> DesactivarDepartamento(int id, CancellationToken ct)
    {
        await personal.CambiarActivoDepartamentoAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("departamentos/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarDepartamento(int id, CancellationToken ct)
    {
        await personal.CambiarActivoDepartamentoAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("puestos")]
    public Task<List<PuestoDto>> Puestos(bool incluirInactivos, CancellationToken ct) => personal.PuestosAsync(incluirInactivos, ct);

    [HttpPost("puestos")]
    public async Task<CreatedResult> CrearPuesto(GuardarPuesto datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/puestos", new { id = await personal.CrearPuestoAsync(datos, ct) });

    [HttpPut("puestos/{id:int}")]
    public async Task<NoContentResult> ActualizarPuesto(int id, GuardarPuesto datos, CancellationToken ct)
    {
        await personal.ActualizarPuestoAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("puestos/{id:int}")]
    public async Task<NoContentResult> DesactivarPuesto(int id, CancellationToken ct)
    {
        await personal.CambiarActivoPuestoAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("puestos/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarPuesto(int id, CancellationToken ct)
    {
        await personal.CambiarActivoPuestoAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("perfiles")]
    public Task<List<PerfilDto>> Perfiles(bool incluirInactivos, CancellationToken ct) => personal.PerfilesAsync(incluirInactivos, ct);

    [HttpPost("perfiles")]
    public async Task<CreatedResult> CrearPerfil(GuardarPerfil datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/perfiles", new { id = await personal.CrearPerfilAsync(datos, ct) });

    [HttpPut("perfiles/{id:int}")]
    public async Task<NoContentResult> ActualizarPerfil(int id, GuardarPerfil datos, CancellationToken ct)
    {
        await personal.ActualizarPerfilAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("perfiles/{id:int}")]
    public async Task<NoContentResult> DesactivarPerfil(int id, CancellationToken ct)
    {
        await personal.CambiarActivoPerfilAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("perfiles/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarPerfil(int id, CancellationToken ct)
    {
        await personal.CambiarActivoPerfilAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("dias-festivos")]
    public Task<List<DiaFestivoDto>> Festivos(bool incluirInactivos, int? anio, CancellationToken ct) => asistencia.FestivosAsync(incluirInactivos, anio, ct);

    [HttpPost("dias-festivos")]
    public async Task<CreatedResult> CrearFestivo(GuardarDiaFestivo datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/dias-festivos", new { id = await asistencia.CrearFestivoAsync(datos, ct) });

    [HttpPut("dias-festivos/{id:int}")]
    public async Task<NoContentResult> ActualizarFestivo(int id, GuardarDiaFestivo datos, CancellationToken ct)
    {
        await asistencia.ActualizarFestivoAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("dias-festivos/{id:int}")]
    public async Task<NoContentResult> DesactivarFestivo(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoFestivoAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("dias-festivos/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarFestivo(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoFestivoAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("checadores")]
    public Task<List<ChecadorDto>> Checadores(bool incluirInactivos, CancellationToken ct) => asistencia.ChecadoresAsync(incluirInactivos, ct);

    [HttpPost("checadores")]
    public async Task<CreatedResult> CrearChecador(GuardarChecador datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/checadores", new { id = await asistencia.CrearChecadorAsync(datos, ct) });

    [HttpPut("checadores/{id:int}")]
    public async Task<NoContentResult> ActualizarChecador(int id, GuardarChecador datos, CancellationToken ct)
    {
        await asistencia.ActualizarChecadorAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("checadores/{id:int}")]
    public async Task<NoContentResult> DesactivarChecador(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoChecadorAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("checadores/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarChecador(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoChecadorAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("reglas-asistencia")]
    public Task<List<ReglaAsistenciaDto>> Reglas(bool incluirInactivos, CancellationToken ct) => asistencia.ReglasAsync(incluirInactivos, ct);

    [HttpPost("reglas-asistencia")]
    public async Task<CreatedResult> CrearRegla(GuardarReglaAsistencia datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/reglas-asistencia", new { id = await asistencia.CrearReglaAsync(datos, ct) });

    [HttpPut("reglas-asistencia/{id:int}")]
    public async Task<NoContentResult> ActualizarRegla(int id, GuardarReglaAsistencia datos, CancellationToken ct)
    {
        await asistencia.ActualizarReglaAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("reglas-asistencia/{id:int}")]
    public async Task<NoContentResult> DesactivarRegla(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoReglaAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("reglas-asistencia/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarRegla(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoReglaAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("materias")]
    public Task<List<MateriaDto>> Materias(bool incluirInactivos, CancellationToken ct) => asistencia.MateriasAsync(incluirInactivos, ct);

    [HttpPost("materias")]
    public async Task<CreatedResult> CrearMateria(GuardarMateria datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/materias", new { id = await asistencia.CrearMateriaAsync(datos, ct) });

    [HttpPut("materias/{id:int}")]
    public async Task<NoContentResult> ActualizarMateria(int id, GuardarMateria datos, CancellationToken ct)
    {
        await asistencia.ActualizarMateriaAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("materias/{id:int}")]
    public async Task<NoContentResult> DesactivarMateria(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoMateriaAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("materias/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarMateria(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoMateriaAsync(id, true, ct);
        return NoContent();
    }

    [HttpGet("periodos-escolares")]
    public Task<List<PeriodoEscolarDto>> PeriodosEscolares(bool incluirInactivos, CancellationToken ct) => asistencia.PeriodosEscolaresAsync(incluirInactivos, ct);

    [HttpPost("periodos-escolares")]
    public async Task<CreatedResult> CrearPeriodoEscolar(GuardarPeriodoEscolar datos, CancellationToken ct) =>
        Created($"/api/catalogos/admin/periodos-escolares", new { id = await asistencia.CrearPeriodoEscolarAsync(datos, ct) });

    [HttpPut("periodos-escolares/{id:int}")]
    public async Task<NoContentResult> ActualizarPeriodoEscolar(int id, GuardarPeriodoEscolar datos, CancellationToken ct)
    {
        await asistencia.ActualizarPeriodoEscolarAsync(id, datos, ct);
        return NoContent();
    }

    [HttpDelete("periodos-escolares/{id:int}")]
    public async Task<NoContentResult> DesactivarPeriodoEscolar(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoPeriodoEscolarAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("periodos-escolares/{id:int}/reactivar")]
    public async Task<NoContentResult> ReactivarPeriodoEscolar(int id, CancellationToken ct)
    {
        await asistencia.CambiarActivoPeriodoEscolarAsync(id, true, ct);
        return NoContent();
    }
}
