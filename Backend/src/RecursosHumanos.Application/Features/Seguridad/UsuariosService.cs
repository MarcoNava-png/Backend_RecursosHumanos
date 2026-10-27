using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Seguridad;

public record FiltroUsuarios(string? Busqueda, int? IdRol, string? Estatus, int NumeroPagina = 1, [Range(1, 200)] int Tamano = 50);

public record UsuarioDto(int IdUsuario, string NombreUsuario, int? IdEmpleado, string? Empleado, int IdRol, string Rol,
    string Proveedor, string Estatus, DateTime? UltimoAcceso, DateTime? BloqueadoHasta, bool DebeCambiarContrasena);

public record NuevoUsuario([StringLength(100, MinimumLength = 3)] string NombreUsuario, int IdRol, int? IdEmpleado,
    [RegularExpression("^(local|entra_id)$", ErrorMessage = "El proveedor debe ser local o entra_id.")] string Proveedor = ProveedorAutenticacion.EntraId);

public record ActualizarUsuario(int IdRol,
    [RegularExpression("^(activo|bloqueado|inactivo)$", ErrorMessage = "El estatus debe ser activo, bloqueado o inactivo.")] string Estatus);

public record UsuarioCreadoDto(int IdUsuario, string NombreUsuario, string? ContrasenaTemporal);

public record ContrasenaTemporalDto(string ContrasenaTemporal);

public record AccesoEmpleadoDto(int? IdUsuario, string? NombreUsuario, string? CuentaWindows, string? Pendiente);

public class UsuariosService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IProcesosBd procesos)
{
    private static readonly Dictionary<string, int> Nivel = new() { [Alcance.Propio] = 1, [Alcance.Equipo] = 2, [Alcance.Todos] = 3 };

    public async Task<Pagina<UsuarioDto>> ListarAsync(FiltroUsuarios filtro, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.UsuariosVer);
        var consulta = db.Usuarios.AsNoTracking().Where(u => u.Activo);
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var texto = filtro.Busqueda.Trim().ToLower();
            consulta = consulta.Where(u => u.NombreUsuario.ToLower().Contains(texto)
                || (u.IdEmpleadoNavigation!.IdPersonaNavigation.Nombre + " " + u.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno).ToLower().Contains(texto));
        }
        if (filtro.IdRol is not null) consulta = consulta.Where(u => u.IdRol == filtro.IdRol);
        if (filtro.Estatus is not null) consulta = consulta.Where(u => u.EstatusUsuario == filtro.Estatus);

        var numeroPagina = Math.Max(filtro.NumeroPagina, 1);
        var elementos = await consulta.OrderBy(u => u.NombreUsuario).Skip((numeroPagina - 1) * filtro.Tamano).Take(filtro.Tamano)
            .Select(u => new UsuarioDto(u.IdUsuario, u.NombreUsuario, u.IdEmpleado,
                u.IdEmpleadoNavigation == null ? null
                    : (u.IdEmpleadoNavigation.IdPersonaNavigation.Nombre + " " + u.IdEmpleadoNavigation.IdPersonaNavigation.ApellidoPaterno).Trim(),
                u.IdRol, u.IdRolNavigation.NombreRol, u.IdProveedorAutenticacionNavigation.Clave, u.EstatusUsuario, u.UltimoAcceso,
                u.BloqueadoHasta, u.DebeCambiarContrasena))
            .ToListAsync(ct);
        return new Pagina<UsuarioDto>(elementos, await consulta.CountAsync(ct), numeroPagina, filtro.Tamano);
    }

    public async Task<UsuarioCreadoDto> CrearAsync(NuevoUsuario nuevo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.UsuariosEditar);
        await ValidarPuedeAsignarRolAsync(nuevo.IdRol, ct);
        if (nuevo.IdEmpleado is int idEmpleado)
        {
            if (!await db.Empleados.AnyAsync(e => e.IdEmpleado == idEmpleado && e.Activo, ct)) throw Errores.Empleados.NoEncontrado(idEmpleado);
            if (await db.Usuarios.AnyAsync(u => u.IdEmpleado == idEmpleado, ct)) throw Errores.Seguridad.EmpleadoYaTieneUsuario();
        }

        var esLocal = nuevo.Proveedor == ProveedorAutenticacion.Local;
        var temporal = esLocal ? Contrasenas.Temporal() : null;
        var creado = new Usuarios
        {
            NombreUsuario = esLocal ? nuevo.NombreUsuario.Trim() : nuevo.NombreUsuario.Trim().ToLower(), IdRol = nuevo.IdRol,
            IdEmpleado = nuevo.IdEmpleado, EstatusUsuario = EstatusUsuario.Activo,
            IdProveedorAutenticacion = await IdProveedorAsync(nuevo.Proveedor, ct),
            PasswordHash = temporal is null ? null : Contrasenas.Hash(temporal), DebeCambiarContrasena = esLocal
        };
        db.Usuarios.Add(creado);
        await db.SaveChangesAsync(ct);
        return new UsuarioCreadoDto(creado.IdUsuario, creado.NombreUsuario, temporal);
    }

    public async Task ActualizarAsync(int idUsuario, ActualizarUsuario datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.UsuariosEditar);
        var objetivo = await CargarParaEditarAsync(idUsuario, ct);
        if (objetivo.IdRol != datos.IdRol) await ValidarPuedeAsignarRolAsync(datos.IdRol, ct);

        objetivo.IdRol = datos.IdRol;
        objetivo.EstatusUsuario = datos.Estatus;
        if (datos.Estatus == EstatusUsuario.Activo)
        {
            objetivo.IntentosFallidos = 0;
            objetivo.BloqueadoHasta = null;
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<ContrasenaTemporalDto> RestablecerContrasenaAsync(int idUsuario, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.UsuariosEditar);
        var objetivo = await CargarParaEditarAsync(idUsuario, ct);
        if (objetivo.IdProveedorAutenticacionNavigation.Clave != ProveedorAutenticacion.Local) throw Errores.Seguridad.SoloLocal();

        var temporal = Contrasenas.Temporal();
        objetivo.PasswordHash = Contrasenas.Hash(temporal);
        objetivo.DebeCambiarContrasena = true;
        objetivo.IntentosFallidos = 0;
        objetivo.BloqueadoHasta = null;
        await db.SaveChangesAsync(ct);
        return new ContrasenaTemporalDto(temporal);
    }

    public Task<AccesoEmpleadoDto> ReintentarAccesoAsync(int idEmpleado, int? idRol, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.UsuariosEditar);
        return CrearAccesoEmpleadoAsync(idEmpleado, idRol, ct);
    }

    public async Task<AccesoEmpleadoDto> CrearAccesoEmpleadoAsync(int idEmpleado, int? idRol, CancellationToken ct)
    {
        if (!usuario.TienePermiso(Permiso.UsuariosEditar)) return Pendiente("No tienes permiso para crear usuarios.");
        var empleado = await db.Empleados.Include(e => e.IdEmpresaNavigation).Include(e => e.Usuarios)
                           .SingleOrDefaultAsync(e => e.IdEmpleado == idEmpleado && e.Activo, ct)
                       ?? throw Errores.Empleados.NoEncontrado(idEmpleado);
        if (empleado.Usuarios is { } existente)
            return new AccesoEmpleadoDto(existente.IdUsuario, existente.NombreUsuario, null, "El empleado ya tenía usuario.");
        if (empleado.IdEmpresaNavigation.DominioCorreo is null)
            return Pendiente("La empresa no tiene dominio de correo configurado; captúralo en el catálogo de empresas y vuelve a intentar.");

        var rol = idRol ?? await db.Roles.Where(r => r.Clave == ClaveRol.Empleado).Select(r => r.IdRol).SingleAsync(ct);
        await ValidarPuedeAsignarRolAsync(rol, ct);
        await procesos.PrepararCuentasDirectorioAsync(ct);
        var cuenta = await db.CuentasDirectorio.AsNoTracking().SingleOrDefaultAsync(c => c.IdEmpleado == idEmpleado && c.Activo, ct);
        if (cuenta is null) return Pendiente("No se pudo preparar la cuenta de directorio del empleado.");

        var creado = new Usuarios
        {
            NombreUsuario = cuenta.UserPrincipalName, IdEmpleado = idEmpleado, IdRol = rol, EstatusUsuario = EstatusUsuario.Activo,
            IdProveedorAutenticacion = await IdProveedorAsync(ProveedorAutenticacion.EntraId, ct)
        };
        db.Usuarios.Add(creado);
        await db.SaveChangesAsync(ct);
        return new AccesoEmpleadoDto(creado.IdUsuario, creado.NombreUsuario, cuenta.SamAccountName, null);
    }

    private static AccesoEmpleadoDto Pendiente(string motivo) => new(null, null, null, motivo);

    private async Task<Usuarios> CargarParaEditarAsync(int idUsuario, CancellationToken ct)
    {
        if (idUsuario == usuario.IdUsuario) throw Errores.Seguridad.PropioUsuario();
        var objetivo = await db.Usuarios.Include(u => u.IdProveedorAutenticacionNavigation)
                           .SingleOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Activo, ct)
                       ?? throw Errores.Seguridad.UsuarioNoEncontrado(idUsuario);
        await ValidarPuedeAsignarRolAsync(objetivo.IdRol, ct);
        return objetivo;
    }

    private async Task ValidarPuedeAsignarRolAsync(int idRol, CancellationToken ct)
    {
        if (!await db.Roles.AnyAsync(r => r.IdRol == idRol && r.Activo, ct)) throw Errores.Seguridad.RolNoEncontrado(idRol);
        if (usuario.TienePermiso(Permiso.RolesEditar)) return;
        var permisosRol = await db.RolesPermisos.Where(rp => rp.IdRol == idRol && rp.Activo && rp.IdPermisoNavigation.Activo)
            .Select(rp => new { rp.IdPermisoNavigation.Clave, rp.Alcance }).ToListAsync(ct);
        var excede = permisosRol.Any(p => !usuario.TienePermiso(p.Clave)
            || (p.Alcance is not null && Nivel.GetValueOrDefault(usuario.Alcance(p.Clave) ?? "") < Nivel[p.Alcance]));
        if (excede) throw Errores.Seguridad.RolSuperior();
    }

    private Task<int> IdProveedorAsync(string clave, CancellationToken ct) =>
        db.ProveedoresAutenticacion.Where(p => p.Clave == clave).Select(p => p.IdProveedorAutenticacion).SingleAsync(ct);
}
