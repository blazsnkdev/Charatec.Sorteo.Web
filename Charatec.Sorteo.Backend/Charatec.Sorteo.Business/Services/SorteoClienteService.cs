using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.DTOs.Responses.Sorteos;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Entities.Enums;
using Charatec.Sorteo.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Business.Services;

public sealed class SorteoClienteService : ISorteoClienteService
{

    private readonly SorteoDbContext _context;

    public SorteoClienteService(SorteoDbContext context)
    {
        _context = context;
    }


    public async Task<Result<ResultadoPaginado<SorteoDisponibleResponse>>>ListarPublicadosAsync(ListarSorteosRequest request, CancellationToken cancellationToken = default)
    {
        int pagina = Math.Max(1, request.Pagina);

        int tamanoPagina = Math.Clamp(
            request.TamanoPagina,
            1,
            50
        );

        IQueryable<TblSorteo> consulta = _context
            .Set<TblSorteo>()
            .AsNoTracking()
            .Where(s =>
                s.Estado == EstadoSorteo.Publicado
            );

        int totalRegistros = await consulta
            .CountAsync(cancellationToken);

        int totalPaginas = (int)Math.Ceiling(
            totalRegistros / (double)tamanoPagina
        );

        var registros = await consulta
            .OrderByDescending(s => s.FechaCreacion)
            .ThenBy(s => s.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(s => new SorteoDisponibleResponse
            {
                Id = s.Id,

                Titulo = s.Titulo,

                Descripcion = s.Descripcion,

                ImagenPortadaUrl = s.ImagenPortadaUrl,

                AporteMinimo = s.AporteMinimo,

                MetaRecaudacion = s.MetaRecaudacion,

                FechaInicio = s.FechaInicio,

                FechaCierre = s.FechaCierre,

                FechaSorteo = s.FechaSorteo,

                Estado = "Publicado"
            })
            .ToListAsync(cancellationToken);

        var resultado = new ResultadoPaginado<SorteoDisponibleResponse>(
            Registros: registros,
            Pagina: pagina,
            TamanoPagina: tamanoPagina,
            TotalRegistros: totalRegistros
        );

        return Result<ResultadoPaginado<SorteoDisponibleResponse>>
            .Success(resultado);
    }
}
