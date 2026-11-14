
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models;

public class TransaccionPago
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrdenPagoId { get; set; }

    public string Proveedor { get; set; } = string.Empty;

    public string? ReferenciaExterna { get; set; }

    public EstadoTransaccionPago Estado { get; set; }
        = EstadoTransaccionPago.Pendiente;

    public DateTimeOffset FechaCreacion { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? FechaProcesamiento { get; set; }

    public OrdenPago OrdenPago { get; set; } = null!;
}
