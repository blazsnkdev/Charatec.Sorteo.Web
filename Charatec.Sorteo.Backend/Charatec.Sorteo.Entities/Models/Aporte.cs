
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models;

public class Aporte
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParticipacionId { get; set; }
    public Guid OrdenPagoId { get; set; }
    public decimal Monto { get; set; }
    public EstadoAporte Estado { get; set; } = EstadoAporte.Pendiente;
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FechaConfirmacion { get; set; }
    public Participacion Participacion { get; set; } = null!;
    public OrdenPago OrdenPago { get; set; } = null!;
}
