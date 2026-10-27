namespace RecursosHumanos.Application.Common.Interfaces;

public interface IProcesosBd
{
    Task<int> GenerarAsistenciaAsync(DateOnly inicio, DateOnly fin, CancellationToken ct);
    Task<int> GenerarSaldosVacacionesAsync(DateOnly fecha, CancellationToken ct);
    Task<int> GenerarPeriodosAsync(int idEmpresa, int ejercicio, string periodicidad, CancellationToken ct);
    Task<int> CalcularNominaAsync(int idPeriodo, CancellationToken ct);
    Task AutorizarPeriodoAsync(int idPeriodo, int idEmpleadoAutoriza, int idUsuario, CancellationToken ct);
    Task CerrarPeriodoAsync(int idPeriodo, int idUsuario, CancellationToken ct);
    Task<int> CalcularAguinaldoAsync(int idPeriodo, CancellationToken ct);
    Task<int> CalcularPtuAsync(int idPtu, CancellationToken ct);

    Task<int> CalcularFiniquitoAsync(int idBaja, DateOnly? fechaPago, CancellationToken ct);

    Task<int> PrepararCuentasDirectorioAsync(CancellationToken ct);
}
