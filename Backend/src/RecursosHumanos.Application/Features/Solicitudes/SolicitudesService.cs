using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Application.Features.Vacaciones;
using RecursosHumanos.Domain.Constantes;
using Solicitud = RecursosHumanos.Domain.Entities.Solicitudes;

namespace RecursosHumanos.Application.Features.Solicitudes;

public class SolicitudesService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj, IProcesosBd procesos,
    IAlmacenArchivos almacen, VacacionesService vacaciones)
{
    private const int MaximoDias = 90;
    private static readonly string[] Activas = [EstatusSolicitud.Pendiente, EstatusSolicitud.AprobadaJefe, EstatusSolicitud.Aprobada];
    private static readonly Dictionary<string, string> TiposArchivo = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png"
    };

    public Task<Pagina<SolicitudDto>> ListarAsync(FiltroSolicitudes filtro, CancellationToken ct)
    {
        var visibles = db.EmpleadosVisibles(usuario, Permiso.SolicitudesVer).Select(e => e.IdEmpleado);
        var consulta = db.Solicitudes.AsNoTracking().Where(s => s.Activo && visibles.Contains(s.IdEmpleado));
        if (filtro.IdEmpleado is not null) consulta = consulta.Where(s => s.IdEmpleado == filtro.IdEmpleado);
        if (filtro.Estatus is not null) consulta = consulta.Where(s => s.EstatusAprobacion == filtro.Estatus);
        if (filtro.Desde is not null) consulta = consulta.Where(s => s.FechaFin >= filtro.Desde);
        if (filtro.Hasta is not null) consulta = consulta.Where(s => s.FechaInicio <= filtro.Hasta);
        return PaginarAsync(consulta, filtro.NumeroPagina, filtro.Tamano, ct);
    }

    public Task<Pagina<SolicitudDto>> PorAprobarAsync(int numeroPagina, int tamano, CancellationToken ct)
    {
        var idEmpleado = usuario.IdEmpleado;
        var aprobadorFinal = usuario.TienePermiso(Permiso.SolicitudesAprobar);
        var consulta = db.Solicitudes.AsNoTracking().Where(s => s.Activo && s.IdEmpleado != idEmpleado
            && ((s.EstatusAprobacion == EstatusSolicitud.Pendiente && s.IdEmpleadoNavigation.IdJefeDirecto == idEmpleado)
                || (aprobadorFinal && (s.EstatusAprobacion == EstatusSolicitud.AprobadaJefe
                             || (s.EstatusAprobacion == EstatusSolicitud.Pendiente && s.IdEmpleadoNavigation.IdJefeDirecto == null)))));
        return PaginarAsync(consulta, numeroPagina, tamano, ct);
    }

    public async Task<SolicitudDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var visibles = db.EmpleadosVisibles(usuario, Permiso.SolicitudesVer).Select(e => e.IdEmpleado);
        return await db.Solicitudes.AsNoTracking()
                   .Where(s => s.IdSolicitud == id && s.Activo && visibles.Contains(s.IdEmpleado))
                   .Select(Proyeccion).SingleOrDefaultAsync(ct)
               ?? throw Errores.Solicitudes.NoEncontrada(id);
    }

    public async Task<int> CrearAsync(NuevaSolicitud nueva, CancellationToken ct)
    {
        var idEmpleado = nueva.IdEmpleado ?? usuario.IdEmpleado ?? throw Errores.General.SinPermiso();
        if (idEmpleado != usuario.IdEmpleado && !usuario.TienePermiso(Permiso.SolicitudesCrearOtros)) throw Errores.General.SinPermiso();
        if (!await db.Empleados.AnyAsync(e => e.IdEmpleado == idEmpleado && e.Activo, ct)) throw Errores.Empleados.NoEncontrado(idEmpleado);
        var tipo = await db.TiposPermiso.Where(t => t.IdTipoPermiso == nueva.IdTipoPermiso && t.Activo)
                       .Select(t => new
                       {
                           t.Descripcion,
                           t.DiasPorAnio,
                           PorHoras = t.IdTipoDuracionPermisoNavigation.Clave == TipoDuracionPermiso.PorHoras,
                           EsVacaciones = t.IdEstatusAsistenciaNavigation.Clave == EstatusAsistencia.Vacaciones
                       })
                       .SingleOrDefaultAsync(ct)
                   ?? throw Errores.Solicitudes.TipoNoValido();
        if (nueva.FechaFin < nueva.FechaInicio) throw Errores.Solicitudes.FechasInvalidas();
        if (nueva.FechaFin.DayNumber - nueva.FechaInicio.DayNumber + 1 > MaximoDias) throw Errores.Solicitudes.RangoMuyGrande(MaximoDias);
        if (await db.HayNominaCerradaAsync(idEmpleado, nueva.FechaInicio, nueva.FechaFin, ct)) throw Errores.Solicitudes.NominaCerrada();

        var solicitud = new Solicitud
        {
            IdEmpleado = idEmpleado, IdTipoPermiso = nueva.IdTipoPermiso, FechaInicio = nueva.FechaInicio,
            FechaFin = nueva.FechaFin, EstatusAprobacion = EstatusSolicitud.Pendiente
        };
        if (tipo.PorHoras)
        {
            if (nueva.HoraInicio is not { } inicio || nueva.HoraFin is not { } fin || fin <= inicio || nueva.FechaFin != nueva.FechaInicio)
                throw Errores.Solicitudes.HorasInvalidas();
            solicitud.HoraInicioPermiso = inicio;
            solicitud.HoraFinPermiso = fin;
            solicitud.HorasSolicitadas = Math.Round((decimal)(fin - inicio).TotalHours, 2);
        }
        else
        {
            var dias = await DiasLaborablesAsync(idEmpleado, nueva.FechaInicio, nueva.FechaFin, ct);
            if (dias == 0) throw Errores.Solicitudes.SinDiasLaborables();
            if (tipo.EsVacaciones) await vacaciones.ValidarDisponiblesAsync(idEmpleado, dias, ct);
            else if (tipo.DiasPorAnio is int tope)
            {
                var anio = nueva.FechaInicio.Year;
                var usados = await DiasUsadosAsync(idEmpleado, nueva.IdTipoPermiso, anio, ct);
                if (usados + dias > tope) throw Errores.Solicitudes.TopeAnual(tipo.Descripcion, tope, usados, anio);
            }
            solicitud.DiasSolicitados = dias;
        }
        if (await SeTraslapaAsync(solicitud, ct)) throw Errores.Solicitudes.Traslape();

        db.Solicitudes.Add(solicitud);
        await db.SaveChangesAsync(ct);
        return solicitud.IdSolicitud;
    }

    public async Task AprobarAsync(int id, CancellationToken ct)
    {
        var solicitud = await CargarAsync(id, ct);
        if (solicitud.IdEmpleado == usuario.IdEmpleado) throw Errores.Solicitudes.PropiaSolicitud();

        if (solicitud.EstatusAprobacion == EstatusSolicitud.Pendiente && EsJefeDe(solicitud))
        {
            solicitud.EstatusAprobacion = EstatusSolicitud.AprobadaJefe;
            solicitud.FechaAprobacionJefe = reloj.AhoraUtc;
            solicitud.IdAutorizadorJefe = usuario.IdEmpleado;
            await db.SaveChangesAsync(ct);
            return;
        }
        if (!PuedeResolverFinal(solicitud)) throw NoPuede(solicitud, "aprobar");
        await ValidarNominaAbiertaAsync(solicitud, ct);
        if (solicitud.IdTipoPermisoNavigation.RequiereComprobante && solicitud.RutaComprobanteAdjunto is null)
            throw Errores.Solicitudes.FaltaComprobante();
        if (EsVacaciones(solicitud)) await vacaciones.DescontarAsync(solicitud.IdEmpleado, (int)solicitud.DiasSolicitados!.Value, ct);

        solicitud.EstatusAprobacion = EstatusSolicitud.Aprobada;
        solicitud.FechaAprobacionRh = reloj.AhoraUtc;
        solicitud.IdAutorizadorRh = usuario.IdEmpleado;
        await db.SaveChangesAsync(ct);
        await procesos.RecalcularDiasPasadosAsync(reloj, solicitud.FechaInicio, solicitud.FechaFin, ct);
    }

    public async Task RechazarAsync(int id, RechazoSolicitud rechazo, CancellationToken ct)
    {
        var solicitud = await CargarAsync(id, ct);
        if (solicitud.IdEmpleado == usuario.IdEmpleado) throw Errores.Solicitudes.PropiaSolicitud();
        var puede = (solicitud.EstatusAprobacion == EstatusSolicitud.Pendiente && EsJefeDe(solicitud)) || PuedeResolverFinal(solicitud);
        if (!puede) throw NoPuede(solicitud, "rechazar");

        solicitud.EstatusAprobacion = EstatusSolicitud.Rechazada;
        solicitud.MotivoRechazo = rechazo.Motivo.Trim();
        await db.SaveChangesAsync(ct);
    }

    public async Task CancelarAsync(int id, CancellationToken ct)
    {
        var solicitud = await CargarAsync(id, ct);
        var aprobadorFinal = usuario.TienePermiso(Permiso.SolicitudesAprobar);
        if (!aprobadorFinal && solicitud.IdEmpleado != usuario.IdEmpleado) throw Errores.General.SinPermiso();
        var estabaAprobada = solicitud.EstatusAprobacion == EstatusSolicitud.Aprobada;
        if (!Activas.Contains(solicitud.EstatusAprobacion) || (estabaAprobada && !aprobadorFinal && solicitud.FechaInicio <= reloj.Hoy))
            throw Errores.Solicitudes.EstatusNoPermite("cancelar");

        if (estabaAprobada) await ValidarNominaAbiertaAsync(solicitud, ct);
        if (estabaAprobada && EsVacaciones(solicitud))
            await vacaciones.DevolverAsync(solicitud.IdEmpleado, (int)solicitud.DiasSolicitados!.Value, ct);
        solicitud.EstatusAprobacion = EstatusSolicitud.Cancelada;
        await db.SaveChangesAsync(ct);
        if (estabaAprobada) await procesos.RecalcularDiasPasadosAsync(reloj, solicitud.FechaInicio, solicitud.FechaFin, ct);
    }

    public async Task AdjuntarComprobanteAsync(int id, Stream contenido, string nombreArchivo, long tamano, CancellationToken ct)
    {
        var solicitud = await CargarAsync(id, ct);
        if (solicitud.IdEmpleado != usuario.IdEmpleado && !usuario.TienePermiso(Permiso.SolicitudesCrearOtros)) throw Errores.General.SinPermiso();
        if (solicitud.EstatusAprobacion is not (EstatusSolicitud.Pendiente or EstatusSolicitud.AprobadaJefe))
            throw Errores.Solicitudes.EstatusNoPermite("modificar");
        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        if (!TiposArchivo.ContainsKey(extension) || tamano == 0 || tamano > almacen.MaximoMb * 1024L * 1024)
            throw Errores.Solicitudes.ArchivoNoValido(almacen.MaximoMb);

        solicitud.RutaComprobanteAdjunto = await almacen.GuardarAsync(contenido, extension, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<ArchivoDto> ComprobanteAsync(int id, CancellationToken ct)
    {
        var visibles = db.EmpleadosVisibles(usuario, Permiso.SolicitudesVer).Select(e => e.IdEmpleado);
        var solicitud = await db.Solicitudes.AsNoTracking()
                            .Where(s => s.IdSolicitud == id && s.Activo && visibles.Contains(s.IdEmpleado))
                            .Select(s => new { s.RutaComprobanteAdjunto }).SingleOrDefaultAsync(ct)
                        ?? throw Errores.Solicitudes.NoEncontrada(id);
        var ruta = solicitud.RutaComprobanteAdjunto ?? throw Errores.Solicitudes.SinComprobante();
        var extension = Path.GetExtension(ruta);
        return new ArchivoDto(almacen.Abrir(ruta), TiposArchivo[extension], $"comprobante-{id}{extension}");
    }

    private async Task<Solicitud> CargarAsync(int id, CancellationToken ct)
    {
        var visibles = db.EmpleadosVisibles(usuario, Permiso.SolicitudesVer).Select(e => e.IdEmpleado);
        return await db.Solicitudes
                   .Include(s => s.IdEmpleadoNavigation)
                   .Include(s => s.IdTipoPermisoNavigation).ThenInclude(t => t.IdEstatusAsistenciaNavigation)
                   .SingleOrDefaultAsync(s => s.IdSolicitud == id && s.Activo && visibles.Contains(s.IdEmpleado), ct)
               ?? throw Errores.Solicitudes.NoEncontrada(id);
    }

    private async Task ValidarNominaAbiertaAsync(Solicitud s, CancellationToken ct)
    {
        if (await db.HayNominaCerradaAsync(s.IdEmpleado, s.FechaInicio, s.FechaFin, ct)) throw Errores.Solicitudes.NominaCerrada();
    }

    private bool EsJefeDe(Solicitud s) => usuario.IdEmpleado is not null && s.IdEmpleadoNavigation.IdJefeDirecto == usuario.IdEmpleado;

    private bool PuedeResolverFinal(Solicitud s) =>
        usuario.TienePermiso(Permiso.SolicitudesAprobar)
        && (s.EstatusAprobacion == EstatusSolicitud.AprobadaJefe
            || (s.EstatusAprobacion == EstatusSolicitud.Pendiente && s.IdEmpleadoNavigation.IdJefeDirecto is null));

    private ErrorAplicacionException NoPuede(Solicitud s, string accion) =>
        EsJefeDe(s) || usuario.TienePermiso(Permiso.SolicitudesAprobar)
            ? Errores.Solicitudes.EstatusNoPermite(accion)
            : Errores.General.SinPermiso();

    private static bool EsVacaciones(Solicitud s) => s.IdTipoPermisoNavigation.IdEstatusAsistenciaNavigation.Clave == EstatusAsistencia.Vacaciones;

    private async Task<int> DiasLaborablesAsync(int idEmpleado, DateOnly inicio, DateOnly fin, CancellationToken ct)
    {
        var horarios = await db.HistorialHorarios
            .Where(h => h.IdEmpleado == idEmpleado && h.Activo && h.FechaInicioVigencia <= fin
                        && (h.FechaFinVigencia == null || h.FechaFinVigencia >= inicio))
            .Select(h => new { h.DiaSemana, h.FechaInicioVigencia, h.FechaFinVigencia })
            .ToListAsync(ct);
        var festivos = (await db.DiasFestivos
            .Where(f => f.Activo && !f.EsLaborable && f.Fecha >= inicio && f.Fecha <= fin)
            .Select(f => f.Fecha).ToListAsync(ct)).ToHashSet();

        var dias = 0;
        for (var dia = inicio; dia <= fin; dia = dia.AddDays(1))
        {
            var diaSemana = ReglasFechas.DiaSemana(dia);
            if (!festivos.Contains(dia) && horarios.Any(h => h.DiaSemana == diaSemana && h.FechaInicioVigencia <= dia
                                                             && (h.FechaFinVigencia == null || h.FechaFinVigencia >= dia)))
                dias++;
        }
        return dias;
    }

    public async Task<List<TopeAnualDto>> TopesAsync(int idEmpleado, int anio, CancellationToken ct)
    {
        if (!await db.EmpleadosVisibles(usuario, Permiso.SolicitudesVer).AnyAsync(e => e.IdEmpleado == idEmpleado, ct))
            throw Errores.Empleados.NoEncontrado(idEmpleado);
        var tipos = await db.TiposPermiso.AsNoTracking().Where(t => t.Activo && t.DiasPorAnio != null)
            .OrderBy(t => t.IdTipoPermiso).Select(t => new { t.IdTipoPermiso, t.Descripcion, Tope = t.DiasPorAnio!.Value })
            .ToListAsync(ct);
        var topes = new List<TopeAnualDto>();
        foreach (var t in tipos)
            topes.Add(new TopeAnualDto(t.IdTipoPermiso, t.Descripcion, t.Tope, await DiasUsadosAsync(idEmpleado, t.IdTipoPermiso, anio, ct)));
        return topes;
    }

    private Task<int> DiasUsadosAsync(int idEmpleado, int idTipoPermiso, int anio, CancellationToken ct) =>
        db.Solicitudes
            .Where(s => s.IdEmpleado == idEmpleado && s.IdTipoPermiso == idTipoPermiso && s.Activo
                        && Activas.Contains(s.EstatusAprobacion) && s.FechaInicio.Year == anio)
            .SumAsync(s => (int)(s.DiasSolicitados ?? 0), ct);

    private Task<bool> SeTraslapaAsync(Solicitud nueva, CancellationToken ct) =>
        db.Solicitudes.AnyAsync(s => s.IdEmpleado == nueva.IdEmpleado && s.Activo && Activas.Contains(s.EstatusAprobacion)
                                     && s.FechaInicio <= nueva.FechaFin && s.FechaFin >= nueva.FechaInicio
                                     && (s.HoraInicioPermiso == null || nueva.HoraInicioPermiso == null
                                         || (s.HoraInicioPermiso < nueva.HoraFinPermiso && s.HoraFinPermiso > nueva.HoraInicioPermiso)), ct);

    private static async Task<Pagina<SolicitudDto>> PaginarAsync(IQueryable<Solicitud> consulta, int numeroPagina, int tamano, CancellationToken ct)
    {
        numeroPagina = Math.Max(numeroPagina, 1);
        var elementos = await consulta.OrderByDescending(s => s.FechaInicio).ThenByDescending(s => s.IdSolicitud)
            .Skip((numeroPagina - 1) * tamano).Take(tamano).Select(Proyeccion).ToListAsync(ct);
        return new Pagina<SolicitudDto>(elementos, await consulta.CountAsync(ct), numeroPagina, tamano);
    }

    private static readonly Expression<Func<Solicitud, SolicitudDto>> Proyeccion = s => new SolicitudDto(
        s.IdSolicitud, s.IdEmpleado,
        (s.IdEmpleadoNavigation.IdPersonaNavigation.Nombre + " " + s.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno + " " +
         (s.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoMaterno ?? "")).Trim(),
        s.IdTipoPermiso, s.IdTipoPermisoNavigation.Descripcion, s.FechaInicio, s.FechaFin, s.HoraInicioPermiso,
        s.HoraFinPermiso, s.DiasSolicitados, s.HorasSolicitadas, s.EstatusAprobacion, s.MotivoRechazo,
        s.FechaAprobacionJefe, s.FechaAprobacionRh, s.IdTipoPermisoNavigation.RequiereComprobante,
        s.RutaComprobanteAdjunto != null, s.FechaCreacion);
}
