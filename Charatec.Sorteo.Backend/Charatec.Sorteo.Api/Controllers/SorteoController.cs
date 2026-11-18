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
        [HttpPost]
        public async Task<IActionResult> Registrar(
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


        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] ListarSorteosRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sorteoService.ListarSorteosAsync(
                request,
                cancellationToken
            );

            return ApiResults.FromResult(
                result,
                "Listado de sorteos obtenido correctamente."
            );
        }



        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Obtener(Guid id, CancellationToken cancellationToken)
        {
            var result = await sorteoService.ObtenerPorIdAsync(
                id,
                cancellationToken
            );

            return ApiResults.FromResult(
                result,
                "Sorteo obtenido correctamente."
            );
        }


        [HttpPatch("{id:guid}/publicar")]
        public async Task<IActionResult> PublicarSorteo(Guid id, CancellationToken cancellationToken)
        {
            //TODO: OJITO AQUI ESTO PUEDO MODIFICAR Y REFACTORIZAR A FUTURO POR QUE 
            var usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!Guid.TryParse(usuarioId, out var administradorId))
            {
                return ApiResults.Unauthorized(
                    "No se pudo identificar al administrador."
                );
            }

            var result = await sorteoService.PublicarSorteoAsync(
                id,
                administradorId,
                cancellationToken
            );

            return ApiResults.FromResult(
                result,
                "Sorteo publicado correctamente."
            );
        }


        [HttpPut("{id:guid}")]
        public async Task<IActionResult> ActualizarBorrador(
            Guid id,
            [FromBody] ActualizarSorteoBorradorRequest request,
            CancellationToken cancellationToken)
        {
            var usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!Guid.TryParse(usuarioId, out var administradorId))
            {
                return ApiResults.Unauthorized(
                    "No se pudo identificar al administrador."
                );
            }

            var result = await sorteoService.ActualizarBorradorAsync(
                id,
                administradorId,
                request,
                cancellationToken
            );

            return ApiResults.FromResult(
                result,
                "Borrador actualizado correctamente."
            );
        }

    }
}
