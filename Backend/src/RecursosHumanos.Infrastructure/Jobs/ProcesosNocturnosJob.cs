using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RecursosHumanos.Application.Common.Interfaces;

namespace RecursosHumanos.Infrastructure.Jobs;

public class ProcesosNocturnosJob(IServiceScopeFactory scopes, IReloj reloj, ILogger<ProcesosNocturnosJob> logger,
    TimeOnly horaEjecucion) : BackgroundService
{
    private const int DiasARecalcular = 3;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TiempoParaSiguienteCorrida(), ct);
            var hoy = reloj.Hoy;
            var fin = hoy.AddDays(-1);
            var inicio = fin.AddDays(1 - DiasARecalcular);
            try
            {
                using var scope = scopes.CreateScope();
                var procesos = scope.ServiceProvider.GetRequiredService<IProcesosBd>();
                var saldos = await procesos.GenerarSaldosVacacionesAsync(hoy, ct);
                var jornadas = await procesos.GenerarAsistenciaAsync(inicio, fin, ct);
                logger.LogInformation("Procesos nocturnos: {Saldos} saldos de vacaciones, {Jornadas} jornadas del {Inicio} al {Fin}",
                    saldos, jornadas, inicio, fin);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Fallaron los procesos nocturnos del {Fecha}", hoy);
            }
        }
    }

    private TimeSpan TiempoParaSiguienteCorrida()
    {
        var ahora = TimeZoneInfo.ConvertTimeFromUtc(reloj.AhoraUtc, Reloj.ZonaHoraria);
        var siguiente = ahora.Date + horaEjecucion.ToTimeSpan();
        return (siguiente > ahora ? siguiente : siguiente.AddDays(1)) - ahora;
    }
}
