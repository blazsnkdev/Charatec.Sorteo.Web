using Charatec.Sorteo.Business.DTOs.Requests.Auth;
using Charatec.Sorteo.Business.DTOs.Responses.Auth;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Identity;
using Charatec.Sorteo.Data.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Charatec.Sorteo.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClienteRepository _clienteRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(
            IUnitOfWork unitOfWork,
            IClienteRepository clienteRepository,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _clienteRepository = clienteRepository;
            _userManager = userManager;
        }
        public Task<Result<ClienteRegistradoResponse>> RegistrarClienteAsync(ClienteRegistrarRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
