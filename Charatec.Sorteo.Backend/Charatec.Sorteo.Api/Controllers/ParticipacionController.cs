using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Charatec.Sorteo.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Cliente")]
    [Route("api/v1/cliente/sorteos")]
    public class ParticipacionController(IParticipacionService participacionService) : ControllerBase
    {

        [HttpGet("{sorteoId:guid}/participar")]
        public async Task<IActionResult> Obtener(
        [FromRoute] Guid sorteoId,
        CancellationToken cancellationToken)
        {
            var usuarioIdTexto = User.FindFirstValue(//TODO: esto lo puedo mejorar 
                ClaimTypes.NameIdentifier
            );

            if (!Guid.TryParse(usuarioIdTexto, out var usuarioId))
            {
                return Unauthorized();
            }

            var email = User.FindFirstValue(
                ClaimTypes.Email
            );

            var result = await participacionService.ObtenerAsync(
                usuarioId,
                email,
                sorteoId,
                cancellationToken
            );

            return ApiResults.FromResult(
                result,
                "Información para participar obtenida correctamente."
            );
        }
    }
}
