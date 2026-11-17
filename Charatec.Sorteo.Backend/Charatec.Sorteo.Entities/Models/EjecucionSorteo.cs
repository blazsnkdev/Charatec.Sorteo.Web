
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models;

public class EjecucionSorteo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SorteoId { get; set; }

    public DateTimeOffset FechaCreacion { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? FechaEjecucion { get; set; }

    public string AlgoritmoVersion { get; set; }
        = string.Empty;

    public string HashPadron { get; set; } = string.Empty;

    public EstadoEjecucionSorteo Estado { get; set; }
        = EstadoEjecucionSorteo.Preparada;

    public TblSorteo Sorteo { get; set; } = null!;

    public ICollection<DetallePadronSorteo> DetallesPadron
    { get; set; } = new List<DetallePadronSorteo>();

    public ICollection<ResultadoSorteo> Resultados
    { get; set; } = new List<ResultadoSorteo>();
}
