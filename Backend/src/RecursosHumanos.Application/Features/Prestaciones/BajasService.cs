using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Prestaciones;

public class BajasService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj, IProcesosBd procesos)
{
    public async Task<List<BajaDto>> ListarAsync(string? estatusPago, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.BajasVer);
        return await db.BajasLaborales.AsNoTracking()
            .Where(b => b.Activo && (estatusPago == null || b.EstatusPago == estatusPago))
            .OrderByDescending(b => b.FechaBaja)
            .Select(b => new BajaDto(b.IdBaja, b.IdEmpleado,
                (b.IdEmpleadoNavigation.IdPersonaNavigation.Nombre + " " + b.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno + " " +
                 (b.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoMaterno ?? "")).Trim(),
                b.IdMotivoBaja, b.IdMotivoBajaNavigation.DescripcionMotivo, b.FechaBaja, b.MontoFiniquitoCalculado,
                b.IdNominaFiniquito, b.EstatusPago))
            .ToListAsync(ct);
    }

    public async Task<int> RegistrarAsync(NuevaBaja nueva, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.BajasEditar);
        var empleado = await db.Empleados.Include(e => e.IdEstatusLaboralNavigation)
                           .SingleOrDefaultAsync(e => e.IdEmpleado == nueva.IdEmpleado && e.Activo, ct)
                       ?? throw Errores.Empleados.NoEncontrado(nueva.IdEmpleado);
        if (empleado.IdEstatusLaboralNavigation.Clave == EstatusLaboral.Baja) throw Errores.Prestaciones.YaTieneBaja();
        if (nueva.FechaBaja > reloj.Hoy) throw Errores.Prestaciones.FechaBajaFutura();
        if (nueva.FechaBaja < empleado.FechaIngreso) throw Errores.Prestaciones.FechaBajaAntesDeIngreso();

        var baja = new BajasLaborales
        {
            IdEmpleado = empleado.IdEmpleado, IdMotivoBaja = nueva.IdMotivoBaja, FechaBaja = nueva.FechaBaja,
            EstatusPago = EstatusPagoBaja.Pendiente
        };
        db.BajasLaborales.Add(baja);
        empleado.IdEstatusLaboral = await db.EstatusLaborales.Where(x => x.Clave == EstatusLaboral.Baja)
            .Select(x => x.IdEstatusLaboral).SingleAsync(ct);
        await db.Usuarios.Where(u => u.IdEmpleado == empleado.IdEmpleado)
            .ForEachAsync(u => u.EstatusUsuario = EstatusUsuario.Inactivo, ct);
        await db.SaveChangesAsync(ct);
        return baja.IdBaja;
    }

    public async Task<FiniquitoDto> CalcularFiniquitoAsync(int idBaja, CalcularFiniquito calcular, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.BajasEditar);
        var baja = await CargarAsync(idBaja, ct);
        if (baja.EstatusPago == EstatusPagoBaja.Pagado) throw Errores.Prestaciones.BajaPagada();

        var idNomina = await procesos.CalcularFiniquitoAsync(idBaja, calcular.FechaPago, ct);
        return await db.Nominas.AsNoTracking().Where(n => n.IdNomina == idNomina)
            .Select(n => new FiniquitoDto(idBaja, n.IdNomina, n.IdPeriodoNomina, n.NetoAPagar)).SingleAsync(ct);
    }

    public async Task MarcarPagadaAsync(int idBaja, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.BajasPagar);
        var baja = await CargarAsync(idBaja, ct);
        if (baja.EstatusPago == EstatusPagoBaja.Pagado) throw Errores.Prestaciones.BajaPagada();
        var estatusPeriodo = await db.Nominas.Where(n => n.IdNomina == baja.IdNominaFiniquito)
                                 .Select(n => n.IdPeriodoNominaNavigation.EstatusCierre).SingleOrDefaultAsync(ct)
                             ?? throw Errores.Prestaciones.SinFiniquito();
        if (estatusPeriodo is EstatusPeriodoNomina.Abierto or EstatusPeriodoNomina.Calculado)
            throw Errores.Prestaciones.FiniquitoSinAutorizar();

        baja.EstatusPago = EstatusPagoBaja.Pagado;
        await db.SaveChangesAsync(ct);
    }

    private async Task<BajasLaborales> CargarAsync(int idBaja, CancellationToken ct) =>
        await db.BajasLaborales.SingleOrDefaultAsync(b => b.IdBaja == idBaja && b.Activo, ct)
        ?? throw Errores.Prestaciones.BajaNoEncontrada(idBaja);
}
