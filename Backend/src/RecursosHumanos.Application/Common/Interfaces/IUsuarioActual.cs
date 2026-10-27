using RecursosHumanos.Application.Common.Exceptions;

namespace RecursosHumanos.Application.Common.Interfaces;

public interface IUsuarioActual
{
    int? IdUsuario { get; }
    int? IdEmpleado { get; }
    string? Ip { get; }

    IReadOnlyDictionary<string, string?> Permisos { get; }

    bool TienePermiso(string permiso) => Permisos.ContainsKey(permiso);

    string? Alcance(string permiso) => Permisos.GetValueOrDefault(permiso);

    void ExigirPermiso(string permiso)
    {
        if (!TienePermiso(permiso)) throw Errores.General.SinPermiso();
    }
}
