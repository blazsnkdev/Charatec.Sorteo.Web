using Charatec.Sorteo.Business.DTOs.Requests.Aportes;
using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Aportes;
using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Ruleta;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;

namespace Charatec.Sorteo.Business.Interfaces
{
    public interface IAporteService //esto hace que sea un contrado que exponga metodos màs no su implementaciòn
    {

        Task<Result<ResultadoPaginado<AporteListadoResponse>>>
            ListarMisAportesAsync(
                Guid usuarioId,
                ListarAportesRequest request,
                CancellationToken cancellationToken = default
            );
        Task<Result<ResultadoPaginado<AportePublicacionResponse>>>
            ListarPorPublicacionAsync(
                Guid sorteoId,
                ListarAportesPublicacionRequest request,
                CancellationToken cancellationToken = default
            );


        Task<Result<VerificarAporteResponse>> VerificarAsync(
            Guid aporteId,
            Guid administradorUsuarioId,
            VerificarAporteRequest request,
            CancellationToken cancellationToken = default
        );


        Task<Result<RuletaDatosResponse>> ObtenerDatosRuletaAsync(
            Guid sorteoId,
            CancellationToken cancellationToken = default
        );


    }
}
