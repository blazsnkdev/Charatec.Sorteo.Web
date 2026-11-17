using Charatec.Sorteo.Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Data.Extensions;

public static class PaginacionExtensions
{
    public static async Task<ResultadoPaginado<T>> PaginarAsync<T>(
        this IQueryable<T> consulta,
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        ArgumentNullException.ThrowIfNull(paginacion);

        if (paginacion.Pagina < 1 ||
            paginacion.TamanoPagina < 1 ||
            paginacion.TamanoPagina > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paginacion),
                "Los parámetros de paginación no son válidos."
            );
        }

        var totalRegistros = await consulta.CountAsync(
            cancellationToken
        );

        var registros = await consulta
            .Skip((paginacion.Pagina - 1) * paginacion.TamanoPagina)
            .Take(paginacion.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<T>(
            Registros: registros,
            Pagina: paginacion.Pagina,
            TamanoPagina: paginacion.TamanoPagina,
            TotalRegistros: totalRegistros
        );
    }
}