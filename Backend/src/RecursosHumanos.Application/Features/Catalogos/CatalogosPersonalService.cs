using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Catalogos;

public record DepartamentoDto(int IdDepartamento, string NombreDepartamento, int? IdResponsable, string? Responsable,
    string? GrupoDirectorio, bool Activo);

public record GuardarDepartamento([StringLength(100, MinimumLength = 2)] string NombreDepartamento, int? IdResponsable,
    [StringLength(100)] string? GrupoDirectorio);

public record PuestoDto(int IdPuesto, string Clave, string NombrePuesto, string? Descripcion, bool Activo);

public record GuardarPuesto([RegularExpression(Patrones.Clave, ErrorMessage = "La clave va sin espacios, de 1 a 20 caracteres.")] string Clave,
    [StringLength(100, MinimumLength = 2)] string NombrePuesto, [StringLength(300)] string? Descripcion);

public record PerfilDto(int IdPerfil, string NombrePerfil, bool Activo);

public record GuardarPerfil([StringLength(60, MinimumLength = 2)] string NombrePerfil);

public class CatalogosPersonalService(IRecursosHumanosDbContext db, IUsuarioActual usuario)
{
    private const string ElDepartamento = "el departamento", ElPuesto = "el puesto", ElPerfil = "el perfil";

    public async Task<List<DepartamentoDto>> DepartamentosAsync(bool incluirInactivos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.Departamentos.AsNoTracking().Where(d => incluirInactivos || d.Activo).OrderBy(d => d.NombreDepartamento)
            .Select(d => new DepartamentoDto(d.IdDepartamento, d.NombreDepartamento, d.IdResponsable,
                d.IdResponsableNavigation == null ? null
                    : (d.IdResponsableNavigation.IdPersonaNavigation.Nombre + " " + d.IdResponsableNavigation.IdPersonaNavigation.ApellidoPaterno).Trim(),
                d.GrupoDirectorio, d.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> CrearDepartamentoAsync(GuardarDepartamento datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var departamento = new Departamentos { Activo = true };
        await CopiarAsync(datos, departamento, ct);
        db.Departamentos.Add(departamento);
        await db.SaveChangesAsync(ct);
        return departamento.IdDepartamento;
    }

    public async Task ActualizarDepartamentoAsync(int id, GuardarDepartamento datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        await CopiarAsync(datos, await db.Departamentos.BuscarAsync(id, ElDepartamento, ct), ct);
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoDepartamentoAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return db.CambiarActivoAsync(db.Departamentos, id, activo, ElDepartamento,
            () => db.Empleados.AnyAsync(e => e.IdDepartamento == id && e.Activo && e.IdEstatusLaboralNavigation.Clave == EstatusLaboral.Activo, ct), ct);
    }

    public async Task<List<PuestoDto>> PuestosAsync(bool incluirInactivos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.Puestos.AsNoTracking().Where(p => incluirInactivos || p.Activo).OrderBy(p => p.NombrePuesto)
            .Select(p => new PuestoDto(p.IdPuesto, p.Clave, p.NombrePuesto, p.Descripcion, p.Activo)).ToListAsync(ct);
    }

    public async Task<int> CrearPuestoAsync(GuardarPuesto datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var puesto = new Puestos { Activo = true };
        Copiar(datos, puesto);
        db.Puestos.Add(puesto);
        await db.SaveChangesAsync(ct);
        return puesto.IdPuesto;
    }

    public async Task ActualizarPuestoAsync(int id, GuardarPuesto datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        Copiar(datos, await db.Puestos.BuscarAsync(id, ElPuesto, ct));
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoPuestoAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return db.CambiarActivoAsync(db.Puestos, id, activo, ElPuesto,
            () => db.Empleados.AnyAsync(e => e.IdPuesto == id && e.Activo && e.IdEstatusLaboralNavigation.Clave == EstatusLaboral.Activo, ct), ct);
    }

    public async Task<List<PerfilDto>> PerfilesAsync(bool incluirInactivos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.Perfiles.AsNoTracking().Where(p => incluirInactivos || p.Activo).OrderBy(p => p.NombrePerfil)
            .Select(p => new PerfilDto(p.IdPerfil, p.NombrePerfil, p.Activo)).ToListAsync(ct);
    }

    public async Task<int> CrearPerfilAsync(GuardarPerfil datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var perfil = new Perfiles { NombrePerfil = datos.NombrePerfil.Trim(), Activo = true };
        db.Perfiles.Add(perfil);
        await db.SaveChangesAsync(ct);
        return perfil.IdPerfil;
    }

    public async Task ActualizarPerfilAsync(int id, GuardarPerfil datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        (await db.Perfiles.BuscarAsync(id, ElPerfil, ct)).NombrePerfil = datos.NombrePerfil.Trim();
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoPerfilAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return db.CambiarActivoAsync(db.Perfiles, id, activo, ElPerfil,
            () => db.Empleados.AnyAsync(e => e.IdPerfil == id && e.Activo && e.IdEstatusLaboralNavigation.Clave == EstatusLaboral.Activo, ct), ct);
    }

    private async Task CopiarAsync(GuardarDepartamento d, Departamentos departamento, CancellationToken ct)
    {
        if (d.IdResponsable is int idResponsable
            && !await db.Empleados.AnyAsync(e => e.IdEmpleado == idResponsable && e.Activo && e.IdEstatusLaboralNavigation.Clave == EstatusLaboral.Activo, ct))
            throw Errores.Catalogos.ResponsableNoValido();
        departamento.NombreDepartamento = d.NombreDepartamento.Trim();
        departamento.IdResponsable = d.IdResponsable;
        departamento.GrupoDirectorio = d.GrupoDirectorio;
    }

    private static void Copiar(GuardarPuesto d, Puestos p)
    {
        p.Clave = d.Clave.ToUpper();
        p.NombrePuesto = d.NombrePuesto.Trim();
        p.Descripcion = d.Descripcion;
    }
}
