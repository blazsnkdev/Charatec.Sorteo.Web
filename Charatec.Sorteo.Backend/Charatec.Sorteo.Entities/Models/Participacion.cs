
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models;

public class Participacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClienteId { get; set; }

    public Guid SorteoId { get; set; }

    public DateTimeOffset FechaInscripcion { get; set; }
        = DateTimeOffset.UtcNow;

    public EstadoParticipacion Estado { get; set; }
        = EstadoParticipacion.Inscrita;

    public Cliente Cliente { get; set; } = null!;

    public Sorteo Sorteo { get; set; } = null!;

    public ICollection<Aporte> Aportes { get; set; }
        = new List<Aporte>();

    public ICollection<DetallePadronSorteo> DetallesPadron { get; set; }
        = new List<DetallePadronSorteo>();

    public ICollection<ResultadoSorteo> Resultados { get; set; }
        = new List<ResultadoSorteo>();
}
