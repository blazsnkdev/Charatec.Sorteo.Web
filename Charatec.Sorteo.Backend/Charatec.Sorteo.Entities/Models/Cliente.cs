
namespace Charatec.Sorteo.Entities.Models;

public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Relación con ASP.NET Core Identity
    public Guid UsuarioId { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string? NumeroDocumento { get; set; }

    public string? Telefono { get; set; }

    public DateTimeOffset FechaCreacion { get; set; }
        = DateTimeOffset.UtcNow;

    public bool Activo { get; set; } = true;

    // Relaciones con entidades del negocio
    public ICollection<Suscripcion> Suscripciones { get; set; }
        = new List<Suscripcion>();

    public ICollection<OrdenPago> OrdenesPago { get; set; }
        = new List<OrdenPago>();

    public ICollection<Participacion> Participaciones { get; set; }
        = new List<Participacion>();
}
