using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.DTOs.Requests.Auth;
using Charatec.Sorteo.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Charatec.Sorteo.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("registro")]
        public async Task<IActionResult> RegistrarCliente(
                [FromBody] ClienteRegistrarRequest request,
                CancellationToken cancellationToken)
        {
            var result = await authService.RegistrarClienteAsync(
                request,
                cancellationToken);

            return ApiResults.FromResult(
                result,
                "Cliente registrado correctamente.",
                StatusCodes.Status201Created);
        }
    }
}
