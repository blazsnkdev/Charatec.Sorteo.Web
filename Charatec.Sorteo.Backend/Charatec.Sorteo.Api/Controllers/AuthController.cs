using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.DTOs.Requests.Auth;
using Charatec.Sorteo.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Charatec.Sorteo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public AuthController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpPost("register")]
        [EnableRateLimiting("registro")]
        public Task<IActionResult> Register([FromBody] ClienteRegistrarRequest request)
        {
            return new ApiResponse(true,200,"",null,null,);
        }
    }
}
