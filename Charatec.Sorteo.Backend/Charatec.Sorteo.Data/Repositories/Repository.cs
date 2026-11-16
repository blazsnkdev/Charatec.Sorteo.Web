using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly SorteoDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(SorteoDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
