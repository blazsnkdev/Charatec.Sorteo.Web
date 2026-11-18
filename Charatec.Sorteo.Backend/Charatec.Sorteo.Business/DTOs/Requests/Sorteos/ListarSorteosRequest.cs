
using System.ComponentModel.DataAnnotations;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Business.DTOs.Requests.Sorteos;

public sealed record ListarSorteosRequest
{
    [Range(1, int.MaxValue,
        ErrorMessage = "La página debe ser mayor que cero.")]
    public int Pagina { get; init; } = 1;

    [Range(1, 100,
        ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int TamanoPagina { get; init; } = 10;
    public EstadoSorteo? Estado { get; init; }
    public PaginacionRequest ObtenerPaginacion() => new()
    {
        Pagina = Pagina,
        TamanoPagina = TamanoPagina
    };
}
