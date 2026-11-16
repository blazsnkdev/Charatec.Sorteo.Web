namespace Charatec.Sorteo.Data.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();

        Task<int> SaveChangesAsync();

        Task CommitTransactionAsync();

        Task RollBackAsync();
    }
}
