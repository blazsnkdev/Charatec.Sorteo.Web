using Charatec.Sorteo.Business.DTOs.Requests.Auth;
using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.DTOs.Responses.Sorteos;
using Charatec.Sorteo.Common.Results;

namespace Charatec.Sorteo.Business.Interfaces
{
    public interface ISorteoService
    {
        Task<Result<BorradorSorteoRegistradoResponse>> RegistrarSorteoAsync(
                    CrearSorteoBorradorRequest request,
                    Guid administradorId,
                    CancellationToken cancellationToken = default);
    }
}
