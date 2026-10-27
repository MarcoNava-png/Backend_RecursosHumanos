using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Interfaces;

namespace RecursosHumanos.Infrastructure.Persistence;

public class ProcesosBd(RecursosHumanosDbContext db) : IProcesosBd
{
    public Task<int> GenerarAsistenciaAsync(DateOnly inicio, DateOnly fin, CancellationToken ct) =>
        db.Database.SqlQuery<int>($"SELECT recursos_humanos.fn_generar_asistencia_rango({inicio}, {fin}) AS \"Value\"")
            .SingleAsync(ct);

    public Task<int> GenerarSaldosVacacionesAsync(DateOnly fecha, CancellationToken ct) =>
        db.Database.SqlQuery<int>($"SELECT recursos_humanos.fn_generar_saldos_vacaciones({fecha}) AS \"Value\"")
            .SingleAsync(ct);

    public Task<int> GenerarPeriodosAsync(int idEmpresa, int ejercicio, string periodicidad, CancellationToken ct) =>
        db.Database.SqlQuery<int>($"SELECT nomina.fn_generar_periodos({idEmpresa}, {ejercicio}, {periodicidad}) AS \"Value\"")
            .SingleAsync(ct);

    public Task<int> CalcularNominaAsync(int idPeriodo, CancellationToken ct) =>
        db.Database.SqlQuery<int>($"SELECT nomina.fn_calcular_nomina({idPeriodo}) AS \"Value\"").SingleAsync(ct);

    public Task AutorizarPeriodoAsync(int idPeriodo, int idEmpleadoAutoriza, int idUsuario, CancellationToken ct) =>
        db.Database.ExecuteSqlAsync($"SELECT nomina.fn_autorizar_periodo({idPeriodo}, {idEmpleadoAutoriza}, {idUsuario})", ct);

    public Task CerrarPeriodoAsync(int idPeriodo, int idUsuario, CancellationToken ct) =>
        db.Database.ExecuteSqlAsync($"SELECT nomina.fn_cerrar_periodo({idPeriodo}, {idUsuario})", ct);

    public Task<int> CalcularAguinaldoAsync(int idPeriodo, CancellationToken ct) =>
        db.Database.SqlQuery<int>($"SELECT nomina.fn_calcular_aguinaldo({idPeriodo}) AS \"Value\"").SingleAsync(ct);

    public Task<int> CalcularPtuAsync(int idPtu, CancellationToken ct) =>
        db.Database.SqlQuery<int>($"SELECT nomina.fn_calcular_ptu({idPtu}) AS \"Value\"").SingleAsync(ct);

    public Task<int> CalcularFiniquitoAsync(int idBaja, DateOnly? fechaPago, CancellationToken ct) =>
        db.Database.SqlQuery<int>($"SELECT nomina.fn_calcular_finiquito({idBaja}, {fechaPago}) AS \"Value\"").SingleAsync(ct);

    public Task<int> PrepararCuentasDirectorioAsync(CancellationToken ct) =>
        db.Database.SqlQuery<int>($"SELECT recursos_humanos.fn_preparar_cuentas_directorio() AS \"Value\"").SingleAsync(ct);
}
