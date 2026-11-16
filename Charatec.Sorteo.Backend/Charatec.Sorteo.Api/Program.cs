using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Identity;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Data.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

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


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();



// ==========================================
// 5. ASP.NET CORE IDENTITY
// ==========================================

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Usuarios
        options.User.RequireUniqueEmail = true;

        // Contraseñas
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Bloqueo por intentos fallidos
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SorteoDbContext>()
    .AddDefaultTokenProviders();

// ==========================================
// 6. AUTORIZACION
// ==========================================

builder.Services.AddAuthorization();

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

// ==========================================
// 8. CONSTRUIR APLICACION
// ==========================================

var app = builder.Build();

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

// Protección contra solicitudes abusivas
app.UseRateLimiter();

// Cuando configuremos JWT agregaremos:
// app.UseAuthentication();

app.UseAuthorization();

// ==========================================
// 12. CONTROLADORES
// ==========================================

app.MapControllers();

// ==========================================
// 13. INICIAR APLICACION
// ==========================================

app.Run();
