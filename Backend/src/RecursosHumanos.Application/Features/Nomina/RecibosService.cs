using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Nomina;

public class RecibosService(IRecursosHumanosDbContext db, IUsuarioActual usuario)
{
    public async Task<List<ReciboDto>> ListarPorPeriodoAsync(int idPeriodo, CancellationToken ct)
    {
        if (!VeTodos()) throw Errores.General.SinPermiso();
        return await db.VwBiRecibos.AsNoTracking().Where(r => r.IdPeriodoNomina == idPeriodo)
            .OrderBy(r => r.Empleado).Select(Proyeccion).ToListAsync(ct);
    }

    public async Task<List<ReciboDto>> MisRecibosAsync(int? ejercicio, CancellationToken ct)
    {
        var idEmpleado = usuario.IdEmpleado ?? throw Errores.General.SinPermiso();
        return await Autorizados(db.VwBiRecibos.AsNoTracking())
            .Where(r => r.IdEmpleado == idEmpleado && (ejercicio == null || r.FechaPago!.Value.Year == ejercicio))
            .OrderByDescending(r => r.FechaPago).Select(Proyeccion).ToListAsync(ct);
    }

    public async Task<ReciboDetalleDto> ObtenerAsync(int idNomina, CancellationToken ct)
    {
        var consulta = db.VwBiRecibos.AsNoTracking().Where(r => r.IdNomina == idNomina);
        if (!VeTodos()) consulta = Autorizados(consulta.Where(r => r.IdEmpleado == usuario.IdEmpleado));
        var r = await consulta.SingleOrDefaultAsync(ct) ?? throw Errores.Nomina.ReciboNoEncontrado(idNomina);

        var conceptos = await db.NominaDetalle.AsNoTracking().Where(d => d.IdNomina == idNomina && d.Activo)
            .OrderBy(d => d.IdConceptoNavigation.IdTipoConceptoNavigation.Clave).ThenBy(d => d.IdConceptoNavigation.ClaveConcepto)
            .Select(d => new ConceptoReciboDto(d.IdConceptoNavigation.ClaveConcepto, d.IdConceptoNavigation.NombreConcepto,
                d.IdConceptoNavigation.IdTipoConceptoNavigation.Clave, d.Cantidad, d.ImporteGravado, d.ImporteExento,
                d.ImporteTotal ?? 0))
            .ToListAsync(ct);
        return new ReciboDetalleDto(AReciboDto(r), r.Empresa!, r.Rfc, r.Curp, r.Nss, r.SalarioDiario ?? 0,
            r.SalarioDiarioIntegrado ?? 0, r.Observaciones, conceptos);
    }

    private bool VeTodos() => usuario.TienePermiso(Permiso.NominaVer);

    private IQueryable<VwBiRecibos> Autorizados(IQueryable<VwBiRecibos> recibos) =>
        recibos.Where(r => db.PeriodosNomina.Any(p => p.IdPeriodoNomina == r.IdPeriodoNomina
            && p.EstatusCierre != EstatusPeriodoNomina.Abierto && p.EstatusCierre != EstatusPeriodoNomina.Calculado));

    private static readonly Expression<Func<VwBiRecibos, ReciboDto>> Proyeccion = r => new ReciboDto(
        r.IdNomina!.Value, r.IdPeriodoNomina!.Value, r.Periodo!, r.TipoNomina!, r.FechaPago!.Value, r.IdEmpleado!.Value,
        r.Matricula, r.Empleado!, r.Departamento!, r.DiasPagados ?? 0, r.TotalPercepciones ?? 0, r.TotalDeducciones ?? 0,
        r.TotalOtrosPagos ?? 0, r.IsrRetenido ?? 0, r.ImssObrero ?? 0, r.NetoAPagar ?? 0, r.EstatusRecibo!);

    private static readonly Func<VwBiRecibos, ReciboDto> AReciboDto = Proyeccion.Compile();
}
