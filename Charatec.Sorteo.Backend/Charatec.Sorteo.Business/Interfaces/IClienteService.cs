using Charatec.Sorteo.Business.DTOs.Requests;
using Charatec.Sorteo.Business.DTOs.Responses;
using Charatec.Sorteo.Common.Results;

namespace Charatec.Sorteo.Business.Interface
{
    public interface IClienteService
    {
        Result<Task<RegistrarClienteResponse>>
            RegistrarClienteAsync(RegistrarClienteRequest request);
    }
}
