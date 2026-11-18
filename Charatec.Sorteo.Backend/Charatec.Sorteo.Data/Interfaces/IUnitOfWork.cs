using Charatec.Sorteo.Common.Interfaces;

namespace Charatec.Sorteo.Data.Interfaces
{
    public interface IUnitOfWork
    {
        public ISorteoRepository _sorteoRepository { get; }
        public IParticipacionYapeRepository _participacionesYape { get; }
        Task BeginTransactionAsync();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync();

        Task RollBackAsync();
    }
}
