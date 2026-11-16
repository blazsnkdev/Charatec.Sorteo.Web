using Charatec.Sorteo.Business.DTOs.Requests;
using Charatec.Sorteo.Business.DTOs.Responses;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Interfaces;

namespace Charatec.Sorteo.Business.Services
{
    public class ClienteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClienteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
