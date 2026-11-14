using Charatec.Sorteo.Business.DTOs.Requests;
using Charatec.Sorteo.Business.DTOs.Responses;
using Charatec.Sorteo.Business.Interface;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Interfaces;

namespace Charatec.Sorteo.Business.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClienteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Result<Task<RegistrarClienteResponse>> RegistrarClienteAsync(RegistrarClienteRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
