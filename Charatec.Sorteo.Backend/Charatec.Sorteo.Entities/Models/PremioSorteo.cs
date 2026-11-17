
namespace Charatec.Sorteo.Entities.Models;

public class PremioSorteo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SorteoId { get; set; }

    public Guid ProductoId { get; set; }

    public int Cantidad { get; set; } = 1;

    public int OrdenPremio { get; set; } = 1;

    public decimal CostoAlPublicar { get; set; }

    public TblSorteo Sorteo { get; set; } = null!;

    public Producto Producto { get; set; } = null!;

    public ICollection<ResultadoSorteo> Resultados { get; set; }
        = new List<ResultadoSorteo>();
}
