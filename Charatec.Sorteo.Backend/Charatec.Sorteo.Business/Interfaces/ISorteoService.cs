using Charatec.Sorteo.Business.DTOs.Requests.Auth;
using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.DTOs.Responses.Sorteos;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;

namespace Charatec.Sorteo.Business.Interfaces
{
    public interface ISorteoService
    {
        Task<Result<BorradorSorteoRegistradoResponse>> RegistrarSorteoAsync(
                    CrearSorteoBorradorRequest request,
                    Guid administradorId,
                    CancellationToken cancellationToken = default);


        Task<Result<ResultadoPaginado<SorteoListadoResponse>>> ListarSorteosAsync(
            ListarSorteosRequest request,
            CancellationToken cancellationToken = default);


        Task<Result<SorteoListadoResponse>> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );

        Task<Result<SorteoPublicadoResponse>> PublicarSorteoAsync(
            Guid sorteoId,
            Guid administradorId,
            CancellationToken cancellationToken = default
        );

        Task<Result<SorteoListadoResponse>> ActualizarBorradorAsync(
            Guid sorteoId,
            Guid administradorId,
            ActualizarSorteoBorradorRequest request,
            CancellationToken cancellationToken = default
        );
        Task<Result<PortadaSorteoResponse>> ActualizarPortadaAsync(
            Guid sorteoId,
            Guid AdministradorId,
            Stream imagen,
            long tamanoBytes,
            CancellationToken cancellationToken = default);
    }
}
