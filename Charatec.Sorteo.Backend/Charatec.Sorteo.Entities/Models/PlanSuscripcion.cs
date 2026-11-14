
namespace Charatec.Sorteo.Entities.Models;

public class PlanSuscripcion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal PrecioMensual { get; set; }

    public int DiasPruebaGratis { get; set; } = 30;

    public bool Activo { get; set; } = true;

    public DateTimeOffset FechaCreacion { get; set; }
        = DateTimeOffset.UtcNow;

    public ICollection<Suscripcion> Suscripciones { get; set; }
        = new List<Suscripcion>();
}
