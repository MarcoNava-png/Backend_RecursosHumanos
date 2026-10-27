using System.ComponentModel.DataAnnotations;
using RecursosHumanos.Application.Features.Seguridad;

namespace RecursosHumanos.Application.Features.Empleados;

public record EmpleadoDto(int IdEmpleado, string NombreCompleto, string? Matricula, string Departamento, string Perfil,
    string? Puesto, string? Campus, string Estatus);

public record FiltroEmpleados(string? Busqueda, int? IdDepartamento, int? IdCampus, int? IdEstatusLaboral,
    int NumeroPagina = 1, [Range(1, 200)] int Tamano = 50);

public record EmpleadoDetalleDto(
    int IdEmpleado, string Nombre, string ApellidoPaterno, string? ApellidoMaterno, string? Curp, string? Rfc,
    string? CorreoElectronico, string? Telefono, int IdEmpresa, int IdDepartamento, int IdPerfil, int? IdPuesto,
    int? IdCampus, int? IdJefeDirecto, int? NumeroChecador, string? Matricula, DateOnly FechaIngreso,
    int IdEstatusLaboral, string? ExtensionTelefono, bool AvisoPrivacidadBiometricos, DateTime? FechaFirmaAviso);

public record GuardarEmpleadoSolicitud(
    [StringLength(100)] string Nombre,
    [StringLength(100)] string ApellidoPaterno,
    [StringLength(100)] string? ApellidoMaterno,
    [StringLength(18, MinimumLength = 18)] string? Curp,
    [StringLength(13, MinimumLength = 12)] string? Rfc,
    [StringLength(150), EmailAddress] string? CorreoElectronico,
    [StringLength(20)] string? Telefono,
    int IdEmpresa,
    int IdDepartamento,
    int IdPerfil,
    int? IdPuesto,
    int? IdCampus,
    int? IdJefeDirecto,
    int? NumeroChecador,
    [StringLength(30)] string? Matricula,
    DateOnly FechaIngreso,
    [StringLength(10)] string? ExtensionTelefono,
    bool AvisoPrivacidadBiometricos,
    int? IdRolAcceso = null);

public record EmpleadoCreadoDto(int IdEmpleado, AccesoEmpleadoDto Acceso);
