using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Common.Results;

namespace Charatec.Sorteo.Business.Interfaces
{
    public interface IParticipacionService
    {
        Task<Result<PrepararParticipacionResponse>>
        ObtenerAsync(
            Guid usuarioId,
            string? email,
            Guid sorteoId,
            CancellationToken cancellationToken = default
        );

        Task<Result<RegistrarParticipacionYapeResponse>>
            RegistrarAsync(
                Guid usuarioId,
                Guid sorteoId,
                RegistrarParticipacionYapeRequest request,
                CancellationToken cancellationToken = default
            );

    }
}
