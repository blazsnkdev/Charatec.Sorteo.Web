using Charatec.Sorteo.Data.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace Charatec.Sorteo.Data.Interfaces
{
    internal class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly SorteoDbContext _dbContext;
        private IDbContextTransaction? _transaction;
        public UnitOfWork(SorteoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Task BeginTransactionAsync()
        {
            throw new NotImplementedException();
        }

        public Task CommitTransactionAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task RollBackAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
