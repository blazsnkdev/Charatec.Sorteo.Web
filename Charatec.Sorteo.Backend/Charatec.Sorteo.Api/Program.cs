using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Business.Services;
using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Identity;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Data.Repositories;
using Charatec.Sorteo.Data.Services;
using Charatec.Sorteo.Entities.Enums;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONTROLADORES Y VALIDACIONES HTTP
// ==========================================

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors
                        .Select(e =>
                            string.IsNullOrWhiteSpace(e.ErrorMessage)
                                ? "El valor ingresado no es válido."
                                : e.ErrorMessage)
                        .ToArray()
                );

            return ApiResults.BadRequest(
                "Los datos ingresados no son válidos.",
                errors);
        };
    });

// ==========================================
// 2. DOCUMENTACION OPENAPI
// ==========================================

builder.Services.AddOpenApi();

// ==========================================
// 3. PROTECCION DE DATOS
// ==========================================

builder.Services.AddDataProtection();

// ==========================================
// 4. CONEXION A POSTGRESQL
// ==========================================

var connectionString = builder.Configuration
    .GetConnectionString("PostgreSQL");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión PostgreSQL.");
}

builder.Services.AddDbContext<SorteoDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

//Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
//Services
builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ==========================================
// 5. ASP.NET CORE IDENTITY
// ==========================================

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Usuarios
        options.User.RequireUniqueEmail = true;

        // Contraseñas
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 1;

        // Bloqueo por intentos fallidos
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SorteoDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// ==========================================
// 6. AUTORIZACION
// ==========================================

// La autenticación JWT se configurará
// cuando implementemos el inicio de sesión.

// ==========================================
// 7. RATE LIMITING
// ==========================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // Política para el registro de clientes
    options.AddPolicy("registro", context =>
    {
        var ip = context.Connection.RemoteIpAddress?
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.AddPolicy("login", context =>
    {
        var ip = context.Connection.RemoteIpAddress?
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });


    // Respuesta JSON cuando se supera el límite
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        var response = new ApiResponse<object>(
            Success: false,
            StatusCode: 429,
            Message: "Demasiadas solicitudes. Inténtalo más tarde.",
            Data: null,
            Errors: null
        );

        await context.HttpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);
    };
});
//LOGIN AUTH

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Falta Jwt:Issuer.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Falta Jwt:Audience.");

var jwtEncodedKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta Jwt:Key.");

var jwtKeyBytes = Convert.FromBase64String(jwtEncodedKey);

if (jwtKeyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "La clave JWT debe tener al menos 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;


        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // Validar firma
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(jwtKeyBytes),

                // Algoritmo permitido
                ValidAlgorithms = new[]
                {
            SecurityAlgorithms.HmacSha256
                },

                // Validar emisor
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                // Validar audiencia
                ValidateAudience = true,
                ValidAudience = jwtAudience,

                // Validar expiración
                ValidateLifetime = true,
                RequireExpirationTime = true,

                // Requerir firma
                RequireSignedTokens = true,

                ClockSkew = TimeSpan.FromSeconds(30),

                // Claims
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };

    });

builder.Services.AddAuthorization();




// ==========================================
// 8. CONSTRUIR APLICACION
// ==========================================

var app = builder.Build();

// ==========================================
// INICIALIZAR ROLES DE IDENTITY
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var nombreRol = Rol.Cliente.ToString();

    if (!await roleManager.RoleExistsAsync(nombreRol))
    {
        var resultado = await roleManager.CreateAsync(
            new IdentityRole<Guid>(nombreRol));

        if (!resultado.Succeeded)
        {
            var errores = string.Join(
                "; ",
                resultado.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"No se pudo crear el rol {nombreRol}: {errores}");
        }
    }
}

// ==========================================
// 9. MANEJO GLOBAL DE EXCEPCIONES
// ==========================================

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features
            .Get<IExceptionHandlerFeature>();

        var logger = context.RequestServices
            .GetRequiredService<ILogger<Program>>();

        if (exceptionFeature is not null)
        {
            logger.LogError(
                exceptionFeature.Error,
                "Error inesperado. TraceId: {TraceId}",
                context.TraceIdentifier);
        }

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        var response = new ApiResponse<object>(
            Success: false,
            StatusCode: 500,
            Message: "Ocurrió un error interno del servidor.",
            Data: null,
            Errors: null
        );

        await context.Response.WriteAsJsonAsync(response);
    });
});

// ==========================================
// 10. DOCUMENTACION EN DESARROLLO
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ==========================================
// 11. MIDDLEWARES
// ==========================================

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
