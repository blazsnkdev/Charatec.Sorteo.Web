
using Charatec.Sorteo.Api.Responses;
using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charatec.Sorteo.Api.Controllers;

[ApiController]
[Route("api/v1/cliente/sorteos")]
[Authorize(Roles = "Cliente")]
public sealed class SorteoClienteController(ISorteoClienteService sorteoClienteService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] ListarSorteosRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sorteoClienteService.ListarPublicadosAsync(
            request,
            cancellationToken
        );

        return ApiResults.FromResult(
            result,
            "Listado de sorteos publicados obtenido correctamente."
        );
    }
}
