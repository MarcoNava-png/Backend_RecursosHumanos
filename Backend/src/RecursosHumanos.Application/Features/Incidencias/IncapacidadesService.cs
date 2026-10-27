using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Incidencias;

public class IncapacidadesService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj, IProcesosBd procesos)
{
    public async Task<Pagina<IncapacidadDto>> ListarAsync(FiltroIncidencias filtro, CancellationToken ct)
    {
        var visibles = db.EmpleadosVisibles(usuario, Permiso.IncapacidadesVer).Select(e => e.IdEmpleado);
        var consulta = db.IncapacidadesImss.AsNoTracking().Where(i => i.Activo && visibles.Contains(i.IdEmpleado));
        if (filtro.IdEmpleado is not null) consulta = consulta.Where(i => i.IdEmpleado == filtro.IdEmpleado);
        if (filtro.Desde is not null) consulta = consulta.Where(i => i.FechaFin >= filtro.Desde);
        if (filtro.Hasta is not null) consulta = consulta.Where(i => i.FechaInicio <= filtro.Hasta);

        var numeroPagina = Math.Max(filtro.NumeroPagina, 1);
        var elementos = await consulta.OrderByDescending(i => i.FechaInicio)
            .Skip((numeroPagina - 1) * filtro.Tamano).Take(filtro.Tamano)
            .Select(i => new IncapacidadDto(i.IdIncapacidad, i.IdEmpleado,
                (i.IdEmpleadoNavigation.IdPersonaNavigation.Nombre + " " + i.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno + " " +
                 (i.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoMaterno ?? "")).Trim(),
                i.FolioImss, i.IdTipoIncapacidad, i.IdTipoIncapacidadNavigation.Descripcion, i.FechaInicio, i.FechaFin,
                i.DiasAutorizados))
            .ToListAsync(ct);
        return new Pagina<IncapacidadDto>(elementos, await consulta.CountAsync(ct), numeroPagina, filtro.Tamano);
    }

    public async Task<int> CrearAsync(NuevaIncapacidad nueva, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.IncapacidadesEditar);
        if (!await db.Empleados.AnyAsync(e => e.IdEmpleado == nueva.IdEmpleado && e.Activo, ct))
            throw Errores.Empleados.NoEncontrado(nueva.IdEmpleado);
        if (nueva.FechaFin < nueva.FechaInicio) throw Errores.Incidencias.FechasInvalidas();
        if (await db.HayNominaCerradaAsync(nueva.IdEmpleado, nueva.FechaInicio, nueva.FechaFin, ct))
            throw Errores.Incidencias.NominaCerrada();
        if (await db.IncapacidadesImss.AnyAsync(i => i.IdEmpleado == nueva.IdEmpleado && i.Activo
                && i.FechaInicio <= nueva.FechaFin && i.FechaFin >= nueva.FechaInicio, ct))
            throw Errores.Incidencias.IncapacidadSeTraslapa();

        var incapacidad = new IncapacidadesImss
        {
            IdEmpleado = nueva.IdEmpleado, FolioImss = nueva.FolioImss.Trim().ToUpper(), IdTipoIncapacidad = nueva.IdTipoIncapacidad,
            FechaInicio = nueva.FechaInicio, FechaFin = nueva.FechaFin,
            DiasAutorizados = nueva.FechaFin.DayNumber - nueva.FechaInicio.DayNumber + 1
        };
        db.IncapacidadesImss.Add(incapacidad);
        await db.SaveChangesAsync(ct);
        await procesos.RecalcularDiasPasadosAsync(reloj, incapacidad.FechaInicio, incapacidad.FechaFin, ct);
        return incapacidad.IdIncapacidad;
    }

    public async Task CancelarAsync(int id, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.IncapacidadesEditar);
        var incapacidad = await db.IncapacidadesImss.SingleOrDefaultAsync(i => i.IdIncapacidad == id && i.Activo, ct)
                          ?? throw Errores.Incidencias.IncapacidadNoEncontrada(id);
        if (await db.HayNominaCerradaAsync(incapacidad.IdEmpleado, incapacidad.FechaInicio, incapacidad.FechaFin, ct))
            throw Errores.Incidencias.NominaCerrada();

        incapacidad.Activo = false;
        await db.SaveChangesAsync(ct);
        await procesos.RecalcularDiasPasadosAsync(reloj, incapacidad.FechaInicio, incapacidad.FechaFin, ct);
    }
}
