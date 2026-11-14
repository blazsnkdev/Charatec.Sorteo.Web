
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models;

public class OrdenPago
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClienteId { get; set; }

    public Guid? SuscripcionId { get; set; }

    public TipoOrdenPago Tipo { get; set; }

    public decimal Monto { get; set; }

    public string Moneda { get; set; } = "PEN";

    public string IdempotencyKey { get; set; } = string.Empty;

    public EstadoOrdenPago Estado { get; set; }
        = EstadoOrdenPago.Pendiente;

    public DateTimeOffset FechaCreacion { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? FechaActualizacion { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public Suscripcion? Suscripcion { get; set; }

    public Aporte? Aporte { get; set; }

    public ICollection<TransaccionPago> Transacciones { get; set; }
        = new List<TransaccionPago>();
}
