using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.DTOs.Requests.Aportes;
using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Charatec.Sorteo.Api.Controllers
{
    [Route("api/v1/Cliente/[controller]")]
    [ApiController]
    public class AporteController(IAporteService aporteService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ListarMisAportes(
            [FromQuery] ListarAportesRequest request,
            CancellationToken cancellationToken)
        {
            // Obtener el ID del usuario autenticado desde JWT.
            var usuarioIdTexto =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(usuarioIdTexto, out var usuarioId))
            {
                return ApiResults.Unauthorized(
                    "No se pudo identificar al usuario autenticado."
                );
            }

            var result = await aporteService.ListarMisAportesAsync(
                usuarioId,
                request,
                cancellationToken
            );

            return ApiResults.FromResult(
                result,
                "Listado de aportes obtenido correctamente."
            );
        }

        [HttpGet("/api/v1/admin/publicaciones/{sorteoId:guid}/aportes")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListarPorPublicacion(
            [FromRoute] Guid sorteoId,
            [FromQuery] ListarAportesPublicacionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await aporteService.ListarPorPublicacionAsync(
                sorteoId,
                request,
                cancellationToken
            );

            return ApiResults.FromResult(
                result,
                "Listado de aportes de la publicación obtenido correctamente."
            );
        }

    }
}
