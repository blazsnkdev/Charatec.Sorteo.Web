
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models;

public class Sorteo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AdministradorUsuarioId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string? ImagenPortadaUrl { get; set; }

    public decimal AporteMinimo { get; set; }

    public decimal? MetaRecaudacion { get; set; }

    public DateTimeOffset FechaInicio { get; set; }

    public DateTimeOffset FechaCierre { get; set; }

    public DateTimeOffset? FechaSorteo { get; set; }

    public EstadoSorteo Estado { get; set; }
        = EstadoSorteo.Borrador;

    public DateTimeOffset FechaCreacion { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? FechaActualizacion { get; set; }

    public ICollection<PremioSorteo> Premios { get; set; }
        = new List<PremioSorteo>();

    public ICollection<Participacion> Participaciones { get; set; }
        = new List<Participacion>();

    public ICollection<EjecucionSorteo> Ejecuciones { get; set; }
        = new List<EjecucionSorteo>();
}
