using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Asistencia;

public class AsistenciaService(IRecursosHumanosDbContext db, IProcesosBd procesos, IUsuarioActual usuario, IReloj reloj)
{
    private const int MaximoDiasRango = 62;

    public async Task<ResultadoLoteDto> RegistrarChecadasAsync(LoteChecadasSolicitud lote, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.AsistenciaRegistrarChecadas);
        var idOrigen = await db.OrigenesChecada
                           .Where(o => o.Clave == lote.Origen && o.RequiereChecador && o.Activo)
                           .Select(o => (int?)o.IdOrigenChecada).SingleOrDefaultAsync(ct)
                       ?? throw Errores.Asistencia.OrigenNoValido(lote.Origen);

        var series = lote.Checadas.Select(c => c.NumeroSerieChecador).Distinct().ToList();
        var checadores = await db.Checadores.Where(c => c.Activo && series.Contains(c.NumeroSerie))
            .ToDictionaryAsync(c => c.NumeroSerie, c => c.IdChecador, ct);
        var numeros = lote.Checadas.Select(c => c.NumeroChecador).Distinct().ToList();
        var empleados = await db.Empleados.Where(e => e.Activo && numeros.Contains(e.NumeroChecador!.Value))
            .ToDictionaryAsync(e => e.NumeroChecador!.Value, e => e.IdEmpleado, ct);
        var registradas = await EventosRegistradosAsync(lote, checadores.Values.ToList(), ct);

        var rechazadas = new List<ChecadaRechazadaDto>();
        var nuevas = new List<Asistencias>();
        foreach (var c in lote.Checadas)
        {
            if (!checadores.TryGetValue(c.NumeroSerieChecador, out var idChecador))
                rechazadas.Add(new(c.NumeroSerieChecador, c.NumeroEvento, "El checador no está registrado o está inactivo."));
            else if (!empleados.TryGetValue(c.NumeroChecador, out var idEmpleado))
                rechazadas.Add(new(c.NumeroSerieChecador, c.NumeroEvento, $"Ningún empleado activo tiene el número de checador {c.NumeroChecador}."));
            else if (registradas.Add((idChecador, c.NumeroEvento)))
                nuevas.Add(new Asistencias
                {
                    IdEmpleado = idEmpleado, IdChecador = idChecador, IdOrigenChecada = idOrigen,
                    NumeroEventoHikvision = c.NumeroEvento, TipoRegistro = TipoRegistro.Checada,
                    FechaHoraRegistro = c.FechaHora.DateTime
                });
        }
        db.Asistencias.AddRange(nuevas);
        await db.SaveChangesAsync(ct);

        return new ResultadoLoteDto(lote.Checadas.Count, nuevas.Count, lote.Checadas.Count - nuevas.Count - rechazadas.Count,
            rechazadas, await RecalcularDiasPasadosAsync(nuevas, ct));
    }

    public async Task<ProcesarResultadoDto> ProcesarAsync(ProcesarSolicitud solicitud, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.AsistenciaProcesar);
        ValidarRango(solicitud.FechaInicio, solicitud.FechaFin);
        return new ProcesarResultadoDto(await procesos.GenerarAsistenciaAsync(solicitud.FechaInicio, solicitud.FechaFin, ct));
    }

    public async Task<Pagina<AsistenciaDiariaDto>> ListarDiariaAsync(FiltroAsistencia filtro, CancellationToken ct)
    {
        ValidarRango(filtro.FechaInicio, filtro.FechaFin);
        var visibles = db.EmpleadosVisibles(usuario, Permiso.AsistenciaVer).Select(e => e.IdEmpleado);
        var consulta = db.AsistenciaDiaria.AsNoTracking()
            .Where(d => d.Activo && visibles.Contains(d.IdEmpleado)
                        && d.FechaDia >= filtro.FechaInicio && d.FechaDia <= filtro.FechaFin
                        && (filtro.IdEmpleado == null || d.IdEmpleado == filtro.IdEmpleado));

        var numeroPagina = Math.Max(filtro.NumeroPagina, 1);
        var elementos = await consulta
            .OrderBy(d => d.FechaDia).ThenBy(d => d.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno)
            .ThenBy(d => d.IdHistorialHorarioNavigation.HoraEntrada)
            .Skip((numeroPagina - 1) * filtro.Tamano).Take(filtro.Tamano)
            .Select(d => new AsistenciaDiariaDto(
                d.IdAsistenciaDiaria, d.IdEmpleado,
                (d.IdEmpleadoNavigation.IdPersonaNavigation.Nombre + " " + d.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno + " " +
                 (d.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoMaterno ?? "")).Trim(),
                d.FechaDia, d.IdHistorialHorarioNavigation.HoraEntrada, d.IdHistorialHorarioNavigation.HoraSalida,
                d.IdHistorialHorarioNavigation.IdMateriaNavigation!.NombreMateria,
                d.IdEstatusAsistenciaNavigation.Clave, d.IdEstatusAsistenciaNavigation.Descripcion,
                d.IdEstatusAsistenciaNavigation.ColorHex, d.MinutosRetardo, d.HorasCubiertas, d.IdSolicitud))
            .ToListAsync(ct);
        return new Pagina<AsistenciaDiariaDto>(elementos, await consulta.CountAsync(ct), numeroPagina, filtro.Tamano);
    }

    public async Task<List<ChecadaDto>> ListarChecadasAsync(int idEmpleado, DateOnly fecha, CancellationToken ct)
    {
        if (!await db.EmpleadosVisibles(usuario, Permiso.AsistenciaVer).AnyAsync(e => e.IdEmpleado == idEmpleado, ct))
            throw Errores.Empleados.NoEncontrado(idEmpleado);
        var inicio = fecha.ToDateTime(TimeOnly.MinValue);
        return await db.Asistencias.AsNoTracking()
            .Where(a => a.IdEmpleado == idEmpleado && a.Activo
                        && a.FechaHoraRegistro >= inicio && a.FechaHoraRegistro < inicio.AddDays(1))
            .OrderBy(a => a.FechaHoraRegistro)
            .Select(a => new ChecadaDto(a.IdAsistencia, a.FechaHoraRegistro, a.IdChecadorNavigation!.NombreChecador,
                a.IdOrigenChecadaNavigation.Descripcion))
            .ToListAsync(ct);
    }

    private async Task<HashSet<(int, long)>> EventosRegistradosAsync(LoteChecadasSolicitud lote, List<int> idsChecador, CancellationToken ct)
    {
        var eventos = lote.Checadas.Select(c => c.NumeroEvento).Distinct().ToList();
        var existentes = await db.Asistencias
            .Where(a => idsChecador.Contains(a.IdChecador!.Value) && eventos.Contains(a.NumeroEventoHikvision!.Value))
            .Select(a => new { IdChecador = a.IdChecador!.Value, NumeroEvento = a.NumeroEventoHikvision!.Value })
            .ToListAsync(ct);
        return existentes.Select(a => (a.IdChecador, a.NumeroEvento)).ToHashSet();
    }

    private async Task<int> RecalcularDiasPasadosAsync(List<Asistencias> nuevas, CancellationToken ct)
    {
        var hoy = reloj.Hoy;
        var dias = nuevas.Select(a => DateOnly.FromDateTime(a.FechaHoraRegistro)).Where(d => d < hoy).Distinct().ToList();
        var total = 0;
        foreach (var dia in dias) total += await procesos.GenerarAsistenciaAsync(dia, dia, ct);
        return total;
    }

    private static void ValidarRango(DateOnly inicio, DateOnly fin)
    {
        if (fin < inicio) throw Errores.Asistencia.RangoInvalido();
        if (fin.DayNumber - inicio.DayNumber + 1 > MaximoDiasRango) throw Errores.Asistencia.RangoMuyGrande(MaximoDiasRango);
    }
}
