namespace Charatec.Sorteo.Data.Interfaces
{
    public interface IUnitOfWork
    {
        public ISorteoRepository _sorteoRepository { get; }
        Task BeginTransactionAsync();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync();

        Task RollBackAsync();
    }
}
