using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Nomina;

public class PeriodosNominaService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IProcesosBd procesos)
{
    public async Task<List<PeriodoDto>> ListarAsync(FiltroPeriodos filtro, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaVer);
        var consulta = db.PeriodosNomina.AsNoTracking().Where(p => p.Activo);
        if (filtro.IdEmpresa is not null) consulta = consulta.Where(p => p.IdEmpresa == filtro.IdEmpresa);
        if (filtro.Ejercicio is not null) consulta = consulta.Where(p => p.Ejercicio == filtro.Ejercicio);
        if (filtro.TipoNomina is not null) consulta = consulta.Where(p => p.IdTipoNominaNavigation.Clave == filtro.TipoNomina);
        if (filtro.Estatus is not null) consulta = consulta.Where(p => p.EstatusCierre == filtro.Estatus);

        return await consulta.OrderBy(p => p.FechaInicio).ThenBy(p => p.IdTipoNomina)
            .Select(p => new PeriodoDto(p.IdPeriodoNomina, p.IdEmpresa, p.IdTipoNominaNavigation.Clave,
                p.IdPeriodicidadPagoNavigation.Clave, p.Ejercicio, p.NumeroPeriodo, p.Descripcion, p.FechaInicio,
                p.FechaFin, p.FechaPago, p.EstatusCierre,
                p.Nominas.Count(n => n.Activo), p.Nominas.Where(n => n.Activo).Sum(n => n.NetoAPagar)))
            .ToListAsync(ct);
    }

    public async Task<PeriodosGeneradosDto> GenerarAsync(GenerarPeriodos generar, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaCalcular);
        return new PeriodosGeneradosDto(await procesos.GenerarPeriodosAsync(generar.IdEmpresa, generar.Ejercicio, generar.Periodicidad, ct));
    }

    public async Task<PeriodoCreadoDto> CrearAguinaldoAsync(NuevoPeriodoAguinaldo nuevo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaCalcular);
        var periodo = new PeriodosNomina
        {
            IdEmpresa = nuevo.IdEmpresa, Ejercicio = nuevo.Ejercicio, NumeroPeriodo = 1, Descripcion = $"Aguinaldo {nuevo.Ejercicio}",
            IdTipoNomina = await db.TiposNomina.Where(x => x.Clave == TipoNomina.Aguinaldo).Select(x => x.IdTipoNomina).SingleAsync(ct),
            IdPeriodicidadPago = await db.PeriodicidadesPago.Where(x => x.Clave == PeriodicidadPago.Unica).Select(x => x.IdPeriodicidadPago).SingleAsync(ct),
            FechaInicio = new DateOnly(nuevo.Ejercicio, 1, 1), FechaFin = new DateOnly(nuevo.Ejercicio, 12, 31),
            FechaPago = nuevo.FechaPago, EstatusCierre = EstatusPeriodoNomina.Abierto
        };
        db.PeriodosNomina.Add(periodo);
        await db.SaveChangesAsync(ct);
        return new PeriodoCreadoDto(periodo.IdPeriodoNomina);
    }

    public async Task<CalculoDto> CalcularAsync(int idPeriodo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaCalcular);
        var tipo = await db.PeriodosNomina.Where(p => p.IdPeriodoNomina == idPeriodo && p.Activo)
                       .Select(p => p.IdTipoNominaNavigation.Clave).SingleOrDefaultAsync(ct)
                   ?? throw Errores.Nomina.PeriodoNoEncontrado(idPeriodo);
        var recibos = tipo switch
        {
            TipoNomina.Aguinaldo => await procesos.CalcularAguinaldoAsync(idPeriodo, ct),
            TipoNomina.Ptu => await procesos.CalcularPtuAsync(await IdPtuDelPeriodoAsync(idPeriodo, ct), ct),
            TipoNomina.Finiquito => throw Errores.Nomina.FiniquitoDesdeBaja(),
            _ => await procesos.CalcularNominaAsync(idPeriodo, ct)
        };
        return new CalculoDto(recibos);
    }

    private async Task<int> IdPtuDelPeriodoAsync(int idPeriodo, CancellationToken ct) =>
        await db.Ptu.Where(p => p.IdPeriodoNomina == idPeriodo && p.Activo).Select(p => (int?)p.IdPtu).SingleOrDefaultAsync(ct)
        ?? throw Errores.Nomina.PeriodoSinPtu();

    public async Task AutorizarAsync(int idPeriodo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaAutorizar);
        var idEmpleado = usuario.IdEmpleado ?? throw Errores.Nomina.SinEmpleado();
        await ValidarExisteAsync(idPeriodo, ct);
        await procesos.AutorizarPeriodoAsync(idPeriodo, idEmpleado, usuario.IdUsuario!.Value, ct);
    }

    public async Task CerrarAsync(int idPeriodo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaCerrar);
        await ValidarExisteAsync(idPeriodo, ct);
        await procesos.CerrarPeriodoAsync(idPeriodo, usuario.IdUsuario!.Value, ct);
    }

    public async Task<List<IncidenciaNominaDto>> ListarIncidenciasAsync(int idPeriodo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaVer);
        await ValidarExisteAsync(idPeriodo, ct);
        return await db.IncidenciasNomina.AsNoTracking()
            .Where(i => i.IdPeriodoNomina == idPeriodo && i.Activo)
            .OrderBy(i => i.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno).ThenBy(i => i.IdConceptoNavigation.ClaveConcepto)
            .Select(i => new IncidenciaNominaDto(i.IdIncidenciaNomina, i.IdEmpleado,
                (i.IdEmpleadoNavigation.IdPersonaNavigation.Nombre + " " + i.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno + " " +
                 (i.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoMaterno ?? "")).Trim(),
                i.IdConcepto, i.IdConceptoNavigation.ClaveConcepto, i.IdConceptoNavigation.NombreConcepto,
                i.IdConceptoNavigation.IdTipoConceptoNavigation.Clave, i.Cantidad, i.Importe, i.Observaciones))
            .ToListAsync(ct);
    }

    public async Task<IncidenciaGuardadaDto> CrearIncidenciaAsync(int idPeriodo, NuevaIncidenciaNomina nueva, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaCalcular);
        var periodo = await PeriodoEditableAsync(idPeriodo, ct);
        if (!await db.ConceptosNomina.AnyAsync(c => c.IdConcepto == nueva.IdConcepto && c.Activo && !c.EsCalculado, ct))
            throw Errores.Nomina.ConceptoNoCapturable();
        if (!await db.EmpleadosNomina.AnyAsync(en => en.IdEmpleado == nueva.IdEmpleado && en.Activo
                && en.IdEmpleadoNavigation.Activo && en.IdEmpleadoNavigation.IdEmpresa == periodo.IdEmpresa, ct))
            throw Errores.Nomina.EmpleadoFueraDeNomina();

        var incidencia = new IncidenciasNomina
        {
            IdPeriodoNomina = idPeriodo, IdEmpleado = nueva.IdEmpleado, IdConcepto = nueva.IdConcepto,
            Cantidad = nueva.Cantidad, Importe = nueva.Importe, Observaciones = nueva.Observaciones?.Trim()
        };
        db.IncidenciasNomina.Add(incidencia);
        await db.SaveChangesAsync(ct);
        return new IncidenciaGuardadaDto(incidencia.IdIncidenciaNomina, periodo.EstatusCierre == EstatusPeriodoNomina.Calculado);
    }

    public async Task<IncidenciaGuardadaDto> EliminarIncidenciaAsync(int idPeriodo, int idIncidencia, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaCalcular);
        var periodo = await PeriodoEditableAsync(idPeriodo, ct);
        var incidencia = await db.IncidenciasNomina
                             .SingleOrDefaultAsync(i => i.IdIncidenciaNomina == idIncidencia && i.IdPeriodoNomina == idPeriodo && i.Activo, ct)
                         ?? throw Errores.Nomina.IncidenciaNoEncontrada(idIncidencia);
        incidencia.Activo = false;
        await db.SaveChangesAsync(ct);
        return new IncidenciaGuardadaDto(idIncidencia, periodo.EstatusCierre == EstatusPeriodoNomina.Calculado);
    }

    private async Task<PeriodosNomina> PeriodoEditableAsync(int idPeriodo, CancellationToken ct)
    {
        var periodo = await db.PeriodosNomina.Include(p => p.IdTipoNominaNavigation)
                          .SingleOrDefaultAsync(p => p.IdPeriodoNomina == idPeriodo && p.Activo, ct)
                      ?? throw Errores.Nomina.PeriodoNoEncontrado(idPeriodo);
        if (periodo.IdTipoNominaNavigation.Clave != TipoNomina.Ordinaria) throw Errores.Nomina.SoloOrdinaria();
        if (periodo.EstatusCierre is not (EstatusPeriodoNomina.Abierto or EstatusPeriodoNomina.Calculado))
            throw Errores.Nomina.PeriodoNoEditable(periodo.EstatusCierre);
        return periodo;
    }

    private async Task ValidarExisteAsync(int idPeriodo, CancellationToken ct)
    {
        if (!await db.PeriodosNomina.AnyAsync(p => p.IdPeriodoNomina == idPeriodo && p.Activo, ct))
            throw Errores.Nomina.PeriodoNoEncontrado(idPeriodo);
    }
}
