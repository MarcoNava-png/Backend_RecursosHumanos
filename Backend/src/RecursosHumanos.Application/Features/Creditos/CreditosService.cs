using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Creditos;

public class CreditosService(IRecursosHumanosDbContext db, IUsuarioActual usuario)
{
    public async Task<CreditosEmpleadoDto> ListarAsync(int idEmpleado, CancellationToken ct)
    {
        if (!await db.EmpleadosVisibles(usuario, Permiso.CreditosVer).AnyAsync(e => e.IdEmpleado == idEmpleado, ct))
            throw Errores.Empleados.NoEncontrado(idEmpleado);

        var infonavit = await db.CreditosInfonavit.AsNoTracking().Where(c => c.IdEmpleado == idEmpleado && c.Activo)
            .OrderByDescending(c => c.FechaInicio)
            .Select(c => new InfonavitDto(c.IdCreditoInfonavit, c.NumeroCredito, c.IdTipoDescuento, c.IdTipoDescuentoNavigation.Descripcion,
                c.ValorDescuento, c.IncluyeSeguroVivienda, c.FechaInicio, c.FechaSuspension)).ToListAsync(ct);
        var fonacot = await db.CreditosFonacot.AsNoTracking().Where(c => c.IdEmpleado == idEmpleado && c.Activo)
            .OrderByDescending(c => c.FechaInicio)
            .Select(c => new FonacotDto(c.IdCreditoFonacot, c.NumeroCredito, c.ImporteTotal, c.RetencionMensual, c.Saldo, c.FechaInicio))
            .ToListAsync(ct);
        var pensiones = await db.PensionesAlimenticias.AsNoTracking().Where(p => p.IdEmpleado == idEmpleado && p.Activo)
            .OrderByDescending(p => p.FechaInicio)
            .Select(p => new PensionDto(p.IdPension, p.Beneficiario, p.OficioJuzgado, p.IdTipoDescuento, p.IdTipoDescuentoNavigation.Descripcion,
                p.Valor, p.FechaInicio, p.FechaFin)).ToListAsync(ct);
        var prestamos = await db.ConceptosHist.AsNoTracking().Where(h => h.IdEmpleado == idEmpleado && h.Activo)
            .OrderByDescending(h => h.FechaInicioDescuento)
            .Select(h => new PrestamoDto(h.IdHistConcepto, h.IdConcepto, h.IdConceptoNavigation.NombreConcepto, h.MontoTotalDeuda,
                h.TotalParcialidades, h.ParcialidadesPagadas, h.SaldoRestante, h.FechaInicioDescuento)).ToListAsync(ct);
        var recurrentes = await db.ConceptosRecurrentes.AsNoTracking().Where(r => r.IdEmpleado == idEmpleado && r.Activo)
            .OrderByDescending(r => r.FechaInicio)
            .Select(r => new RecurrenteDto(r.IdConceptoRecurrente, r.IdConcepto, r.IdConceptoNavigation.NombreConcepto,
                r.IdConceptoNavigation.IdTipoConceptoNavigation.Clave, r.Importe, r.FechaInicio, r.FechaFin)).ToListAsync(ct);
        return new CreditosEmpleadoDto(idEmpleado, infonavit, fonacot, pensiones, prestamos, recurrentes);
    }

    public async Task<CreadoDto> CrearInfonavitAsync(NuevoInfonavit nuevo, CancellationToken ct)
    {
        await ValidarAltaAsync(nuevo.IdEmpleado, ct);
        if (!await db.TiposDescuento.AnyAsync(t => t.IdTipoDescuento == nuevo.IdTipoDescuento && t.AplicaInfonavit, ct))
            throw Errores.Creditos.TipoDescuentoNoAplica();
        var credito = new CreditosInfonavit
        {
            IdEmpleado = nuevo.IdEmpleado, NumeroCredito = nuevo.NumeroCredito.Trim(), IdTipoDescuento = nuevo.IdTipoDescuento,
            ValorDescuento = nuevo.ValorDescuento, IncluyeSeguroVivienda = nuevo.IncluyeSeguroVivienda, FechaInicio = nuevo.FechaInicio
        };
        db.CreditosInfonavit.Add(credito);
        await db.SaveChangesAsync(ct);
        return new CreadoDto(credito.IdCreditoInfonavit);
    }

    public async Task SuspenderInfonavitAsync(int id, FechaTermino suspension, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CreditosEditar);
        var credito = await db.CreditosInfonavit.SingleOrDefaultAsync(c => c.IdCreditoInfonavit == id && c.Activo, ct)
                      ?? throw Errores.Creditos.NoEncontrado("crédito INFONAVIT", id);
        if (suspension.Fecha < credito.FechaInicio) throw Errores.Creditos.FechaFinInvalida();
        credito.FechaSuspension = suspension.Fecha;
        await db.SaveChangesAsync(ct);
    }

    public async Task<CreadoDto> CrearFonacotAsync(NuevoFonacot nuevo, CancellationToken ct)
    {
        await ValidarAltaAsync(nuevo.IdEmpleado, ct);
        var credito = new CreditosFonacot
        {
            IdEmpleado = nuevo.IdEmpleado, NumeroCredito = nuevo.NumeroCredito.Trim(), ImporteTotal = nuevo.ImporteTotal,
            RetencionMensual = nuevo.RetencionMensual, Saldo = nuevo.ImporteTotal, FechaInicio = nuevo.FechaInicio
        };
        db.CreditosFonacot.Add(credito);
        await db.SaveChangesAsync(ct);
        return new CreadoDto(credito.IdCreditoFonacot);
    }

    public async Task CancelarFonacotAsync(int id, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CreditosEditar);
        var credito = await db.CreditosFonacot.SingleOrDefaultAsync(c => c.IdCreditoFonacot == id && c.Activo, ct)
                      ?? throw Errores.Creditos.NoEncontrado("crédito FONACOT", id);
        if (credito.Saldo != credito.ImporteTotal) throw Errores.Creditos.ConPagos();
        credito.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<CreadoDto> CrearPensionAsync(NuevaPension nueva, CancellationToken ct)
    {
        await ValidarAltaAsync(nueva.IdEmpleado, ct);
        if (!await db.TiposDescuento.AnyAsync(t => t.IdTipoDescuento == nueva.IdTipoDescuento && t.AplicaPension, ct))
            throw Errores.Creditos.TipoDescuentoNoAplica();
        if (nueva.FechaFin < nueva.FechaInicio) throw Errores.Creditos.FechaFinInvalida();
        var pension = new PensionesAlimenticias
        {
            IdEmpleado = nueva.IdEmpleado, Beneficiario = nueva.Beneficiario.Trim(), OficioJuzgado = nueva.OficioJuzgado?.Trim(),
            IdTipoDescuento = nueva.IdTipoDescuento, Valor = nueva.Valor, FechaInicio = nueva.FechaInicio, FechaFin = nueva.FechaFin
        };
        db.PensionesAlimenticias.Add(pension);
        await db.SaveChangesAsync(ct);
        return new CreadoDto(pension.IdPension);
    }

    public async Task TerminarPensionAsync(int id, FechaTermino termino, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CreditosEditar);
        var pension = await db.PensionesAlimenticias.SingleOrDefaultAsync(p => p.IdPension == id && p.Activo, ct)
                      ?? throw Errores.Creditos.NoEncontrado("pensión", id);
        if (termino.Fecha < pension.FechaInicio) throw Errores.Creditos.FechaFinInvalida();
        pension.FechaFin = termino.Fecha;
        await db.SaveChangesAsync(ct);
    }

    public async Task<CreadoDto> CrearPrestamoAsync(NuevoPrestamo nuevo, CancellationToken ct)
    {
        await ValidarAltaAsync(nuevo.IdEmpleado, ct);
        await ValidarConceptoAsync(nuevo.IdConcepto, soloDeducciones: true, ct);
        var prestamo = new ConceptosHist
        {
            IdEmpleado = nuevo.IdEmpleado, IdConcepto = nuevo.IdConcepto, MontoTotalDeuda = nuevo.MontoTotal,
            TotalParcialidades = nuevo.TotalParcialidades, SaldoRestante = nuevo.MontoTotal, FechaInicioDescuento = nuevo.FechaInicioDescuento
        };
        db.ConceptosHist.Add(prestamo);
        await db.SaveChangesAsync(ct);
        return new CreadoDto(prestamo.IdHistConcepto);
    }

    public async Task CancelarPrestamoAsync(int id, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CreditosEditar);
        var prestamo = await db.ConceptosHist.SingleOrDefaultAsync(h => h.IdHistConcepto == id && h.Activo, ct)
                       ?? throw Errores.Creditos.NoEncontrado("préstamo", id);
        if (prestamo.ParcialidadesPagadas > 0) throw Errores.Creditos.ConPagos();
        prestamo.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<CreadoDto> CrearRecurrenteAsync(NuevoRecurrente nuevo, CancellationToken ct)
    {
        await ValidarAltaAsync(nuevo.IdEmpleado, ct);
        await ValidarConceptoAsync(nuevo.IdConcepto, soloDeducciones: false, ct);
        if (nuevo.FechaFin < nuevo.FechaInicio) throw Errores.Creditos.FechaFinInvalida();
        var recurrente = new ConceptosRecurrentes
        {
            IdEmpleado = nuevo.IdEmpleado, IdConcepto = nuevo.IdConcepto, Importe = nuevo.Importe,
            FechaInicio = nuevo.FechaInicio, FechaFin = nuevo.FechaFin
        };
        db.ConceptosRecurrentes.Add(recurrente);
        await db.SaveChangesAsync(ct);
        return new CreadoDto(recurrente.IdConceptoRecurrente);
    }

    public async Task TerminarRecurrenteAsync(int id, FechaTermino termino, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CreditosEditar);
        var recurrente = await db.ConceptosRecurrentes.SingleOrDefaultAsync(r => r.IdConceptoRecurrente == id && r.Activo, ct)
                         ?? throw Errores.Creditos.NoEncontrado("concepto fijo", id);
        if (termino.Fecha < recurrente.FechaInicio) throw Errores.Creditos.FechaFinInvalida();
        recurrente.FechaFin = termino.Fecha;
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidarAltaAsync(int idEmpleado, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CreditosEditar);
        if (!await db.Empleados.AnyAsync(e => e.IdEmpleado == idEmpleado && e.Activo, ct)) throw Errores.Empleados.NoEncontrado(idEmpleado);
    }

    private async Task ValidarConceptoAsync(int idConcepto, bool soloDeducciones, CancellationToken ct)
    {
        var valido = await db.ConceptosNomina.AnyAsync(c => c.IdConcepto == idConcepto && c.Activo && !c.EsCalculado
            && (!soloDeducciones || c.IdTipoConceptoNavigation.Clave == TipoConcepto.Deduccion), ct);
        if (!valido) throw Errores.Creditos.ConceptoNoValido();
    }
}
