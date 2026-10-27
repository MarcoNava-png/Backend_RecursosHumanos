using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Horarios;

public record HorarioDto(int IdHistorialHorario, int IdEmpleado, short DiaSemana, TimeOnly HoraEntrada, TimeOnly HoraSalida,
    TimeOnly? HoraComidaInicio, TimeOnly? HoraComidaFin, int IdReglaAsistencia, string Regla, int? IdMateria,
    string? Materia, int? IdPeriodo, string? Periodo, DateOnly FechaInicioVigencia, DateOnly? FechaFinVigencia);

public record NuevoHorario(int IdEmpleado, int IdReglaAsistencia, [Range(1, 7)] short DiaSemana, TimeOnly HoraEntrada,
    TimeOnly HoraSalida, TimeOnly? HoraComidaInicio, TimeOnly? HoraComidaFin, int? IdMateria, int? IdPeriodo,
    DateOnly FechaInicioVigencia, DateOnly? FechaFinVigencia);

public record TerminarHorario(DateOnly FechaFinVigencia);

public class HorariosService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj)
{
    public async Task<List<HorarioDto>> ListarAsync(int idEmpleado, bool soloVigentes, CancellationToken ct)
    {
        if (!await db.EmpleadosVisibles(usuario, Permiso.HorariosVer).AnyAsync(e => e.IdEmpleado == idEmpleado, ct))
            throw Errores.Empleados.NoEncontrado(idEmpleado);
        var hoy = reloj.Hoy;
        return await db.HistorialHorarios.AsNoTracking()
            .Where(h => h.IdEmpleado == idEmpleado && h.Activo && (!soloVigentes || h.FechaFinVigencia == null || h.FechaFinVigencia >= hoy))
            .OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraEntrada).ThenBy(h => h.FechaInicioVigencia)
            .Select(h => new HorarioDto(h.IdHistorialHorario, h.IdEmpleado, h.DiaSemana, h.HoraEntrada, h.HoraSalida,
                h.HoraComidaInicio, h.HoraComidaFin, h.IdReglaAsistencia, h.IdReglaAsistenciaNavigation.NombreRegla,
                h.IdMateria, h.IdMateriaNavigation!.NombreMateria, h.IdPeriodo, h.IdPeriodoNavigation!.NombrePeriodo,
                h.FechaInicioVigencia, h.FechaFinVigencia))
            .ToListAsync(ct);
    }

    public async Task<int> CrearAsync(NuevoHorario nuevo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.HorariosEditar);
        if (!await db.Empleados.AnyAsync(e => e.IdEmpleado == nuevo.IdEmpleado && e.Activo, ct))
            throw Errores.Empleados.NoEncontrado(nuevo.IdEmpleado);
        var finNuevo = nuevo.FechaFinVigencia ?? DateOnly.MaxValue;
        var seCruza = await db.HistorialHorarios.AnyAsync(h => h.IdEmpleado == nuevo.IdEmpleado && h.Activo
            && h.DiaSemana == nuevo.DiaSemana
            && h.FechaInicioVigencia <= finNuevo && (h.FechaFinVigencia == null || h.FechaFinVigencia >= nuevo.FechaInicioVigencia)
            && h.HoraEntrada < nuevo.HoraSalida && h.HoraSalida > nuevo.HoraEntrada, ct);
        if (seCruza) throw Errores.Horarios.Traslape();

        var horario = new HistorialHorarios
        {
            IdEmpleado = nuevo.IdEmpleado, IdReglaAsistencia = nuevo.IdReglaAsistencia, DiaSemana = nuevo.DiaSemana,
            HoraEntrada = nuevo.HoraEntrada, HoraSalida = nuevo.HoraSalida, HoraComidaInicio = nuevo.HoraComidaInicio,
            HoraComidaFin = nuevo.HoraComidaFin, IdMateria = nuevo.IdMateria, IdPeriodo = nuevo.IdPeriodo,
            FechaInicioVigencia = nuevo.FechaInicioVigencia, FechaFinVigencia = nuevo.FechaFinVigencia
        };
        db.HistorialHorarios.Add(horario);
        await db.SaveChangesAsync(ct);
        return horario.IdHistorialHorario;
    }

    public async Task TerminarAsync(int id, TerminarHorario terminar, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.HorariosEditar);
        var horario = await db.HistorialHorarios.SingleOrDefaultAsync(h => h.IdHistorialHorario == id && h.Activo, ct)
                      ?? throw Errores.Horarios.NoEncontrado(id);
        horario.FechaFinVigencia = terminar.FechaFinVigencia;
        await db.SaveChangesAsync(ct);
    }
}
