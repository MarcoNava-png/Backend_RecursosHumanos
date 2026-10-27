using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Common;

public static class Visibilidad
{
    public static IQueryable<Empleados> EmpleadosVisibles(this IRecursosHumanosDbContext db, IUsuarioActual usuario, string permiso)
    {
        var empleados = db.Empleados.Where(e => e.Activo);
        return usuario.Alcance(permiso) switch
        {
            Alcance.Todos => empleados,
            Alcance.Equipo => empleados.Where(e => e.IdJefeDirecto == usuario.IdEmpleado || e.IdEmpleado == usuario.IdEmpleado),
            Alcance.Propio => empleados.Where(e => e.IdEmpleado == usuario.IdEmpleado),
            _ => empleados.Where(e => false)
        };
    }

    public static async Task<IReadOnlyDictionary<string, string?>> PermisosDelUsuarioAsync(this IRecursosHumanosDbContext db,
        int idUsuario, CancellationToken ct) =>
        await db.Usuarios.AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario && u.Activo && u.EstatusUsuario == EstatusUsuario.Activo && !u.DebeCambiarContrasena
                        && u.IdRolNavigation.Activo)
            .SelectMany(u => u.IdRolNavigation.RolesPermisos)
            .Where(rp => rp.Activo && rp.IdPermisoNavigation.Activo)
            .Select(rp => new { rp.IdPermisoNavigation.Clave, rp.Alcance })
            .ToDictionaryAsync(rp => rp.Clave, rp => rp.Alcance, ct);
}
