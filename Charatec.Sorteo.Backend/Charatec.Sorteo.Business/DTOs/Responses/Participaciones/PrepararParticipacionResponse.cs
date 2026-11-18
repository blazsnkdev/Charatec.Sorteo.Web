
namespace Charatec.Sorteo.Business.DTOs.Responses.Participaciones;

public sealed class PrepararParticipacionResponse
{
    public Guid SorteoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string? ImagenPortadaUrl { get; set; }

    public decimal AporteMinimo { get; set; }

    public decimal AporteMaximo { get; set; }

    public decimal PasoAporte { get; set; }

    public decimal MontoInicial { get; set; }

    public decimal? MetaRecaudacion { get; set; }

    public DateTimeOffset FechaInicio { get; set; }

    public DateTimeOffset FechaCierre { get; set; }

    public DateTimeOffset? FechaSorteo { get; set; }

    public Guid UsuarioId { get; set; }

    public string? Email { get; set; }

    public bool PuedeParticipar { get; set; }

    public string MensajeDisponibilidad { get; set; }
        = string.Empty;

    public string Moneda { get; set; } = "PEN";
}
