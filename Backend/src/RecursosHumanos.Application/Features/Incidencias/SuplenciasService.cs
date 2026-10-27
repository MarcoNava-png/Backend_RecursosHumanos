using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Incidencias;

public class SuplenciasService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj, IProcesosBd procesos)
{
    private static readonly string[] Vigentes = [EstatusSuplencia.Pendiente, EstatusSuplencia.Aprobada];

    public async Task<Pagina<SuplenciaDto>> ListarAsync(FiltroIncidencias filtro, CancellationToken ct)
    {
        var visibles = db.EmpleadosVisibles(usuario, Permiso.SuplenciasVer).Select(e => e.IdEmpleado);
        var consulta = db.Suplencias.AsNoTracking()
            .Where(s => s.Activo && (visibles.Contains(s.IdEmpleadoTitular) || visibles.Contains(s.IdEmpleadoSuplente)));
        if (filtro.IdEmpleado is not null)
            consulta = consulta.Where(s => s.IdEmpleadoTitular == filtro.IdEmpleado || s.IdEmpleadoSuplente == filtro.IdEmpleado);
        if (filtro.Desde is not null) consulta = consulta.Where(s => s.FechaSuplencia >= filtro.Desde);
        if (filtro.Hasta is not null) consulta = consulta.Where(s => s.FechaSuplencia <= filtro.Hasta);
        if (filtro.Estatus is not null) consulta = consulta.Where(s => s.EstatusAprobacion == filtro.Estatus);

        var numeroPagina = Math.Max(filtro.NumeroPagina, 1);
        var elementos = await consulta.OrderByDescending(s => s.FechaSuplencia).ThenBy(s => s.IdHistorialHorarioNavigation.HoraEntrada)
            .Skip((numeroPagina - 1) * filtro.Tamano).Take(filtro.Tamano)
            .Select(s => new SuplenciaDto(s.IdSuplencia, s.FechaSuplencia, s.IdHistorialHorario,
                s.IdHistorialHorarioNavigation.HoraEntrada, s.IdHistorialHorarioNavigation.HoraSalida,
                s.IdHistorialHorarioNavigation.IdMateriaNavigation!.NombreMateria,
                s.IdEmpleadoTitular,
                (s.IdEmpleadoTitularNavigation.IdPersonaNavigation.Nombre + " " + s.IdEmpleadoTitularNavigation.IdPersonaNavigation.ApellidoPaterno).Trim(),
                s.IdEmpleadoSuplente,
                (s.IdEmpleadoSuplenteNavigation.IdPersonaNavigation.Nombre + " " + s.IdEmpleadoSuplenteNavigation.IdPersonaNavigation.ApellidoPaterno).Trim(),
                s.EstatusAprobacion))
            .ToListAsync(ct);
        return new Pagina<SuplenciaDto>(elementos, await consulta.CountAsync(ct), numeroPagina, filtro.Tamano);
    }

    public async Task<int> CrearAsync(NuevaSuplencia nueva, CancellationToken ct)
    {
        var horario = await db.HistorialHorarios.Include(h => h.IdEmpleadoNavigation)
                          .SingleOrDefaultAsync(h => h.IdHistorialHorario == nueva.IdHistorialHorario && h.Activo, ct)
                      ?? throw Errores.Horarios.NoEncontrado(nueva.IdHistorialHorario);
        if (!usuario.TienePermiso(Permiso.SuplenciasAsignar) && horario.IdEmpleadoNavigation.IdJefeDirecto != usuario.IdEmpleado)
            throw Errores.General.SinPermiso();
        var fecha = nueva.FechaSuplencia;
        if (horario.DiaSemana != ReglasFechas.DiaSemana(fecha) || horario.FechaInicioVigencia > fecha
            || (horario.FechaFinVigencia != null && horario.FechaFinVigencia < fecha))
            throw Errores.Incidencias.HorarioNoAplica();
        if (nueva.IdEmpleadoSuplente == horario.IdEmpleado
            || !await db.Empleados.AnyAsync(e => e.IdEmpleado == nueva.IdEmpleadoSuplente && e.Activo, ct))
            throw Errores.Incidencias.SuplenteNoValido();
        await ValidarSuplenteLibreAsync(nueva.IdEmpleadoSuplente, horario, fecha, ct);
        await ValidarNominaAbiertaAsync(horario.IdEmpleado, nueva.IdEmpleadoSuplente, fecha, ct);

        var suplencia = await db.Suplencias.SingleOrDefaultAsync(s => s.IdHistorialHorario == horario.IdHistorialHorario
                                                                     && s.FechaSuplencia == fecha, ct);
        if (suplencia is not null && suplencia.Activo && Vigentes.Contains(suplencia.EstatusAprobacion))
            throw Errores.Incidencias.ClaseYaTieneSuplente();
        if (suplencia is null)
        {
            suplencia = new Suplencias { IdHistorialHorario = horario.IdHistorialHorario, FechaSuplencia = fecha };
            db.Suplencias.Add(suplencia);
        }
        suplencia.IdEmpleadoTitular = horario.IdEmpleado;
        suplencia.IdEmpleadoSuplente = nueva.IdEmpleadoSuplente;
        suplencia.EstatusAprobacion = EstatusSuplencia.Pendiente;
        suplencia.IdAutorizador = null;
        suplencia.Activo = true;
        await db.SaveChangesAsync(ct);
        return suplencia.IdSuplencia;
    }

    public async Task AprobarAsync(int id, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.SuplenciasAprobar);
        var suplencia = await CargarAsync(id, ct);
        if (suplencia.EstatusAprobacion != EstatusSuplencia.Pendiente) throw Errores.Incidencias.EstatusNoPermite("aprobar");
        await ValidarNominaAbiertaAsync(suplencia.IdEmpleadoTitular, suplencia.IdEmpleadoSuplente, suplencia.FechaSuplencia, ct);

        suplencia.EstatusAprobacion = EstatusSuplencia.Aprobada;
        suplencia.IdAutorizador = usuario.IdEmpleado;
        await db.SaveChangesAsync(ct);
        await procesos.RecalcularDiasPasadosAsync(reloj, suplencia.FechaSuplencia, suplencia.FechaSuplencia, ct);
    }

    public async Task RechazarAsync(int id, CancellationToken ct)
    {
        var suplencia = await CargarAsync(id, ct);
        ValidarPuedeGestionar(suplencia);
        if (suplencia.EstatusAprobacion != EstatusSuplencia.Pendiente) throw Errores.Incidencias.EstatusNoPermite("rechazar");
        suplencia.EstatusAprobacion = EstatusSuplencia.Rechazada;
        await db.SaveChangesAsync(ct);
    }

    public async Task CancelarAsync(int id, CancellationToken ct)
    {
        var suplencia = await CargarAsync(id, ct);
        ValidarPuedeGestionar(suplencia);
        if (!Vigentes.Contains(suplencia.EstatusAprobacion)) throw Errores.Incidencias.EstatusNoPermite("cancelar");
        var estabaAprobada = suplencia.EstatusAprobacion == EstatusSuplencia.Aprobada;
        if (estabaAprobada)
            await ValidarNominaAbiertaAsync(suplencia.IdEmpleadoTitular, suplencia.IdEmpleadoSuplente, suplencia.FechaSuplencia, ct);

        suplencia.EstatusAprobacion = EstatusSuplencia.Cancelada;
        await db.SaveChangesAsync(ct);
        if (estabaAprobada) await procesos.RecalcularDiasPasadosAsync(reloj, suplencia.FechaSuplencia, suplencia.FechaSuplencia, ct);
    }

    private async Task<Suplencias> CargarAsync(int id, CancellationToken ct) =>
        await db.Suplencias.Include(s => s.IdEmpleadoTitularNavigation)
            .SingleOrDefaultAsync(s => s.IdSuplencia == id && s.Activo, ct)
        ?? throw Errores.Incidencias.SuplenciaNoEncontrada(id);

    private void ValidarPuedeGestionar(Suplencias s)
    {
        if (!usuario.TienePermiso(Permiso.SuplenciasAsignar) && s.IdEmpleadoTitularNavigation.IdJefeDirecto != usuario.IdEmpleado)
            throw Errores.General.SinPermiso();
    }

    private async Task ValidarSuplenteLibreAsync(int idSuplente, HistorialHorarios clase, DateOnly fecha, CancellationToken ct)
    {
        var diaSemana = (short)ReglasFechas.DiaSemana(fecha);
        var tieneClase = await db.HistorialHorarios.AnyAsync(h => h.IdEmpleado == idSuplente && h.Activo && h.DiaSemana == diaSemana
            && h.FechaInicioVigencia <= fecha && (h.FechaFinVigencia == null || h.FechaFinVigencia >= fecha)
            && h.HoraEntrada < clase.HoraSalida && h.HoraSalida > clase.HoraEntrada, ct);
        var cubreOtra = await db.Suplencias.AnyAsync(s => s.IdEmpleadoSuplente == idSuplente && s.Activo && s.FechaSuplencia == fecha
            && Vigentes.Contains(s.EstatusAprobacion) && s.IdHistorialHorario != clase.IdHistorialHorario
            && s.IdHistorialHorarioNavigation.HoraEntrada < clase.HoraSalida && s.IdHistorialHorarioNavigation.HoraSalida > clase.HoraEntrada, ct);
        if (tieneClase || cubreOtra) throw Errores.Incidencias.SuplenteOcupado();
    }

    private async Task ValidarNominaAbiertaAsync(int idTitular, int idSuplente, DateOnly fecha, CancellationToken ct)
    {
        if (await db.HayNominaCerradaAsync(idTitular, fecha, fecha, ct) || await db.HayNominaCerradaAsync(idSuplente, fecha, fecha, ct))
            throw Errores.Incidencias.NominaCerrada();
    }
}
