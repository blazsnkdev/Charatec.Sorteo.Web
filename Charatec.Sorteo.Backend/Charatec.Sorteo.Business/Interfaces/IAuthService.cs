using Charatec.Sorteo.Business.DTOs.Requests.Auth;
using Charatec.Sorteo.Business.DTOs.Responses.Auth;
using Charatec.Sorteo.Common.Results;


namespace Charatec.Sorteo.Business.Interfaces
{
    public interface IAuthService
    {
        Task<Result<ClienteRegistradoResponse>> RegistrarClienteAsync(ClienteRegistrarRequest request, CancellationToken cancellationToken = default);
        Task<Result<ClienteLoginResponse>> LoginAsync(
     ClienteLoginRequest request,
     CancellationToken cancellationToken = default);
    }
}
