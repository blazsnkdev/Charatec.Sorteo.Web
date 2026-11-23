using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Enums;
using Charatec.Sorteo.Entities.Models;
using Microsoft.EntityFrameworkCore;
namespace Charatec.Sorteo.Data.Repositories
{
    public class SorteoRepository : Repository<TblSorteo>, ISorteoRepository
    {
        private readonly SorteoDbContext _dbContext;
        public SorteoRepository(SorteoDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<TblSorteo> Consultar()
        {
            return _dbContext.Set<TblSorteo>().AsNoTracking();
        }

        public async Task<TblSorteo?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TblSorteo>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );
        }

        public Task<TblSorteo?> ObtenerParaActualizarAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<TblSorteo>()
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken
                );
        }

        public async Task<TblSorteo?> ObtenerPublicadoPorIdAsync(Guid sorteoId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TblSorteo>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.Id == sorteoId &&
                     s.Estado == EstadoSorteo.Publicado,
                cancellationToken
            );
        }


        public async Task<TblSorteo?> ObtenerPorIdParaActualizarAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TblSorteo>()
                .AsTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken
                );
        }

    }
}
