using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;

namespace RecursosHumanos.Application.Features.Vacaciones;

public record SaldoVacacionesDto(int AnioServicio, int DiasOtorgados, int DiasExcepcion, int DiasDisfrutados,
    int DiasRestantes, DateOnly FechaCaducidad, bool Caducado);

public record ResumenVacacionesDto(int IdEmpleado, int Disponibles, int Solicitados, List<SaldoVacacionesDto> Saldos);

public record SaldosGeneradosDto(int SaldosCreados);

public class VacacionesService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj, IProcesosBd procesos)
{
    public async Task<ResumenVacacionesDto> ResumenAsync(int idEmpleado, CancellationToken ct)
    {
        if (!await db.EmpleadosVisibles(usuario, Permiso.VacacionesVer).AnyAsync(e => e.IdEmpleado == idEmpleado, ct))
            throw Errores.Empleados.NoEncontrado(idEmpleado);
        var hoy = reloj.Hoy;
        var saldos = await db.SaldosVacaciones.AsNoTracking()
            .Where(s => s.IdEmpleado == idEmpleado && s.Activo)
            .OrderBy(s => s.AnioServicio)
            .Select(s => new SaldoVacacionesDto(s.AnioServicio, s.DiasOtorgados, s.DiasExcepcion, s.DiasDisfrutados,
                s.DiasOtorgados + s.DiasExcepcion - s.DiasDisfrutados, s.FechaCaducidad, s.FechaCaducidad < hoy))
            .ToListAsync(ct);
        var solicitados = await SolicitadosAsync(idEmpleado, ct);
        return new ResumenVacacionesDto(idEmpleado, saldos.Where(s => !s.Caducado).Sum(s => s.DiasRestantes) - solicitados,
            solicitados, saldos);
    }

    public async Task<SaldosGeneradosDto> GenerarSaldosAsync(CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.VacacionesGenerarSaldos);
        return new SaldosGeneradosDto(await procesos.GenerarSaldosVacacionesAsync(reloj.Hoy, ct));
    }

    public async Task ValidarDisponiblesAsync(int idEmpleado, int dias, CancellationToken ct)
    {
        var hoy = reloj.Hoy;
        var restantes = await db.SaldosVacaciones
            .Where(s => s.IdEmpleado == idEmpleado && s.Activo && s.FechaCaducidad >= hoy)
            .SumAsync(s => s.DiasOtorgados + s.DiasExcepcion - s.DiasDisfrutados, ct);
        var disponibles = restantes - await SolicitadosAsync(idEmpleado, ct);
        if (dias > disponibles) throw Errores.Vacaciones.SaldoInsuficiente(Math.Max(disponibles, 0));
    }

    public async Task DescontarAsync(int idEmpleado, int dias, CancellationToken ct)
    {
        var hoy = reloj.Hoy;
        var saldos = await db.SaldosVacaciones
            .Where(s => s.IdEmpleado == idEmpleado && s.Activo && s.FechaCaducidad >= hoy)
            .OrderBy(s => s.FechaCaducidad).ToListAsync(ct);
        var restante = dias;
        foreach (var saldo in saldos)
        {
            var tomar = Math.Min(restante, saldo.DiasOtorgados + saldo.DiasExcepcion - saldo.DiasDisfrutados);
            saldo.DiasDisfrutados += tomar;
            restante -= tomar;
        }
        if (restante > 0) throw Errores.Vacaciones.SaldoInsuficiente(dias - restante);
    }

    public async Task DevolverAsync(int idEmpleado, int dias, CancellationToken ct)
    {
        var saldos = await db.SaldosVacaciones
            .Where(s => s.IdEmpleado == idEmpleado && s.Activo && s.DiasDisfrutados > 0)
            .OrderByDescending(s => s.FechaCaducidad).ToListAsync(ct);
        var restante = dias;
        foreach (var saldo in saldos)
        {
            var regresar = Math.Min(restante, saldo.DiasDisfrutados);
            saldo.DiasDisfrutados -= regresar;
            restante -= regresar;
        }
    }

    private Task<int> SolicitadosAsync(int idEmpleado, CancellationToken ct) =>
        db.Solicitudes
            .Where(s => s.IdEmpleado == idEmpleado && s.Activo
                        && (s.EstatusAprobacion == EstatusSolicitud.Pendiente || s.EstatusAprobacion == EstatusSolicitud.AprobadaJefe)
                        && s.IdTipoPermisoNavigation.IdEstatusAsistenciaNavigation.Clave == EstatusAsistencia.Vacaciones)
            .SumAsync(s => (int)(s.DiasSolicitados ?? 0), ct);
}
