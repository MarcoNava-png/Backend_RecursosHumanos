using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;

namespace RecursosHumanos.Application.Features.Catalogos;

public record CatalogoDto(int Id, string Descripcion);

public record TipoPermisoDto(int Id, string Descripcion, bool PorHoras, bool RequiereComprobante, bool ConGoce, int? DiasPorAnio);

public record MetodoPagoDto(int Id, string Descripcion, bool RequiereClabe);

public record ConceptoNominaDto(int Id, string Clave, string Nombre, string Tipo);

public record TopeTipoPermiso([Range(1, 365)] int? DiasPorAnio);

public class CatalogosService(IRecursosHumanosDbContext db, IUsuarioActual usuario)
{
    public Task<List<CatalogoDto>> DepartamentosAsync(CancellationToken ct) =>
        db.Departamentos.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.NombreDepartamento)
            .Select(x => new CatalogoDto(x.IdDepartamento, x.NombreDepartamento)).ToListAsync(ct);

    public Task<List<CatalogoDto>> PerfilesAsync(CancellationToken ct) =>
        db.Perfiles.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.NombrePerfil)
            .Select(x => new CatalogoDto(x.IdPerfil, x.NombrePerfil)).ToListAsync(ct);

    public Task<List<CatalogoDto>> PuestosAsync(CancellationToken ct) =>
        db.Puestos.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.NombrePuesto)
            .Select(x => new CatalogoDto(x.IdPuesto, x.NombrePuesto)).ToListAsync(ct);

    public Task<List<CatalogoDto>> CampusAsync(int? idEmpresa, CancellationToken ct) =>
        db.Campus.AsNoTracking().Where(x => x.Activo && (idEmpresa == null || x.IdEmpresa == idEmpresa))
            .OrderBy(x => x.NombreCampus)
            .Select(x => new CatalogoDto(x.IdCampus, x.NombreCampus)).ToListAsync(ct);

    public Task<List<CatalogoDto>> EmpresasAsync(CancellationToken ct) =>
        db.Empresas.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.NombreComercial)
            .Select(x => new CatalogoDto(x.IdEmpresa, x.NombreComercial)).ToListAsync(ct);

    public Task<List<CatalogoDto>> EstatusLaboralesAsync(CancellationToken ct) =>
        db.EstatusLaborales.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.IdEstatusLaboral)
            .Select(x => new CatalogoDto(x.IdEstatusLaboral, x.Descripcion)).ToListAsync(ct);

    public Task<List<TipoPermisoDto>> TiposPermisoAsync(CancellationToken ct) =>
        db.TiposPermiso.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.IdTipoPermiso)
            .Select(x => new TipoPermisoDto(x.IdTipoPermiso, x.Descripcion,
                x.IdTipoDuracionPermisoNavigation.Clave == TipoDuracionPermiso.PorHoras, x.RequiereComprobante, x.EsConGoce, x.DiasPorAnio))
            .ToListAsync(ct);

    public async Task ConfigurarTopeAsync(int idTipoPermiso, TopeTipoPermiso tope, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var tipo = await db.TiposPermiso.Include(t => t.IdEstatusAsistenciaNavigation)
                       .SingleOrDefaultAsync(t => t.IdTipoPermiso == idTipoPermiso && t.Activo, ct)
                   ?? throw Errores.Catalogos.TipoPermisoNoEncontrado(idTipoPermiso);
        if (tipo.IdEstatusAsistenciaNavigation.Clave == EstatusAsistencia.Vacaciones) throw Errores.Catalogos.VacacionesSinTope();
        tipo.DiasPorAnio = tope.DiasPorAnio;
        await db.SaveChangesAsync(ct);
    }

    public Task<List<CatalogoDto>> RegistrosPatronalesAsync(int? idEmpresa, CancellationToken ct) =>
        db.RegistrosPatronales.AsNoTracking().Where(x => x.Activo && (idEmpresa == null || x.IdEmpresa == idEmpresa))
            .OrderBy(x => x.RegistroPatronal).Select(x => new CatalogoDto(x.IdRegistroPatronal, x.RegistroPatronal)).ToListAsync(ct);

    public Task<List<CatalogoDto>> EsquemasPrestacionesAsync(int? idEmpresa, CancellationToken ct) =>
        db.EsquemasPrestaciones.AsNoTracking().Where(x => x.Activo && (idEmpresa == null || x.IdEmpresa == idEmpresa))
            .OrderBy(x => x.NombreEsquema).Select(x => new CatalogoDto(x.IdEsquemaPrestaciones, x.NombreEsquema)).ToListAsync(ct);

    public Task<List<CatalogoDto>> PeriodicidadesPagoAsync(CancellationToken ct) =>
        db.PeriodicidadesPago.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Descripcion)
            .Select(x => new CatalogoDto(x.IdPeriodicidadPago, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> TiposContratoAsync(CancellationToken ct) =>
        db.TiposContrato.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Descripcion)
            .Select(x => new CatalogoDto(x.IdTipoContrato, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> TiposRegimenAsync(CancellationToken ct) =>
        db.TiposRegimen.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Descripcion)
            .Select(x => new CatalogoDto(x.IdTipoRegimen, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> TiposJornadaAsync(CancellationToken ct) =>
        db.TiposJornada.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Descripcion)
            .Select(x => new CatalogoDto(x.IdTipoJornada, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> ClasesRiesgoAsync(CancellationToken ct) =>
        db.ClasesRiesgo.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Descripcion)
            .Select(x => new CatalogoDto(x.IdClaseRiesgo, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> BancosAsync(CancellationToken ct) =>
        db.Bancos.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.NombreCorto)
            .Select(x => new CatalogoDto(x.IdBanco, x.NombreCorto)).ToListAsync(ct);

    public Task<List<MetodoPagoDto>> MetodosPagoAsync(CancellationToken ct) =>
        db.MetodosPago.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.IdMetodoPago)
            .Select(x => new MetodoPagoDto(x.IdMetodoPago, x.Descripcion, x.RequiereClabe)).ToListAsync(ct);

    public Task<List<ConceptoNominaDto>> ConceptosCapturablesAsync(CancellationToken ct) =>
        db.ConceptosNomina.AsNoTracking().Where(x => x.Activo && !x.EsCalculado).OrderBy(x => x.ClaveConcepto)
            .Select(x => new ConceptoNominaDto(x.IdConcepto, x.ClaveConcepto, x.NombreConcepto, x.IdTipoConceptoNavigation.Clave))
            .ToListAsync(ct);

    public Task<List<CatalogoDto>> MotivosBajaAsync(CancellationToken ct) =>
        db.MotivosBaja.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.IdMotivoBaja)
            .Select(x => new CatalogoDto(x.IdMotivoBaja, x.DescripcionMotivo)).ToListAsync(ct);

    public Task<List<CatalogoDto>> TiposDescuentoAsync(string? aplicaA, CancellationToken ct) =>
        db.TiposDescuento.AsNoTracking()
            .Where(x => x.Activo && (aplicaA == null || (aplicaA == "infonavit" && x.AplicaInfonavit) || (aplicaA == "pension" && x.AplicaPension)))
            .OrderBy(x => x.IdTipoDescuento).Select(x => new CatalogoDto(x.IdTipoDescuento, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> RegimenesFiscalesAsync(CancellationToken ct) =>
        db.RegimenesFiscales.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Clave)
            .Select(x => new CatalogoDto(x.IdRegimenFiscal, x.Clave + " " + x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> EstadosAsync(CancellationToken ct) =>
        db.Estados.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Descripcion)
            .Select(x => new CatalogoDto(x.IdEstado, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> ZonasSalarioMinimoAsync(CancellationToken ct) =>
        db.ZonasSalarioMinimo.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.IdZonaSalarioMinimo)
            .Select(x => new CatalogoDto(x.IdZonaSalarioMinimo, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> PeriodosAcumulacionAsync(CancellationToken ct) =>
        db.PeriodosAcumulacion.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.IdPeriodoAcumulacion)
            .Select(x => new CatalogoDto(x.IdPeriodoAcumulacion, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> ModosChecadaAsync(CancellationToken ct) =>
        db.ModosChecada.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.IdModoChecada)
            .Select(x => new CatalogoDto(x.IdModoChecada, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> TiposIncapacidadAsync(CancellationToken ct) =>
        db.TiposIncapacidad.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Clave)
            .Select(x => new CatalogoDto(x.IdTipoIncapacidad, x.Descripcion)).ToListAsync(ct);

    public Task<List<CatalogoDto>> ReglasAsistenciaAsync(CancellationToken ct) =>
        db.ReglasAsistencia.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.NombreRegla)
            .Select(x => new CatalogoDto(x.IdReglaAsistencia, x.NombreRegla)).ToListAsync(ct);

    public Task<List<CatalogoDto>> MateriasAsync(CancellationToken ct) =>
        db.Materias.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.NombreMateria)
            .Select(x => new CatalogoDto(x.IdMateria, x.NombreMateria)).ToListAsync(ct);

    public Task<List<CatalogoDto>> PeriodosEscolaresAsync(CancellationToken ct) =>
        db.PeriodosEscolares.AsNoTracking().Where(x => x.Activo).OrderByDescending(x => x.FechaInicio)
            .Select(x => new CatalogoDto(x.IdPeriodo, x.NombrePeriodo)).ToListAsync(ct);
}
