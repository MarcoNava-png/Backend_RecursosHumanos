using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;

namespace RecursosHumanos.Application.Common;

public static class ReglasFechas
{
    public static Task<bool> HayNominaCerradaAsync(this IRecursosHumanosDbContext db, int idEmpleado, DateOnly inicio,
        DateOnly fin, CancellationToken ct) =>
        db.VwBiPeriodosNomina.AnyAsync(p => p.Activo == true && p.TipoNomina == TipoNomina.Ordinaria
            && p.IdEmpresa == db.Empleados.Where(e => e.IdEmpleado == idEmpleado).Select(e => e.IdEmpresa).FirstOrDefault()
            && p.FechaInicio <= fin && p.FechaFin >= inicio
            && p.EstatusCierre != EstatusPeriodoNomina.Abierto && p.EstatusCierre != EstatusPeriodoNomina.Calculado, ct);

    public static Task<bool> HayNominaCerradaEnFechaAsync(this IRecursosHumanosDbContext db, DateOnly fecha, CancellationToken ct) =>
        db.VwBiPeriodosNomina.AnyAsync(p => p.Activo == true && p.TipoNomina == TipoNomina.Ordinaria
            && p.FechaInicio <= fecha && p.FechaFin >= fecha
            && p.EstatusCierre != EstatusPeriodoNomina.Abierto && p.EstatusCierre != EstatusPeriodoNomina.Calculado, ct);

    public static int DiaSemana(DateOnly fecha) => fecha.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)fecha.DayOfWeek;

    public static async Task RecalcularDiasPasadosAsync(this IProcesosBd procesos, IReloj reloj, DateOnly inicio,
        DateOnly fin, CancellationToken ct)
    {
        var ayer = reloj.Hoy.AddDays(-1);
        var hasta = fin < ayer ? fin : ayer;
        if (hasta >= inicio) await procesos.GenerarAsistenciaAsync(inicio, hasta, ct);
    }
}
