
namespace Charatec.Sorteo.Entities.Models;

public class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string? Modelo { get; set; }

    public string? NumeroParte { get; set; }

    public string? ImagenUrl { get; set; }

    public decimal Costo { get; set; }

    public bool Activo { get; set; } = true;

    public DateTimeOffset FechaCreacion { get; set; }
        = DateTimeOffset.UtcNow;

    public ICollection<PremioSorteo> PremiosSorteo { get; set; }
        = new List<PremioSorteo>();
}
