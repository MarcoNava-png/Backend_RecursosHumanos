using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Application.Features.Seguridad;
using RecursosHumanos.Domain.Constantes;

namespace RecursosHumanos.Application.Features.Auth;

public class AuthService(IRecursosHumanosDbContext db, IGeneradorToken generadorToken, IReloj reloj, IUsuarioActual usuarioActual,
    PoliticaAcceso politica, IValidadorEntraId entraId)
{
    public async Task<LoginRespuesta> IniciarSesionAsync(LoginSolicitud solicitud, CancellationToken ct)
    {
        var usuario = await db.Usuarios
            .Include(u => u.IdRolNavigation)
            .SingleOrDefaultAsync(u => u.NombreUsuario == solicitud.NombreUsuario
                && u.Activo
                && u.EstatusUsuario == EstatusUsuario.Activo
                && u.IdProveedorAutenticacionNavigation.Clave == ProveedorAutenticacion.Local, ct);
        if (usuario?.PasswordHash is null) throw Errores.Auth.CredencialesInvalidas();

        var ahora = reloj.AhoraUtc;
        if (usuario.BloqueadoHasta > ahora)
            throw Errores.Auth.Bloqueado((int)Math.Ceiling((usuario.BloqueadoHasta.Value - ahora).TotalMinutes));

        if (!BCrypt.Net.BCrypt.Verify(solicitud.Contrasena, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= politica.IntentosMaximos)
            {
                usuario.BloqueadoHasta = ahora.AddMinutes(politica.MinutosBloqueo);
                usuario.IntentosFallidos = 0;
            }
            await db.SaveChangesAsync(ct);
            throw Errores.Auth.CredencialesInvalidas();
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.UltimoAcceso = ahora;
        await db.SaveChangesAsync(ct);

        var (token, expira) = generadorToken.Generar(usuario.IdUsuario, usuario.IdEmpleado, usuario.NombreUsuario, usuario.IdRolNavigation.Clave);
        return new LoginRespuesta(token, expira, await SesionAsync(usuario.IdUsuario, ct));
    }

    public async Task<LoginRespuesta> IniciarSesionEntraAsync(LoginEntraSolicitud solicitud, CancellationToken ct)
    {
        var identidad = await entraId.ValidarAsync(solicitud.Token, ct);
        var upn = identidad.Upn.ToLower();
        var usuario = await db.Usuarios
            .Include(u => u.IdRolNavigation)
            .Where(u => u.Activo && u.EstatusUsuario == EstatusUsuario.Activo
                && u.IdProveedorAutenticacionNavigation.Clave == ProveedorAutenticacion.EntraId)
            .SingleOrDefaultAsync(u => u.IdExterno == identidad.Oid
                || (u.IdExterno == null && u.NombreUsuario.ToLower() == upn), ct)
            ?? throw Errores.Auth.SinAccesoEntra();

        usuario.IdExterno = identidad.Oid;
        usuario.UltimoAcceso = reloj.AhoraUtc;
        await db.SaveChangesAsync(ct);

        var (token, expira) = generadorToken.Generar(usuario.IdUsuario, usuario.IdEmpleado, usuario.NombreUsuario, usuario.IdRolNavigation.Clave);
        return new LoginRespuesta(token, expira, await SesionAsync(usuario.IdUsuario, ct));
    }

    public ConfiguracionEntraId ConfiguracionEntra() => entraId.Configuracion;

    public Task<SesionDto> YoAsync(CancellationToken ct) =>
        SesionAsync(usuarioActual.IdUsuario ?? throw Errores.General.SinPermiso(), ct);

    public async Task CambiarContrasenaAsync(CambioContrasena cambio, CancellationToken ct)
    {
        var idUsuario = usuarioActual.IdUsuario ?? throw Errores.General.SinPermiso();
        var usuario = await db.Usuarios.Include(u => u.IdProveedorAutenticacionNavigation).SingleAsync(u => u.IdUsuario == idUsuario, ct);
        if (usuario.IdProveedorAutenticacionNavigation.Clave != ProveedorAutenticacion.Local || usuario.PasswordHash is null)
            throw Errores.Seguridad.SoloLocal();
        if (!BCrypt.Net.BCrypt.Verify(cambio.Actual, usuario.PasswordHash)) throw Errores.Seguridad.ContrasenaActualIncorrecta();
        if (!Contrasenas.CumplePolitica(cambio.Nueva) || cambio.Nueva == cambio.Actual) throw Errores.Seguridad.ContrasenaDebil();

        usuario.PasswordHash = Contrasenas.Hash(cambio.Nueva);
        usuario.DebeCambiarContrasena = false;
        usuario.FechaCambioContrasena = reloj.AhoraUtc;
        await db.SaveChangesAsync(ct);
    }

    private async Task<SesionDto> SesionAsync(int idUsuario, CancellationToken ct)
    {
        var usuario = await db.Usuarios.AsNoTracking().Where(u => u.IdUsuario == idUsuario)
            .Select(u => new { u.IdUsuario, u.IdEmpleado, u.NombreUsuario, u.IdRolNavigation.Clave, u.IdRolNavigation.NombreRol, u.DebeCambiarContrasena })
            .SingleAsync(ct);
        var permisos = await db.PermisosDelUsuarioAsync(idUsuario, ct);
        return new SesionDto(usuario.IdUsuario, usuario.IdEmpleado, usuario.NombreUsuario, usuario.Clave, usuario.NombreRol,
            usuario.DebeCambiarContrasena, permisos.OrderBy(p => p.Key).Select(p => new PermisoAsignadoDto(p.Key, p.Value)).ToList());
    }
}
