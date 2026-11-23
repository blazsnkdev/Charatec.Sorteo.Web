using Charatec.Sorteo.Entities.Models;

namespace Charatec.Sorteo.Data.Interfaces
{
    public interface ISorteoRepository : IRepository<TblSorteo>
    {
        IQueryable<TblSorteo> Consultar();

        Task<TblSorteo?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );

        Task<TblSorteo?> ObtenerParaActualizarAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );
        Task<TblSorteo?> ObtenerPublicadoPorIdAsync(
        Guid sorteoId,
        CancellationToken cancellationToken = default
        );

        Task<TblSorteo?> ObtenerPorIdParaActualizarAsync(
            Guid id,
            CancellationToken cancellationToken = default);


    }
}
