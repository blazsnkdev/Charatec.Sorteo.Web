using Charatec.Sorteo.Common.Models;

namespace Charatec.Sorteo.Common.Interfaces
{
    public interface IParticipacionYapeRepository
    {
        Task<RegistroPagoYapeData> RegistrarPendienteAsync(
            RegistrarPagoYapeData solicitud,
            CancellationToken cancellationToken = default
        );
    }
}
