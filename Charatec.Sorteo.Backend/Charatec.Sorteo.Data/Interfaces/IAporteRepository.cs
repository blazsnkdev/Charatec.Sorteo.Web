using Charatec.Sorteo.Entities.Models;

namespace Charatec.Sorteo.Data.Interfaces
{
    public interface IAporteRepository
    {
        IQueryable<Aporte> Consultar();

        IQueryable<AporteRevisionConsulta> ConsultarPorSorteo(
            Guid sorteoId
        );
        Task<RevisionAporteResultado> VerificarAsync(
            Guid aporteId,
            Guid administradorUsuarioId,
            bool aprobar,
            string? observacion,
            CancellationToken cancellationToken = default
        );

    }
}
