using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Features.Catalogos;

public static class Patrones
{
    public const string Rfc = "(?i)^[A-ZÑ&]{3,4}[0-9]{6}[A-Z0-9]{3}$";
    public const string CodigoPostal = "^[0-9]{5}$";
    public const string Dominio = "(?i)^[a-z0-9-]+(\\.[a-z0-9-]+)+$";
    public const string Ou = "(?i)^(OU|CN)=[^,]+(,(OU|CN|DC)=[^,]+)*,DC=[^,]+$";
    public const string Clave = "(?i)^[A-Z0-9_-]{1,20}$";
}

public record EmpresaDto(int IdEmpresa, string RazonSocial, string NombreComercial, string Rfc, int IdRegimenFiscal, string RegimenFiscal,
    string CodigoPostalExpedicion, string? DominioCorreo, string? OuDirectorio, string? NumeroCertificadoCsd, string? RutaCertificadoCsd,
    DateOnly? VigenciaCsd, string? ProveedorPac, string? SerieRecibos, bool Activo);

public record GuardarEmpresa(
    [StringLength(200, MinimumLength = 3)] string RazonSocial,
    [StringLength(100, MinimumLength = 2)] string NombreComercial,
    [RegularExpression(Patrones.Rfc, ErrorMessage = "El RFC no es válido.")] string Rfc,
    int IdRegimenFiscal,
    [RegularExpression(Patrones.CodigoPostal, ErrorMessage = "El código postal debe tener 5 dígitos.")] string CodigoPostalExpedicion,
    [RegularExpression(Patrones.Dominio, ErrorMessage = "El dominio de correo no es válido (por ejemplo usag.edu.mx).")] string? DominioCorreo,
    [RegularExpression(Patrones.Ou, ErrorMessage = "La OU no es válida (por ejemplo OU=USAG,DC=ad,DC=usag,DC=edu,DC=mx).")] string? OuDirectorio,
    [StringLength(20)] string? NumeroCertificadoCsd,
    [StringLength(300)] string? RutaCertificadoCsd,
    DateOnly? VigenciaCsd,
    [StringLength(50)] string? ProveedorPac,
    [StringLength(10)] string? SerieRecibos);

public record CampusDto(int IdCampus, int IdEmpresa, string Empresa, string Clave, string NombreCampus, string? CalleNumero, string? Ciudad,
    int? IdEstado, string? Estado, string? CodigoPostal, string? Telefono, string? OuDirectorio, bool Activo);

public record GuardarCampus(
    int IdEmpresa,
    [RegularExpression(Patrones.Clave, ErrorMessage = "La clave va sin espacios, de 1 a 20 caracteres.")] string Clave,
    [StringLength(100, MinimumLength = 2)] string NombreCampus,
    [StringLength(200)] string? CalleNumero,
    [StringLength(100)] string? Ciudad,
    int? IdEstado,
    [RegularExpression(Patrones.CodigoPostal, ErrorMessage = "El código postal debe tener 5 dígitos.")] string? CodigoPostal,
    [StringLength(20)] string? Telefono,
    [RegularExpression(Patrones.Ou, ErrorMessage = "La OU no es válida.")] string? OuDirectorio);

public record RegistroPatronalDto(int IdRegistroPatronal, int IdEmpresa, string Empresa, string RegistroPatronal, int IdClaseRiesgo,
    string ClaseRiesgo, int IdZonaSalarioMinimo, string ZonaSalarioMinimo, int IdEstado, string Estado, bool Activo);

public record GuardarRegistroPatronal(
    int IdEmpresa,
    [RegularExpression("(?i)^[A-Z0-9]{11}$", ErrorMessage = "El registro patronal tiene 11 caracteres.")] string RegistroPatronal,
    int IdClaseRiesgo,
    int IdZonaSalarioMinimo,
    int IdEstado);

public record EsquemaPrestacionesDto(int IdEsquemaPrestaciones, int IdEmpresa, string Empresa, string NombreEsquema, int DiasAguinaldo,
    decimal PorcentajePrimaVacacional, decimal PorcentajePrimaDominical, bool DescontarFaltasAguinaldo, bool Activo);

public record GuardarEsquemaPrestaciones(
    int IdEmpresa,
    [StringLength(100, MinimumLength = 2)] string NombreEsquema,
    [Range(15, 365, ErrorMessage = "El aguinaldo es de al menos 15 días.")] int DiasAguinaldo,
    [Range(25, 100, ErrorMessage = "La prima vacacional es de al menos 25%.")] decimal PorcentajePrimaVacacional,
    [Range(25, 100, ErrorMessage = "La prima dominical es de al menos 25%.")] decimal PorcentajePrimaDominical,
    bool DescontarFaltasAguinaldo);

public class EmpresasService(IRecursosHumanosDbContext db, IUsuarioActual usuario)
{
    private const string LaEmpresa = "la empresa", ElCampus = "el campus", ElRegistro = "el registro patronal", ElEsquema = "el esquema de prestaciones";

    public async Task<List<EmpresaDto>> EmpresasAsync(bool incluirInactivos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.Empresas.AsNoTracking().Where(e => incluirInactivos || e.Activo).OrderBy(e => e.NombreComercial)
            .Select(e => new EmpresaDto(e.IdEmpresa, e.RazonSocial, e.NombreComercial, e.Rfc, e.IdRegimenFiscal,
                e.IdRegimenFiscalNavigation.Clave + " " + e.IdRegimenFiscalNavigation.Descripcion, e.CodigoPostalExpedicion, e.DominioCorreo,
                e.OuDirectorio, e.NumeroCertificadoCsd, e.RutaCertificadoCsd, e.VigenciaCsd, e.ProveedorPac, e.SerieRecibos, e.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> CrearEmpresaAsync(GuardarEmpresa datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var empresa = new Empresas { FolioSiguiente = 1, Activo = true };
        await CopiarAsync(datos, empresa, ct);
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync(ct);
        return empresa.IdEmpresa;
    }

    public async Task ActualizarEmpresaAsync(int id, GuardarEmpresa datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        await CopiarAsync(datos, await db.Empresas.BuscarAsync(id, LaEmpresa, ct), ct);
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoEmpresaAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return db.CambiarActivoAsync(db.Empresas, id, activo, LaEmpresa,
            () => db.Empleados.AnyAsync(e => e.IdEmpresa == id && e.Activo && e.IdEstatusLaboralNavigation.Clave == EstatusLaboral.Activo, ct), ct);
    }

    public async Task<List<CampusDto>> CampusAsync(bool incluirInactivos, int? idEmpresa, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.Campus.AsNoTracking().Where(c => (incluirInactivos || c.Activo) && (idEmpresa == null || c.IdEmpresa == idEmpresa))
            .OrderBy(c => c.NombreCampus)
            .Select(c => new CampusDto(c.IdCampus, c.IdEmpresa, c.IdEmpresaNavigation.NombreComercial, c.Clave, c.NombreCampus, c.CalleNumero,
                c.Ciudad, c.IdEstado, c.IdEstadoNavigation!.Descripcion, c.CodigoPostal, c.Telefono, c.OuDirectorio, c.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> CrearCampusAsync(GuardarCampus datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var campus = new Campus { Activo = true };
        Copiar(datos, campus);
        db.Campus.Add(campus);
        await db.SaveChangesAsync(ct);
        return campus.IdCampus;
    }

    public async Task ActualizarCampusAsync(int id, GuardarCampus datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        Copiar(datos, await db.Campus.BuscarAsync(id, ElCampus, ct));
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoCampusAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return db.CambiarActivoAsync(db.Campus, id, activo, ElCampus,
            () => db.Empleados.AnyAsync(e => e.IdCampus == id && e.Activo && e.IdEstatusLaboralNavigation.Clave == EstatusLaboral.Activo, ct), ct);
    }

    public async Task<List<RegistroPatronalDto>> RegistrosPatronalesAsync(bool incluirInactivos, int? idEmpresa, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.RegistrosPatronales.AsNoTracking()
            .Where(r => (incluirInactivos || r.Activo) && (idEmpresa == null || r.IdEmpresa == idEmpresa))
            .OrderBy(r => r.RegistroPatronal)
            .Select(r => new RegistroPatronalDto(r.IdRegistroPatronal, r.IdEmpresa, r.IdEmpresaNavigation.NombreComercial, r.RegistroPatronal,
                r.IdClaseRiesgo, r.IdClaseRiesgoNavigation.Descripcion, r.IdZonaSalarioMinimo, r.IdZonaSalarioMinimoNavigation.Descripcion,
                r.IdEstado, r.IdEstadoNavigation.Descripcion, r.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> CrearRegistroPatronalAsync(GuardarRegistroPatronal datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var registro = new RegistrosPatronales { Activo = true };
        Copiar(datos, registro);
        db.RegistrosPatronales.Add(registro);
        await db.SaveChangesAsync(ct);
        return registro.IdRegistroPatronal;
    }

    public async Task ActualizarRegistroPatronalAsync(int id, GuardarRegistroPatronal datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        Copiar(datos, await db.RegistrosPatronales.BuscarAsync(id, ElRegistro, ct));
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoRegistroPatronalAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return db.CambiarActivoAsync(db.RegistrosPatronales, id, activo, ElRegistro,
            () => db.EmpleadosNomina.AnyAsync(n => n.IdRegistroPatronal == id && n.Activo && n.IdEmpleadoNavigation.Activo, ct), ct);
    }

    public async Task<List<EsquemaPrestacionesDto>> EsquemasAsync(bool incluirInactivos, int? idEmpresa, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return await db.EsquemasPrestaciones.AsNoTracking()
            .Where(e => (incluirInactivos || e.Activo) && (idEmpresa == null || e.IdEmpresa == idEmpresa))
            .OrderBy(e => e.NombreEsquema)
            .Select(e => new EsquemaPrestacionesDto(e.IdEsquemaPrestaciones, e.IdEmpresa, e.IdEmpresaNavigation.NombreComercial, e.NombreEsquema,
                e.DiasAguinaldo, e.PorcentajePrimaVacacional, e.PorcentajePrimaDominical, e.DescontarFaltasAguinaldo, e.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> CrearEsquemaAsync(GuardarEsquemaPrestaciones datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        var esquema = new EsquemasPrestaciones { Activo = true };
        Copiar(datos, esquema);
        db.EsquemasPrestaciones.Add(esquema);
        await db.SaveChangesAsync(ct);
        return esquema.IdEsquemaPrestaciones;
    }

    public async Task ActualizarEsquemaAsync(int id, GuardarEsquemaPrestaciones datos, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        Copiar(datos, await db.EsquemasPrestaciones.BuscarAsync(id, ElEsquema, ct));
        await db.SaveChangesAsync(ct);
    }

    public Task CambiarActivoEsquemaAsync(int id, bool activo, CancellationToken ct)
    {
        usuario.ExigirPermiso(Permiso.CatalogosEditar);
        return db.CambiarActivoAsync(db.EsquemasPrestaciones, id, activo, ElEsquema,
            () => db.EmpleadosNomina.AnyAsync(n => n.IdEsquemaPrestaciones == id && n.Activo && n.IdEmpleadoNavigation.Activo, ct), ct);
    }

    private async Task CopiarAsync(GuardarEmpresa d, Empresas e, CancellationToken ct)
    {
        var regimen = await db.RegimenesFiscales.BuscarAsync(d.IdRegimenFiscal, "el régimen fiscal", ct);
        var personaMoral = d.Rfc.Length == 12;
        if (personaMoral ? !regimen.PersonaMoral : !regimen.PersonaFisica) throw Errores.Catalogos.RegimenNoAplica();

        e.RazonSocial = d.RazonSocial.Trim();
        e.NombreComercial = d.NombreComercial.Trim();
        e.Rfc = d.Rfc.ToUpper();
        e.IdRegimenFiscal = d.IdRegimenFiscal;
        e.CodigoPostalExpedicion = d.CodigoPostalExpedicion;
        e.DominioCorreo = d.DominioCorreo?.ToLower();
        e.OuDirectorio = d.OuDirectorio;
        e.NumeroCertificadoCsd = d.NumeroCertificadoCsd;
        e.RutaCertificadoCsd = d.RutaCertificadoCsd;
        e.VigenciaCsd = d.VigenciaCsd;
        e.ProveedorPac = d.ProveedorPac;
        e.SerieRecibos = d.SerieRecibos ?? e.SerieRecibos ?? "NOM";
    }

    private static void Copiar(GuardarCampus d, Campus c)
    {
        c.IdEmpresa = d.IdEmpresa;
        c.Clave = d.Clave.ToUpper();
        c.NombreCampus = d.NombreCampus.Trim();
        c.CalleNumero = d.CalleNumero;
        c.Ciudad = d.Ciudad;
        c.IdEstado = d.IdEstado;
        c.CodigoPostal = d.CodigoPostal;
        c.Telefono = d.Telefono;
        c.OuDirectorio = d.OuDirectorio;
    }

    private static void Copiar(GuardarRegistroPatronal d, RegistrosPatronales r)
    {
        r.IdEmpresa = d.IdEmpresa;
        r.RegistroPatronal = d.RegistroPatronal.ToUpper();
        r.IdClaseRiesgo = d.IdClaseRiesgo;
        r.IdZonaSalarioMinimo = d.IdZonaSalarioMinimo;
        r.IdEstado = d.IdEstado;
    }

    private static void Copiar(GuardarEsquemaPrestaciones d, EsquemasPrestaciones e)
    {
        e.IdEmpresa = d.IdEmpresa;
        e.NombreEsquema = d.NombreEsquema.Trim();
        e.DiasAguinaldo = d.DiasAguinaldo;
        e.PorcentajePrimaVacacional = d.PorcentajePrimaVacacional;
        e.PorcentajePrimaDominical = d.PorcentajePrimaDominical;
        e.DescontarFaltasAguinaldo = d.DescontarFaltasAguinaldo;
    }
}
