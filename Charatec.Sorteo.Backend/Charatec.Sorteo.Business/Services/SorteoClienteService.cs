using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.DTOs.Responses.Sorteos;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Business.Services;

public sealed class SorteoClienteService : ISorteoClienteService
{
    private readonly IUnitOfWork _unitOfWork;
    public SorteoClienteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ResultadoPaginado<SorteoDisponibleResponse>>>
        ListarPublicadosAsync(
            ListarSorteosRequest request,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        int pagina = Math.Max(1, request.Pagina);

        int tamanoPagina = Math.Clamp(
            request.TamanoPagina,
            1,
            50
        );
        var ahora = DateTimeOffset.UtcNow;

        var consulta = _unitOfWork._sorteoRepository
            .Consultar()
            .AsNoTracking()
            .Where(s =>
                s.Estado == EstadoSorteo.Publicado &&
                s.FechaSorteo.HasValue &&
                s.FechaSorteo.Value > ahora
            );

        int totalRegistros = await consulta.CountAsync(cancellationToken);

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

        return Result<ResultadoPaginado<SorteoDisponibleResponse>>
            .Success(new ResultadoPaginado<SorteoDisponibleResponse>(
            Registros: registros,
            Pagina: pagina,
            TamanoPagina: tamanoPagina,
            TotalRegistros: totalRegistros
        ));
    }
}