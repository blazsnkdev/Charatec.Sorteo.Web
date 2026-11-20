using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.DTOs.Requests.Aportes;
using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> ListarMisAportes([FromQuery] ListarAportesRequest request,CancellationToken cancellationToken)
        {
            var usuarioIdTexto =User.FindFirstValue(ClaimTypes.NameIdentifier);

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


        [HttpPatch("/api/v1/admin/aportes/{aporteId:guid}/verificacion")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> VerificarAporte(
            [FromRoute] Guid aporteId,
            [FromBody] VerificarAporteRequest request,
            CancellationToken cancellationToken)
        {
            var usuarioIdTexto =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(
                usuarioIdTexto,
                out var administradorUsuarioId))
            {
                return ApiResults.Unauthorized(
                    "No se pudo identificar al administrador."
                );
            }

            var result = await aporteService.VerificarAsync(
                aporteId,
                administradorUsuarioId,
                request,
                cancellationToken
            );

            return ApiResults.FromResult(
                result,
                "Verificación del aporte procesada correctamente."
            );
        }


        [HttpGet("/api/v1/admin/publicaciones/{sorteoId:guid}/ruleta")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerDatosRuleta(
            [FromRoute] Guid sorteoId,
            CancellationToken cancellationToken)
        {
            var resultado = await aporteService.ObtenerDatosRuletaAsync(
                sorteoId,
                cancellationToken
            );

            Response.Headers.CacheControl = "no-store";

            return ApiResults.FromResult(
                resultado,
                "Datos de la ruleta obtenidos correctamente."
            );
        }

    }
}
