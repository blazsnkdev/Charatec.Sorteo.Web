using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Business.Options;
using Charatec.Sorteo.Business.Services;
using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Identity;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Data.Repositories;
using Charatec.Sorteo.Data.Services;
using Charatec.Sorteo.Entities.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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


builder.Services.AddOpenApi();

builder.Services.AddDataProtection();

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
//Limite de participacion
builder.Services.Configure<ParticipacionOptions>(
    builder.Configuration.GetSection("Participacion")
);

  
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ISorteoRepository, SorteoRepository>();
builder.Services.AddScoped<ISorteoClienteService, SorteoClienteService>();
builder.Services.AddScoped<IParticipacionYapeRepository,ParticipacionYapeRepository>();
builder.Services.AddScoped<IAporteRepository, AporteRepository>();

builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ISorteoService, SorteoService>();
builder.Services.AddScoped<IParticipacionService, ParticipacionService>();
builder.Services.AddScoped<IAporteService, AporteService>();

var directorioPortadas = Path.Combine(
    builder.Environment.ContentRootPath,
    "wwwroot",
    "uploads",
    "sorteos"
);

builder.Services.AddScoped<IImagenStorageService>(
    _ => new LocalImagenStorageService(directorioPortadas)
);
Directory.CreateDirectory(directorioPortadas);

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
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(jwtKeyBytes),

                ValidAlgorithms = new[]
                {
            SecurityAlgorithms.HmacSha256
                },

                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                RequireExpirationTime = true,

                RequireSignedTokens = true,

                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };

    });

builder.Services.AddAuthorization();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var roles = new[]
    {
        Rol.Cliente,
        Rol.Administrador
    };

    foreach (var rol in roles)
    {
        var nombreRol = rol.ToString();

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
}


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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AngularFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
