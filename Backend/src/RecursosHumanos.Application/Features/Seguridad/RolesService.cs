using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Seguridad;

public record PermisoDto(string Clave, string Modulo, string Descripcion, bool UsaAlcance);

public record PermisoAsignadoDto([Required] string Permiso, string? Alcance);

public record RolDto(int IdRol, string Clave, string Nombre, int UsuariosActivos, List<PermisoAsignadoDto> Permisos);

public record NuevoRol(
    [RegularExpression("^[a-z][a-z0-9_]{2,29}$", ErrorMessage = "La clave va en minúsculas y sin espacios, de 3 a 30 caracteres.")] string Clave,
    [StringLength(60, MinimumLength = 3)] string Nombre);

public class RolesService(IRecursosHumanosDbContext db, IUsuarioActual usuario)
{
    private static readonly string[] Alcances = [Alcance.Todos, Alcance.Equipo, Alcance.Propio];

    public async Task<List<PermisoDto>> PermisosAsync(CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.RolesEditar);
        return await db.Permisos.AsNoTracking().Where(p => p.Activo).OrderBy(p => p.Modulo).ThenBy(p => p.Clave)
            .Select(p => new PermisoDto(p.Clave, p.Modulo, p.Descripcion, p.UsaAlcance)).ToListAsync(ct);
    }

    public async Task<List<RolDto>> RolesAsync(CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.RolesEditar);
        return await db.Roles.AsNoTracking().Where(r => r.Activo).OrderBy(r => r.IdRol)
            .Select(r => new RolDto(r.IdRol, r.Clave, r.NombreRol,
                r.Usuarios.Count(u => u.Activo && u.EstatusUsuario == EstatusUsuario.Activo),
                r.RolesPermisos.Where(rp => rp.Activo).OrderBy(rp => rp.IdPermisoNavigation.Clave)
                    .Select(rp => new PermisoAsignadoDto(rp.IdPermisoNavigation.Clave, rp.Alcance)).ToList()))
            .ToListAsync(ct);
    }

    public async Task<int> CrearAsync(NuevoRol nuevo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.RolesEditar);
        var rol = new Roles { Clave = nuevo.Clave, NombreRol = nuevo.Nombre.Trim() };
        db.Roles.Add(rol);
        await db.SaveChangesAsync(ct);
        return rol.IdRol;
    }

    public async Task AsignarPermisosAsync(int idRol, List<PermisoAsignadoDto> permisos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.RolesEditar);
        var rol = await db.Roles.Include(r => r.RolesPermisos).SingleOrDefaultAsync(r => r.IdRol == idRol && r.Activo, ct)
                  ?? throw Errores.Seguridad.RolNoEncontrado(idRol);
        var catalogo = await db.Permisos.Where(p => p.Activo).ToDictionaryAsync(p => p.Clave, ct);
        var nuevos = Validar(permisos, catalogo);
        await ValidarQueQuedeAdministradorAsync(rol, nuevos.ContainsKey(catalogo[Permiso.RolesEditar].IdPermiso), ct);

        foreach (var actual in rol.RolesPermisos.ToList())
        {
            if (nuevos.Remove(actual.IdPermiso, out var alcance))
            {
                actual.Alcance = alcance;
                actual.Activo = true;
            }
            else db.RolesPermisos.Remove(actual);
        }
        foreach (var (idPermiso, alcance) in nuevos)
            rol.RolesPermisos.Add(new RolesPermisos { IdPermiso = idPermiso, Alcance = alcance });
        await db.SaveChangesAsync(ct);
    }

    private static Dictionary<int, string?> Validar(List<PermisoAsignadoDto> permisos, Dictionary<string, Permisos> catalogo)
    {
        var resultado = new Dictionary<int, string?>();
        foreach (var p in permisos)
        {
            if (!catalogo.TryGetValue(p.Permiso, out var permiso)) throw Errores.Seguridad.PermisoNoExiste(p.Permiso);
            if (permiso.UsaAlcance && !Alcances.Contains(p.Alcance)) throw Errores.Seguridad.AlcanceRequerido(p.Permiso);
            if (!permiso.UsaAlcance && p.Alcance is not null) throw Errores.Seguridad.AlcanceNoAplica(p.Permiso);
            if (!resultado.TryAdd(permiso.IdPermiso, p.Alcance)) throw Errores.Seguridad.PermisoRepetido(p.Permiso);
        }
        return resultado;
    }

    private async Task ValidarQueQuedeAdministradorAsync(Roles rol, bool conservaPermiso, CancellationToken ct)
    {
        var otroRolLoTiene = await db.RolesPermisos.AnyAsync(rp => rp.IdRol != rol.IdRol && rp.Activo
            && rp.IdPermisoNavigation.Clave == Permiso.RolesEditar && rp.IdRolNavigation.Activo
            && rp.IdRolNavigation.Usuarios.Any(u => u.Activo && u.EstatusUsuario == EstatusUsuario.Activo), ct);
        var esteRolTieneUsuarios = await db.Usuarios.AnyAsync(u => u.IdRol == rol.IdRol && u.Activo && u.EstatusUsuario == EstatusUsuario.Activo, ct);
        if (!otroRolLoTiene && !(conservaPermiso && esteRolTieneUsuarios)) throw Errores.Seguridad.SinAdministradorDeRoles();
    }
}
