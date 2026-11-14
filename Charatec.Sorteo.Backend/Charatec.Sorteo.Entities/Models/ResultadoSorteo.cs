
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models;

public class ResultadoSorteo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EjecucionSorteoId { get; set; }

    public Guid ParticipacionId { get; set; }

    public Guid PremioSorteoId { get; set; }

    public int Posicion { get; set; } = 1;

    public DateTimeOffset FechaSeleccion { get; set; }
        = DateTimeOffset.UtcNow;

    public EstadoEntregaPremio EstadoEntrega { get; set; }
        = EstadoEntregaPremio.Pendiente;

    public DateTimeOffset? FechaEntrega { get; set; }

    public EjecucionSorteo EjecucionSorteo { get; set; }
        = null!;

    public Participacion Participacion { get; set; }
        = null!;

    public PremioSorteo PremioSorteo { get; set; }
        = null!;
}
