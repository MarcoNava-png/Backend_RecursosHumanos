using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Prestaciones;

public class PtuService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IProcesosBd procesos)
{
    public async Task<List<PtuDto>> ListarAsync(int? idEmpresa, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.PtuVer);
        return await db.Ptu.AsNoTracking().Where(p => p.Activo && (idEmpresa == null || p.IdEmpresa == idEmpresa))
            .OrderByDescending(p => p.Ejercicio)
            .Select(p => new PtuDto(p.IdPtu, p.IdEmpresa, p.Ejercicio, p.RentaGravable, p.MontoRepartir ?? 0,
                p.TopeSalarioConfianza, p.IdPeriodoNomina, p.Estatus, p.PtuEmpleados.Count(e => e.Activo),
                p.PtuEmpleados.Where(e => e.Activo).Sum(e => e.PtuFinal)))
            .ToListAsync(ct);
    }

    public async Task<int> CrearAsync(NuevoPtu nuevo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.PtuEditar);
        if (await db.Ptu.AnyAsync(p => p.IdEmpresa == nuevo.IdEmpresa && p.Ejercicio == nuevo.Ejercicio, ct))
            throw Errores.Prestaciones.PtuDuplicado(nuevo.Ejercicio);
        var inicioMes = new DateOnly(nuevo.FechaPago.Year, nuevo.FechaPago.Month, 1);
        var periodo = new PeriodosNomina
        {
            IdEmpresa = nuevo.IdEmpresa, Ejercicio = nuevo.FechaPago.Year, NumeroPeriodo = 1,
            IdTipoNomina = await IdTipoNominaAsync(TipoNomina.Ptu, ct),
            IdPeriodicidadPago = await db.PeriodicidadesPago.Where(x => x.Clave == PeriodicidadPago.Unica).Select(x => x.IdPeriodicidadPago).SingleAsync(ct),
            Descripcion = $"PTU {nuevo.Ejercicio}", FechaInicio = inicioMes, FechaFin = inicioMes.AddMonths(1).AddDays(-1),
            FechaPago = nuevo.FechaPago, EstatusCierre = EstatusPeriodoNomina.Abierto
        };
        var ptu = new Ptu
        {
            IdEmpresa = nuevo.IdEmpresa, Ejercicio = nuevo.Ejercicio, RentaGravable = nuevo.RentaGravable,
            TopeSalarioConfianza = nuevo.TopeSalarioConfianza, IdPeriodoNominaNavigation = periodo, Estatus = EstatusPtu.Borrador
        };
        db.Ptu.Add(ptu);
        await db.SaveChangesAsync(ct);
        return ptu.IdPtu;
    }

    public async Task ActualizarAsync(int idPtu, ActualizarPtu datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.PtuEditar);
        var ptu = await CargarEditableAsync(idPtu, ct);
        ptu.RentaGravable = datos.RentaGravable;
        ptu.TopeSalarioConfianza = datos.TopeSalarioConfianza;
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<PtuEmpleadoDto>> EmpleadosAsync(int idPtu, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.PtuVer);
        if (!await db.Ptu.AnyAsync(p => p.IdPtu == idPtu && p.Activo, ct)) throw Errores.Prestaciones.PtuNoEncontrado(idPtu);
        return await db.PtuEmpleados.AsNoTracking().Where(e => e.IdPtu == idPtu && e.Activo)
            .OrderBy(e => e.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno)
            .Select(e => new PtuEmpleadoDto(e.IdEmpleado,
                (e.IdEmpleadoNavigation.IdPersonaNavigation.Nombre + " " + e.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno).Trim(),
                e.DiasTrabajados, e.SalarioAnual, e.PtuPorDias, e.PtuPorSalarios, e.PtuCalculada, e.TopeTresMeses,
                e.PromedioTresAnios, e.PtuFinal))
            .ToListAsync(ct);
    }

    public async Task CapturarPromedioAsync(int idPtu, int idEmpleado, PromedioPtu promedio, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.PtuEditar);
        await CargarEditableAsync(idPtu, ct);
        var empleado = await db.PtuEmpleados.SingleOrDefaultAsync(e => e.IdPtu == idPtu && e.IdEmpleado == idEmpleado && e.Activo, ct)
                       ?? throw Errores.Prestaciones.EmpleadoSinPtu();
        empleado.PromedioTresAnios = promedio.PromedioTresAnios;
        await db.SaveChangesAsync(ct);
    }

    public async Task<CalculoPtuDto> CalcularAsync(int idPtu, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.PtuEditar);
        await CargarEditableAsync(idPtu, ct);
        return new CalculoPtuDto(await procesos.CalcularPtuAsync(idPtu, ct));
    }

    private async Task<Ptu> CargarEditableAsync(int idPtu, CancellationToken ct)
    {
        var ptu = await db.Ptu.Include(p => p.IdPeriodoNominaNavigation).SingleOrDefaultAsync(p => p.IdPtu == idPtu && p.Activo, ct)
                  ?? throw Errores.Prestaciones.PtuNoEncontrado(idPtu);
        var estatus = ptu.IdPeriodoNominaNavigation?.EstatusCierre ?? EstatusPeriodoNomina.Abierto;
        if (estatus is not (EstatusPeriodoNomina.Abierto or EstatusPeriodoNomina.Calculado)) throw Errores.Prestaciones.PtuNoEditable(estatus);
        return ptu;
    }

    private Task<int> IdTipoNominaAsync(string clave, CancellationToken ct) =>
        db.TiposNomina.Where(x => x.Clave == clave).Select(x => x.IdTipoNomina).SingleAsync(ct);
}
