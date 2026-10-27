using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;

namespace RecursosHumanos.Infrastructure.Seguridad;

public class EntraIdOpciones
{
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string Autoridad { get; set; } = "";

    public bool Habilitado => TenantId != "" && ClientId != "";
    public string AutoridadEfectiva => Autoridad != "" ? Autoridad.TrimEnd('/') : $"https://login.microsoftonline.com/{TenantId}/v2.0";
}

public class ValidadorEntraId : IValidadorEntraId
{
    private readonly EntraIdOpciones opciones;
    private readonly ConfigurationManager<OpenIdConnectConfiguration>? metadatos;

    public ValidadorEntraId(EntraIdOpciones opciones)
    {
        this.opciones = opciones;
        if (!opciones.Habilitado) return;
        var autoridad = new Uri(opciones.AutoridadEfectiva);
        metadatos = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{opciones.AutoridadEfectiva}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = !autoridad.IsLoopback });
    }

    public ConfiguracionEntraId Configuracion => opciones.Habilitado
        ? new(true, opciones.TenantId, opciones.ClientId, opciones.AutoridadEfectiva)
        : new(false, null, null, null);

    public async Task<IdentidadEntraId> ValidarAsync(string token, CancellationToken ct)
    {
        if (metadatos is null) throw Errores.Auth.EntraNoConfigurado();
        OpenIdConnectConfiguration configuracion;
        try { configuracion = await metadatos.GetConfigurationAsync(ct); }
        catch (InvalidOperationException) { throw Errores.Auth.SinConexionEntra(); }

        var resultado = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuers = [configuracion.Issuer, $"https://sts.windows.net/{opciones.TenantId}/"],
            ValidAudiences = [opciones.ClientId, $"api://{opciones.ClientId}"],
            IssuerSigningKeys = configuracion.SigningKeys,
            ClockSkew = TimeSpan.FromMinutes(2)
        });
        if (!resultado.IsValid)
        {
            if (resultado.Exception is SecurityTokenSignatureKeyNotFoundException) metadatos.RequestRefresh();
            throw Errores.Auth.TokenEntraInvalido();
        }

        string? Claim(string nombre) => resultado.Claims.TryGetValue(nombre, out var valor) ? valor as string : null;
        var oid = Claim("oid");
        var upn = Claim("preferred_username") ?? Claim("upn");
        if (string.IsNullOrEmpty(oid) || string.IsNullOrEmpty(upn)) throw Errores.Auth.TokenEntraInvalido();
        return new IdentidadEntraId(oid, upn);
    }
}
