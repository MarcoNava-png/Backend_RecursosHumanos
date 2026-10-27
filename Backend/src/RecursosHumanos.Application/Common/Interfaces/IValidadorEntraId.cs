namespace RecursosHumanos.Application.Common.Interfaces;

public interface IValidadorEntraId
{
    ConfiguracionEntraId Configuracion { get; }

    Task<IdentidadEntraId> ValidarAsync(string token, CancellationToken ct);
}

public record ConfiguracionEntraId(bool Habilitado, string? TenantId, string? ClientId, string? Autoridad);

public record IdentidadEntraId(string Oid, string Upn);
