using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Application.Features.Seguridad;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;
using Empleado = RecursosHumanos.Domain.Entities.Empleados;

namespace RecursosHumanos.Application.Features.Empleados;

public class EmpleadosService(IRecursosHumanosDbContext db, IUsuarioActual usuario, IReloj reloj, UsuariosService usuarios)
{
    public async Task<Pagina<EmpleadoDto>> ListarAsync(FiltroEmpleados filtro, CancellationToken ct)
    {
        var consulta = Visibles();
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var texto = filtro.Busqueda.Trim().ToLower();
            consulta = consulta.Where(e => (e.IdPersonaNavigation.Nombre + " " + e.IdPersonaNavigation.ApellidoPaterno + " " +
                                            (e.IdPersonaNavigation.ApellidoMaterno ?? "")).ToLower().Contains(texto)
                                           || (e.Matricula ?? "").ToLower().Contains(texto));
        }
        if (filtro.IdDepartamento is not null) consulta = consulta.Where(e => e.IdDepartamento == filtro.IdDepartamento);
        if (filtro.IdCampus is not null) consulta = consulta.Where(e => e.IdCampus == filtro.IdCampus);
        if (filtro.IdEstatusLaboral is not null) consulta = consulta.Where(e => e.IdEstatusLaboral == filtro.IdEstatusLaboral);

        var numeroPagina = Math.Max(filtro.NumeroPagina, 1);
        var elementos = await consulta
            .OrderBy(e => e.IdPersonaNavigation.ApellidoPaterno)
            .ThenBy(e => e.IdPersonaNavigation.ApellidoMaterno)
            .ThenBy(e => e.IdPersonaNavigation.Nombre)
            .Skip((numeroPagina - 1) * filtro.Tamano)
            .Take(filtro.Tamano)
            .Select(e => new EmpleadoDto(
                e.IdEmpleado,
                (e.IdPersonaNavigation.Nombre + " " + e.IdPersonaNavigation.ApellidoPaterno + " " +
                 (e.IdPersonaNavigation.ApellidoMaterno ?? "")).Trim(),
                e.Matricula,
                e.IdDepartamentoNavigation.NombreDepartamento,
                e.IdPerfilNavigation.NombrePerfil,
                e.IdPuestoNavigation!.NombrePuesto,
                e.IdCampusNavigation!.NombreCampus,
                e.IdEstatusLaboralNavigation.Descripcion))
            .ToListAsync(ct);
        return new Pagina<EmpleadoDto>(elementos, await consulta.CountAsync(ct), numeroPagina, filtro.Tamano);
    }

    public async Task<EmpleadoDetalleDto> ObtenerAsync(int id, CancellationToken ct) =>
        await Visibles()
            .Where(e => e.IdEmpleado == id)
            .Select(e => new EmpleadoDetalleDto(
                e.IdEmpleado, e.IdPersonaNavigation.Nombre, e.IdPersonaNavigation.ApellidoPaterno,
                e.IdPersonaNavigation.ApellidoMaterno, e.IdPersonaNavigation.Curp, e.IdPersonaNavigation.Rfc,
                e.IdPersonaNavigation.CorreoElectronico, e.IdPersonaNavigation.Telefono, e.IdEmpresa, e.IdDepartamento,
                e.IdPerfil, e.IdPuesto, e.IdCampus, e.IdJefeDirecto, e.NumeroChecador, e.Matricula, e.FechaIngreso,
                e.IdEstatusLaboral, e.ExtensionTelefono, e.AvisoPrivacidadBiometricos, e.FechaFirmaAviso))
            .SingleOrDefaultAsync(ct)
        ?? throw Errores.Empleados.NoEncontrado(id);

    public async Task<EmpleadoCreadoDto> CrearAsync(GuardarEmpleadoSolicitud solicitud, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.EmpleadosEditar);
        await ValidarAsync(solicitud, null, ct);

        var persona = solicitud.Curp is null ? null : await db.Personas.SingleOrDefaultAsync(p => p.Curp == solicitud.Curp, ct);
        if (persona is not null && await db.Empleados.AnyAsync(e => e.IdPersona == persona.IdPersona && e.IdEmpresa == solicitud.IdEmpresa, ct))
            throw Errores.Empleados.YaRegistradoEnEmpresa();

        var empleado = new Empleado
        {
            IdPersonaNavigation = persona ?? new Personas(),
            IdEmpresa = solicitud.IdEmpresa,
            IdEstatusLaboral = await db.EstatusLaborales
                .Where(x => x.Clave == EstatusLaboral.Activo).Select(x => x.IdEstatusLaboral).SingleAsync(ct)
        };
        Copiar(solicitud, empleado);
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync(ct);
        return new EmpleadoCreadoDto(empleado.IdEmpleado, await AccesoAsync(empleado.IdEmpleado, solicitud.IdRolAcceso, ct));
    }

    private async Task<AccesoEmpleadoDto> AccesoAsync(int idEmpleado, int? idRol, CancellationToken ct)
    {
        try
        {
            return await usuarios.CrearAccesoEmpleadoAsync(idEmpleado, idRol, ct);
        }
        catch (ErrorAplicacionException e)
        {
            return new AccesoEmpleadoDto(null, null, null, e.Message);
        }
        catch (DbUpdateException)
        {
            return new AccesoEmpleadoDto(null, null, null, "No se pudo crear el usuario; revisa que no exista otro con el mismo correo.");
        }
    }

    public async Task ActualizarAsync(int id, GuardarEmpleadoSolicitud solicitud, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.EmpleadosEditar);
        var empleado = await db.Empleados.Include(e => e.IdPersonaNavigation)
                           .SingleOrDefaultAsync(e => e.IdEmpleado == id && e.Activo, ct)
                       ?? throw Errores.Empleados.NoEncontrado(id);
        if (empleado.IdEmpresa != solicitud.IdEmpresa) throw Errores.Empleados.CambioDeEmpresa();
        await ValidarAsync(solicitud, id, ct);

        Copiar(solicitud, empleado);
        await db.SaveChangesAsync(ct);
    }

    private IQueryable<Empleado> Visibles() => db.EmpleadosVisibles(usuario, Permiso.EmpleadosVer).AsNoTracking();

    private async Task ValidarAsync(GuardarEmpleadoSolicitud solicitud, int? idEmpleado, CancellationToken ct)
    {
        if (solicitud.NumeroChecador is not null && !solicitud.AvisoPrivacidadBiometricos)
            throw Errores.Empleados.ChecadorSinAviso();
        if (solicitud.IdJefeDirecto is not null
            && (solicitud.IdJefeDirecto == idEmpleado
                || !await db.Empleados.AnyAsync(e => e.IdEmpleado == solicitud.IdJefeDirecto && e.Activo, ct)))
            throw Errores.Empleados.JefeNoValido();
    }

    private void Copiar(GuardarEmpleadoSolicitud s, Empleado e)
    {
        var p = e.IdPersonaNavigation;
        p.Nombre = s.Nombre.Trim();
        p.ApellidoPaterno = s.ApellidoPaterno.Trim();
        p.ApellidoMaterno = s.ApellidoMaterno?.Trim();
        p.Curp = s.Curp?.ToUpper();
        p.Rfc = s.Rfc?.ToUpper();
        p.CorreoElectronico = s.CorreoElectronico?.ToLower();
        p.Telefono = s.Telefono;

        if (s.AvisoPrivacidadBiometricos && !e.AvisoPrivacidadBiometricos) e.FechaFirmaAviso = reloj.AhoraUtc;
        e.AvisoPrivacidadBiometricos = s.AvisoPrivacidadBiometricos;
        e.IdDepartamento = s.IdDepartamento;
        e.IdPerfil = s.IdPerfil;
        e.IdPuesto = s.IdPuesto;
        e.IdCampus = s.IdCampus;
        e.IdJefeDirecto = s.IdJefeDirecto;
        e.NumeroChecador = s.NumeroChecador;
        e.Matricula = s.Matricula;
        e.FechaIngreso = s.FechaIngreso;
        e.ExtensionTelefono = s.ExtensionTelefono;
    }
}
