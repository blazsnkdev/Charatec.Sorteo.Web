
using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.DTOs.Responses.Sorteos;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;

namespace Charatec.Sorteo.Business.Interfaces;

public interface ISorteoClienteService
{
    Task<Result<ResultadoPaginado<SorteoDisponibleResponse>>>
        ListarPublicadosAsync(
            ListarSorteosRequest request,
            CancellationToken cancellationToken = default
        );
}
