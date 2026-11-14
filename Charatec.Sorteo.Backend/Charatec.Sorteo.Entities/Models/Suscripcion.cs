
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models;

public class Suscripcion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClienteId { get; set; }

    public Guid PlanSuscripcionId { get; set; }

    public DateTimeOffset FechaInicio { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? FechaFinPrueba { get; set; }

    public DateTimeOffset? FechaProximoCobro { get; set; }

    public DateTimeOffset? FechaFin { get; set; }

    public DateTimeOffset? FechaCancelacion { get; set; }

    public EstadoSuscripcion Estado { get; set; }
        = EstadoSuscripcion.PruebaGratis;

    public Cliente Cliente { get; set; } = null!;

    public PlanSuscripcion PlanSuscripcion { get; set; }
        = null!;

    public ICollection<OrdenPago> OrdenesPago { get; set; }
        = new List<OrdenPago>();
}
