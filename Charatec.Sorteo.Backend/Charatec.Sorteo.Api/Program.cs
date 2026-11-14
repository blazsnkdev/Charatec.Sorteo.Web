using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Protección de datos utilizada por Identity
builder.Services.AddDataProtection();


builder.Services.AddControllers();
var connectionString = builder.Configuration
    .GetConnectionString("PostgreSQL");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontro la cadena de conexion PostgreSQL."
    );
}

builder.Services.AddDbContext<SorteoDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});


builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Configuracion de usuarios
        options.User.RequireUniqueEmail = true;

        // Reglas de contrasena
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Proteccion contra intentos fallidos
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SorteoDbContext>()
    .AddDefaultTokenProviders();



builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
