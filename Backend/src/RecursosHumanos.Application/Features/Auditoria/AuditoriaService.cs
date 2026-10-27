using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;

namespace RecursosHumanos.Application.Features.Auditoria;

public record FiltroAuditoria(string? Tabla, string? IdRegistro, int? IdUsuario, string? Accion, DateOnly? Desde,
    DateOnly? Hasta, int NumeroPagina = 1, [Range(1, 200)] int Tamano = 50);

public record MovimientoDto(long IdLog, DateTime FechaAccion, int IdUsuario, string Usuario, string Tabla, string IdRegistro,
    string Accion, JsonElement? DatosPrevios, JsonElement? DatosNuevos, string? Ip);

public class AuditoriaService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj)
{
    private const int MaximoDiasRango = 93;

    public async Task<Pagina<MovimientoDto>> ListarAsync(FiltroAuditoria filtro, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.AuditoriaVer);
        var hasta = filtro.Hasta ?? reloj.Hoy;
        var desde = filtro.Desde ?? hasta.AddDays(-30);
        if (hasta < desde) throw Errores.Auditoria.RangoInvalido();
        if (hasta.DayNumber - desde.DayNumber + 1 > MaximoDiasRango) throw Errores.Auditoria.RangoMuyGrande(MaximoDiasRango);

        var inicio = reloj.InicioDelDiaUtc(desde);
        var fin = reloj.InicioDelDiaUtc(hasta.AddDays(1));
        var consulta = db.AuditoriaLogs.AsNoTracking().Where(l => l.FechaAccion >= inicio && l.FechaAccion < fin);
        if (filtro.Tabla is not null) consulta = consulta.Where(l => l.TablaAfectada == filtro.Tabla);
        if (filtro.IdRegistro is not null) consulta = consulta.Where(l => l.IdRegistro == filtro.IdRegistro);
        if (filtro.IdUsuario is not null) consulta = consulta.Where(l => l.IdUsuario == filtro.IdUsuario);
        if (filtro.Accion is not null) consulta = consulta.Where(l => l.Accion == filtro.Accion.ToUpper());

        var numeroPagina = Math.Max(filtro.NumeroPagina, 1);
        var filas = await consulta.OrderByDescending(l => l.IdLog)
            .Skip((numeroPagina - 1) * filtro.Tamano).Take(filtro.Tamano)
            .Select(l => new
            {
                l.IdLog, l.FechaAccion, l.IdUsuario, l.IdUsuarioNavigation.NombreUsuario, l.TablaAfectada, l.IdRegistro,
                l.Accion, l.JsonDatosPrevios, l.JsonDatosNuevos, l.IpOrigen
            })
            .ToListAsync(ct);
        var elementos = filas.Select(l => new MovimientoDto(l.IdLog, l.FechaAccion, l.IdUsuario, l.NombreUsuario, l.TablaAfectada,
            l.IdRegistro, l.Accion, Json(l.JsonDatosPrevios), Json(l.JsonDatosNuevos), l.IpOrigen)).ToList();
        return new Pagina<MovimientoDto>(elementos, await consulta.CountAsync(ct), numeroPagina, filtro.Tamano);
    }

    public async Task<List<string>> TablasAsync(CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.AuditoriaVer);
        return await db.AuditoriaLogs.AsNoTracking().Select(l => l.TablaAfectada).Distinct().OrderBy(t => t).ToListAsync(ct);
    }

    private static JsonElement? Json(string? texto) => texto is null ? null : JsonDocument.Parse(texto).RootElement.Clone();
}
