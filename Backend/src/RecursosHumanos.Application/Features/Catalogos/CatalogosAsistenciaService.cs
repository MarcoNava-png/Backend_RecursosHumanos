using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Catalogos;

public record DiaFestivoDto(int IdDiaFestivo, DateOnly Fecha, string DescripcionFestividad, bool EsLaborable, bool Activo);

public record GuardarDiaFestivo(DateOnly Fecha, [StringLength(150, MinimumLength = 2)] string DescripcionFestividad, bool EsLaborable);

public record ChecadorDto(int IdChecador, string NombreChecador, string NumeroSerie, string? IpDispositivo, string? Ubicacion, bool Activo);

public record GuardarChecador([StringLength(100, MinimumLength = 2)] string NombreChecador,
    [StringLength(60, MinimumLength = 2)] string NumeroSerie, [StringLength(45)] string? IpDispositivo, [StringLength(150)] string? Ubicacion);

public record ReglaAsistenciaDto(int IdReglaAsistencia, string NombreRegla, int MinutosTolerancia, int MinutosLimiteFalta,
    int MaximoRetardosPermitidos, bool AplicaSancionFalta, int IdPeriodoAcumulacion, string PeriodoAcumulacion, int IdModoChecada,
    string ModoChecada, DateOnly FechaInicioVigencia, DateOnly? FechaFinVigencia, bool Activo);

public record GuardarReglaAsistencia([StringLength(100, MinimumLength = 2)] string NombreRegla, [Range(0, 240)] int MinutosTolerancia,
    [Range(0, 240)] int MinutosLimiteFalta, [Range(1, 50)] int MaximoRetardosPermitidos, bool AplicaSancionFalta,
    int IdPeriodoAcumulacion, int IdModoChecada, DateOnly FechaInicioVigencia, DateOnly? FechaFinVigencia);

public record MateriaDto(int IdMateria, string NombreMateria, bool Activo);

public record GuardarMateria([StringLength(150, MinimumLength = 2)] string NombreMateria);

public record PeriodoEscolarDto(int IdPeriodo, string NombrePeriodo, DateOnly? FechaInicio, DateOnly? FechaFin, bool Activo);

public record GuardarPeriodoEscolar([StringLength(100, MinimumLength = 2)] string NombrePeriodo, DateOnly? FechaInicio, DateOnly? FechaFin);

public class CatalogosAsistenciaService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj, IProcesosBd procesos)
{
    private const string ElFestivo = "el día festivo", ElChecador = "el checador", LaRegla = "la regla de asistencia",
        LaMateria = "la materia", ElPeriodo = "el periodo escolar";

    public async Task<List<DiaFestivoDto>> FestivosAsync(bool incluirInactivos, int? anio, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.DiasFestivos.AsNoTracking().Where(f => (incluirInactivos || f.Activo) && (anio == null || f.Fecha.Year == anio))
            .OrderBy(f => f.Fecha).Select(f => new DiaFestivoDto(f.IdDiaFestivo, f.Fecha, f.DescripcionFestividad, f.EsLaborable, f.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> CrearFestivoAsync(GuardarDiaFestivo datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        await ValidarFechaAbiertaAsync(datos.Fecha, ct);
        var festivo = new DiasFestivos { Fecha = datos.Fecha, DescripcionFestividad = datos.DescripcionFestividad.Trim(), EsLaborable = datos.EsLaborable, Activo = true };
        db.DiasFestivos.Add(festivo);
        await db.SaveChangesAsync(ct);
        await procesos.RecalcularDiasPasadosAsync(reloj, festivo.Fecha, festivo.Fecha, ct);
        return festivo.IdDiaFestivo;
    }

    public async Task ActualizarFestivoAsync(int id, GuardarDiaFestivo datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var festivo = await db.DiasFestivos.BuscarAsync(id, ElFestivo, ct);
        var fechaAnterior = festivo.Fecha;
        await ValidarFechaAbiertaAsync(fechaAnterior, ct);
        await ValidarFechaAbiertaAsync(datos.Fecha, ct);
        festivo.Fecha = datos.Fecha;
        festivo.DescripcionFestividad = datos.DescripcionFestividad.Trim();
        festivo.EsLaborable = datos.EsLaborable;
        await db.SaveChangesAsync(ct);
        await procesos.RecalcularDiasPasadosAsync(reloj, fechaAnterior, fechaAnterior, ct);
        await procesos.RecalcularDiasPasadosAsync(reloj, datos.Fecha, datos.Fecha, ct);
    }

    public async Task CambiarActivoFestivoAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var festivo = await db.DiasFestivos.BuscarAsync(id, ElFestivo, ct);
        await ValidarFechaAbiertaAsync(festivo.Fecha, ct);
        festivo.Activo = activo;
        await db.SaveChangesAsync(ct);
        await procesos.RecalcularDiasPasadosAsync(reloj, festivo.Fecha, festivo.Fecha, ct);
    }

    public async Task<List<ChecadorDto>> ChecadoresAsync(bool incluirInactivos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.Checadores.AsNoTracking().Where(c => incluirInactivos || c.Activo).OrderBy(c => c.NombreChecador)
            .Select(c => new ChecadorDto(c.IdChecador, c.NombreChecador, c.NumeroSerie, c.IpDispositivo, c.Ubicacion, c.Activo)).ToListAsync(ct);
    }

    public async Task<int> CrearChecadorAsync(GuardarChecador datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var checador = new Checadores { Activo = true };
        Copiar(datos, checador);
        db.Checadores.Add(checador);
        await db.SaveChangesAsync(ct);
        return checador.IdChecador;
    }

    public async Task ActualizarChecadorAsync(int id, GuardarChecador datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        Copiar(datos, await db.Checadores.BuscarAsync(id, ElChecador, ct));
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoChecadorAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return db.CambiarActivoAsync(db.Checadores, id, activo, ElChecador, null, ct);
    }

    public async Task<List<ReglaAsistenciaDto>> ReglasAsync(bool incluirInactivos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.ReglasAsistencia.AsNoTracking().Where(r => incluirInactivos || r.Activo).OrderBy(r => r.NombreRegla)
            .Select(r => new ReglaAsistenciaDto(r.IdReglaAsistencia, r.NombreRegla, r.MinutosTolerancia, r.MinutosLimiteFalta,
                r.MaximoRetardosPermitidos, r.AplicaSancionFalta, r.IdPeriodoAcumulacion, r.IdPeriodoAcumulacionNavigation.Descripcion,
                r.IdModoChecada, r.IdModoChecadaNavigation.Descripcion, r.FechaInicioVigencia, r.FechaFinVigencia, r.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> CrearReglaAsync(GuardarReglaAsistencia datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var regla = new ReglasAsistencia { Activo = true };
        Copiar(datos, regla);
        db.ReglasAsistencia.Add(regla);
        await db.SaveChangesAsync(ct);
        return regla.IdReglaAsistencia;
    }

    public async Task ActualizarReglaAsync(int id, GuardarReglaAsistencia datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        Copiar(datos, await db.ReglasAsistencia.BuscarAsync(id, LaRegla, ct));
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoReglaAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var hoy = reloj.Hoy;
        return db.CambiarActivoAsync(db.ReglasAsistencia, id, activo, LaRegla,
            () => db.HistorialHorarios.AnyAsync(h => h.IdReglaAsistencia == id && h.Activo && (h.FechaFinVigencia == null || h.FechaFinVigencia >= hoy), ct), ct);
    }

    public async Task<List<MateriaDto>> MateriasAsync(bool incluirInactivos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.Materias.AsNoTracking().Where(m => incluirInactivos || m.Activo).OrderBy(m => m.NombreMateria)
            .Select(m => new MateriaDto(m.IdMateria, m.NombreMateria, m.Activo)).ToListAsync(ct);
    }

    public async Task<int> CrearMateriaAsync(GuardarMateria datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var materia = new Materias { NombreMateria = datos.NombreMateria.Trim(), Activo = true };
        db.Materias.Add(materia);
        await db.SaveChangesAsync(ct);
        return materia.IdMateria;
    }

    public async Task ActualizarMateriaAsync(int id, GuardarMateria datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        (await db.Materias.BuscarAsync(id, LaMateria, ct)).NombreMateria = datos.NombreMateria.Trim();
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoMateriaAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var hoy = reloj.Hoy;
        return db.CambiarActivoAsync(db.Materias, id, activo, LaMateria,
            () => db.HistorialHorarios.AnyAsync(h => h.IdMateria == id && h.Activo && (h.FechaFinVigencia == null || h.FechaFinVigencia >= hoy), ct), ct);
    }

    public async Task<List<PeriodoEscolarDto>> PeriodosEscolaresAsync(bool incluirInactivos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.PeriodosEscolares.AsNoTracking().Where(p => incluirInactivos || p.Activo).OrderByDescending(p => p.FechaInicio)
            .Select(p => new PeriodoEscolarDto(p.IdPeriodo, p.NombrePeriodo, p.FechaInicio, p.FechaFin, p.Activo)).ToListAsync(ct);
    }

    public async Task<int> CrearPeriodoEscolarAsync(GuardarPeriodoEscolar datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var periodo = new PeriodosEscolares { Activo = true };
        Copiar(datos, periodo);
        db.PeriodosEscolares.Add(periodo);
        await db.SaveChangesAsync(ct);
        return periodo.IdPeriodo;
    }

    public async Task ActualizarPeriodoEscolarAsync(int id, GuardarPeriodoEscolar datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        Copiar(datos, await db.PeriodosEscolares.BuscarAsync(id, ElPeriodo, ct));
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoPeriodoEscolarAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var hoy = reloj.Hoy;
        return db.CambiarActivoAsync(db.PeriodosEscolares, id, activo, ElPeriodo,
            () => db.HistorialHorarios.AnyAsync(h => h.IdPeriodo == id && h.Activo && (h.FechaFinVigencia == null || h.FechaFinVigencia >= hoy), ct), ct);
    }

    private async Task ValidarFechaAbiertaAsync(DateOnly fecha, CancellationToken ct)
    {
        if (await db.HayNominaCerradaEnFechaAsync(fecha, ct)) throw Errores.Catalogos.FechaEnNominaCerrada();
    }

    private static void Copiar(GuardarChecador d, Checadores c)
    {
        if (d.IpDispositivo is not null && !IPAddress.TryParse(d.IpDispositivo, out _)) throw Errores.Catalogos.IpNoValida();
        c.NombreChecador = d.NombreChecador.Trim();
        c.NumeroSerie = d.NumeroSerie.Trim();
        c.IpDispositivo = d.IpDispositivo;
        c.Ubicacion = d.Ubicacion;
    }

    private static void Copiar(GuardarReglaAsistencia d, ReglasAsistencia r)
    {
        if (d.FechaFinVigencia < d.FechaInicioVigencia) throw Errores.Catalogos.FechasInvalidas();
        r.NombreRegla = d.NombreRegla.Trim();
        r.MinutosTolerancia = d.MinutosTolerancia;
        r.MinutosLimiteFalta = d.MinutosLimiteFalta;
        r.MaximoRetardosPermitidos = d.MaximoRetardosPermitidos;
        r.AplicaSancionFalta = d.AplicaSancionFalta;
        r.IdPeriodoAcumulacion = d.IdPeriodoAcumulacion;
        r.IdModoChecada = d.IdModoChecada;
        r.FechaInicioVigencia = d.FechaInicioVigencia;
        r.FechaFinVigencia = d.FechaFinVigencia;
    }

    private static void Copiar(GuardarPeriodoEscolar d, PeriodosEscolares p)
    {
        if (d.FechaFin < d.FechaInicio) throw Errores.Catalogos.FechasInvalidas();
        p.NombrePeriodo = d.NombrePeriodo.Trim();
        p.FechaInicio = d.FechaInicio;
        p.FechaFin = d.FechaFin;
    }
}
