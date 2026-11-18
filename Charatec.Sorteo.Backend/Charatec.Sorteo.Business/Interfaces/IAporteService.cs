using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;

namespace Charatec.Sorteo.Business.Interfaces
{
    public interface IAporteService //esto hace que sea un contrado que exponga metodos màs no su implementaciòn
    {

        Task<Result<ResultadoPaginado<AporteListadoResponse>>>
            ListarMisAportesAsync(
                Guid usuarioId,
                ListarAportesRequest request,
                CancellationToken cancellationToken = default
            );

    }
}
