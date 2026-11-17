using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Charatec.Sorteo.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")] //TODO: protegemos nuestra ruta
    public class SorteoController(ISorteoService sorteoService) : ControllerBase
    {
        [HttpPost("crear")]
        public async Task<IActionResult> RegistrarBorrador(
            [FromBody] CrearSorteoBorradorRequest request,
            CancellationToken cancellationToken)
            {
                var administradorId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await sorteoService.RegistrarSorteoAsync(
                request,
                administradorId,
                cancellationToken
            );
            return ApiResults.FromResult(
              result,
              "Borrador del sorteo registrado correctamente.",
              StatusCodes.Status201Created
          );
        }

    }
}
