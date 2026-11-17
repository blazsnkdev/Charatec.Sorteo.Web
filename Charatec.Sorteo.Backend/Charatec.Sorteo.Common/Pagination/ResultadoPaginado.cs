namespace Charatec.Sorteo.Common.Pagination;

public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Registros,
    int Pagina,
    int TamanoPagina,
    int TotalRegistros
)
{
    public int TotalPaginas =>
        TotalRegistros == 0
            ? 0
            : (int)Math.Ceiling(
                (double)TotalRegistros / TamanoPagina
            );

    public bool TienePaginaAnterior => Pagina > 1;

    public bool TienePaginaSiguiente =>
        Pagina < TotalPaginas;
}
