using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecursosHumanos.Api.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RecursosHumanos.Application.Common;
using Microsoft.OpenApi;
using RecursosHumanos.Api;
using RecursosHumanos.Api.Seguridad;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Application.Features.Asistencia;
using RecursosHumanos.Application.Features.Auditoria;
using RecursosHumanos.Application.Features.Auth;
using RecursosHumanos.Application.Features.Catalogos;
using RecursosHumanos.Application.Features.Creditos;
using RecursosHumanos.Application.Features.Empleados;
using RecursosHumanos.Application.Features.Horarios;
using RecursosHumanos.Application.Features.Incidencias;
using RecursosHumanos.Application.Features.Nomina;
using RecursosHumanos.Application.Features.Prestaciones;
using RecursosHumanos.Application.Features.Seguridad;
using RecursosHumanos.Application.Features.Solicitudes;
using RecursosHumanos.Application.Features.Vacaciones;
using RecursosHumanos.Infrastructure;
using RecursosHumanos.Infrastructure.Archivos;
using RecursosHumanos.Infrastructure.Jobs;
using RecursosHumanos.Infrastructure.Persistence;
using RecursosHumanos.Infrastructure.Seguridad;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWindowsService(o => o.ServiceName = "RecursosHumanos");

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>()
    ?? throw new InvalidOperationException("Falta la sección Jwt en appsettings.");
if (jwt.Clave.Length < 32) throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 caracteres.");

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<IReloj, Reloj>();
builder.Services.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();
builder.Services.AddSingleton<IValidadorEntraId>(new ValidadorEntraId(
    builder.Configuration.GetSection("EntraId").Get<EntraIdOpciones>() ?? new EntraIdOpciones()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
builder.Services.AddScoped<AuditoriaInterceptor>();
builder.Services.AddScoped<UsuarioConexionInterceptor>();
builder.Services.AddDbContext<RecursosHumanosDbContext>((sp, o) => o
    .UseNpgsql(builder.Configuration.GetConnectionString("RecursosHumanos"))
    .AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>(), sp.GetRequiredService<UsuarioConexionInterceptor>()));
builder.Services.AddScoped<IRecursosHumanosDbContext>(sp => sp.GetRequiredService<RecursosHumanosDbContext>());

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmpleadosService>();
builder.Services.AddScoped<CatalogosService>();
builder.Services.AddScoped<EmpresasService>();
builder.Services.AddScoped<CatalogosPersonalService>();
builder.Services.AddScoped<CatalogosAsistenciaService>();
builder.Services.AddScoped<AsistenciaService>();
builder.Services.AddScoped<SolicitudesService>();
builder.Services.AddScoped<VacacionesService>();
builder.Services.AddScoped<HorariosService>();
builder.Services.AddScoped<IncapacidadesService>();
builder.Services.AddScoped<SuplenciasService>();
builder.Services.AddScoped<PeriodosNominaService>();
builder.Services.AddScoped<RecibosService>();
builder.Services.AddScoped<DatosNominaService>();
builder.Services.AddScoped<BajasService>();
builder.Services.AddScoped<PtuService>();
builder.Services.AddScoped<CreditosService>();
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<RolesService>();
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddSingleton(new PoliticaAcceso(builder.Configuration.GetValue("Seguridad:IntentosMaximos", 5),
    builder.Configuration.GetValue("Seguridad:MinutosBloqueo", 15)));
builder.Services.AddScoped<IProcesosBd, ProcesosBd>();
builder.Services.AddSingleton<IAlmacenArchivos>(new AlmacenArchivosLocal(
    Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Archivos:Carpeta"] ?? "archivos"),
    builder.Configuration.GetValue("Archivos:MaximoMb", 5)));
builder.Services.AddHostedService(sp => new ProcesosNocturnosJob(
    sp.GetRequiredService<IServiceScopeFactory>(), sp.GetRequiredService<IReloj>(),
    sp.GetRequiredService<ILogger<ProcesosNocturnosJob>>(),
    TimeOnly.Parse(builder.Configuration["ProcesosNocturnos:Hora"] ?? "01:00")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Emisor,
            ValidAudience = jwt.Audiencia,
            IssuerSigningKey = jwt.LlaveFirma(),
            NameClaimType = ClaimsToken.Nombre,
            RoleClaimType = ClaimsToken.Rol
        };
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(AuthController.LimiteLogin, http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
    o.OnRejected = (contexto, ct) => new ValueTask(contexto.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = StatusCodes.Status429TooManyRequests,
        Title = "Demasiados intentos. Espera un minuto e inténtalo de nuevo.",
        Extensions = { ["codigo"] = Errores.Auth.CodigoDemasiadosIntentos }
    }, ct));
});

var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddExceptionHandler<ManejadorErrores>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi(o => o.AddDocumentTransformer((documento, _, _) =>
{
    documento.Components ??= new OpenApiComponents();
    documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    documento.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };
    documento.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", documento)] = [] }];
    return Task.CompletedTask;
}));

var app = builder.Build();

if (app.Configuration.GetValue<bool>("BaseDatos:CrearSiNoExiste")
    && await CreadorBaseDatos.CrearSiNoExisteAsync(app.Configuration.GetConnectionString("RecursosHumanos")!,
        Path.Combine(AppContext.BaseDirectory, "BaseDatos", "recursoshumanos.sql")))
    app.Logger.LogInformation("Se creó la base de datos con BaseDatos/recursoshumanos.sql");

app.UseExceptionHandler();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Documentacion:Habilitada"))
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "RecursosHumanos"));
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.Use(async (contexto, siguiente) =>
{
    if (int.TryParse(contexto.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var idUsuario))
        contexto.Items[UsuarioActual.LlavePermisos] = await contexto.RequestServices.GetRequiredService<IRecursosHumanosDbContext>()
            .PermisosDelUsuarioAsync(idUsuario, contexto.RequestAborted);
    await siguiente();
});
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
