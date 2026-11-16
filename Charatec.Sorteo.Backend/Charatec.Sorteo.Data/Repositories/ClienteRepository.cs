using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Data.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        private readonly SorteoDbContext _dbContext;
        private readonly ILookupNormalizer _normalizer;

        public ClienteRepository(
              SorteoDbContext dbContext,
              ILookupNormalizer normalizer) : base(dbContext)
        {
            _dbContext = dbContext;
            _normalizer = normalizer;
        }
        public async Task<bool> ExistePorEmailAsync(string email)
        {
            var emailNormalizado = _normalizer.NormalizeEmail(email.Trim());

            return await _dbContext.Users
                .AnyAsync(x => x.NormalizedEmail == emailNormalizado);
        }
    }
}
