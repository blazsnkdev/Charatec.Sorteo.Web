
namespace Charatec.Sorteo.Entities.Models;

public class DetallePadronSorteo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EjecucionSorteoId { get; set; }

    public Guid ParticipacionId { get; set; }

    public long PesoParticipacion { get; set; }

    // Relaciones
    public EjecucionSorteo EjecucionSorteo { get; set; }
        = null!;

    public Participacion Participacion { get; set; }
        = null!;
}
