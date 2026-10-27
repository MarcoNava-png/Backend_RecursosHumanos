using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Nomina;

public class DatosNominaService(IRecursosHumanosDbContext db, IUsuarioActual usuario)
{
    public async Task<DatosNominaDto> ObtenerAsync(int idEmpleado, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaDatosVer);
        return await db.EmpleadosNomina.AsNoTracking().Where(n => n.IdEmpleado == idEmpleado && n.Activo)
                   .Select(n => new DatosNominaDto(n.IdEmpleado, n.IdRegistroPatronal, n.IdEsquemaPrestaciones, n.Nss,
                       n.FechaAltaImss, n.IdPeriodicidadPago, n.IdTipoContrato, n.IdTipoRegimen, n.IdTipoJornada,
                       n.IdClaseRiesgoPuesto, n.Sindicalizado, n.ExcluidoPtu, n.IdMetodoPago, n.IdBanco, n.Clabe))
                   .SingleOrDefaultAsync(ct)
               ?? throw Errores.Nomina.SinDatosNomina(idEmpleado);
    }

    public async Task GuardarAsync(int idEmpleado, GuardarDatosNomina datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaDatosEditar);
        var idEmpresa = await db.Empleados.Where(e => e.IdEmpleado == idEmpleado && e.Activo)
                            .Select(e => (int?)e.IdEmpresa).SingleOrDefaultAsync(ct)
                        ?? throw Errores.Empleados.NoEncontrado(idEmpleado);
        if (!await db.RegistrosPatronales.AnyAsync(r => r.IdRegistroPatronal == datos.IdRegistroPatronal && r.IdEmpresa == idEmpresa, ct))
            throw Errores.Nomina.RegistroPatronalDeOtraEmpresa();
        if (!await db.EsquemasPrestaciones.AnyAsync(e => e.IdEsquemaPrestaciones == datos.IdEsquemaPrestaciones && e.IdEmpresa == idEmpresa, ct))
            throw Errores.Nomina.EsquemaDeOtraEmpresa();
        if (datos.Clabe is null && await db.MetodosPago.AnyAsync(m => m.IdMetodoPago == datos.IdMetodoPago && m.RequiereClabe, ct))
            throw Errores.Nomina.FaltaClabe();

        var nomina = await db.EmpleadosNomina.SingleOrDefaultAsync(n => n.IdEmpleado == idEmpleado, ct);
        if (nomina is null)
        {
            nomina = new EmpleadosNomina { IdEmpleado = idEmpleado };
            db.EmpleadosNomina.Add(nomina);
        }
        nomina.IdRegistroPatronal = datos.IdRegistroPatronal;
        nomina.IdEsquemaPrestaciones = datos.IdEsquemaPrestaciones;
        nomina.Nss = datos.Nss;
        nomina.FechaAltaImss = datos.FechaAltaImss;
        nomina.IdPeriodicidadPago = datos.IdPeriodicidadPago;
        nomina.IdTipoContrato = datos.IdTipoContrato;
        nomina.IdTipoRegimen = datos.IdTipoRegimen;
        nomina.IdTipoJornada = datos.IdTipoJornada;
        nomina.IdClaseRiesgoPuesto = datos.IdClaseRiesgoPuesto;
        nomina.Sindicalizado = datos.Sindicalizado;
        nomina.ExcluidoPtu = datos.ExcluidoPtu;
        nomina.IdMetodoPago = datos.IdMetodoPago;
        nomina.IdBanco = datos.IdBanco;
        nomina.Clabe = datos.Clabe;
        nomina.Activo = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<SueldoDto>> SueldosAsync(int idEmpleado, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaDatosVer);
        return await db.HistorialSueldos.AsNoTracking().Where(s => s.IdEmpleado == idEmpleado && s.Activo)
            .OrderByDescending(s => s.FechaInicioVigencia)
            .Select(s => new SueldoDto(s.IdHistorialSueldo, s.SalarioBase, s.TarifaPorHora, s.FechaInicioVigencia,
                s.FechaFinVigencia, s.FechaFinVigencia == null))
            .ToListAsync(ct);
    }

    public async Task<int> RegistrarSueldoAsync(int idEmpleado, NuevoSueldo nuevo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.NominaDatosEditar);
        if (nuevo.SalarioDiario is null && nuevo.TarifaPorHora is null) throw Errores.Nomina.SueldoSinMonto();
        if (!await db.Empleados.AnyAsync(e => e.IdEmpleado == idEmpleado && e.Activo, ct)) throw Errores.Empleados.NoEncontrado(idEmpleado);
        var inicio = nuevo.FechaInicioVigencia;
        if (await db.HayNominaCerradaAsync(idEmpleado, inicio, inicio, ct)) throw Errores.Nomina.NominaCerrada();

        await using var transaccion = await db.Database.BeginTransactionAsync(ct);
        var vigente = await db.HistorialSueldos.SingleOrDefaultAsync(s => s.IdEmpleado == idEmpleado && s.Activo && s.FechaFinVigencia == null, ct);
        if (vigente is not null)
        {
            if (inicio <= vigente.FechaInicioVigencia) throw Errores.Nomina.SueldoAnteriorAlVigente(vigente.FechaInicioVigencia);
            vigente.FechaFinVigencia = inicio.AddDays(-1);
            await db.SaveChangesAsync(ct);
        }
        var sueldo = new HistorialSueldos
        {
            IdEmpleado = idEmpleado, SalarioBase = nuevo.SalarioDiario, TarifaPorHora = nuevo.TarifaPorHora, FechaInicioVigencia = inicio
        };
        db.HistorialSueldos.Add(sueldo);
        await db.SaveChangesAsync(ct);
        await transaccion.CommitAsync(ct);
        return sueldo.IdHistorialSueldo;
    }
}
