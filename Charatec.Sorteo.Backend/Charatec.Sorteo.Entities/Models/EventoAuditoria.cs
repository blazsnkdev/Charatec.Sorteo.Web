
namespace Charatec.Sorteo.Entities.Models;

public class EventoAuditoria
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UsuarioId { get; set; }

    public string Entidad { get; set; } = string.Empty;

    public Guid? EntidadId { get; set; }

    public string Accion { get; set; } = string.Empty;

    public string? DatosAnteriores { get; set; }

    public string? DatosNuevos { get; set; }

    public DateTimeOffset Fecha { get; set; }
        = DateTimeOffset.UtcNow;
}
