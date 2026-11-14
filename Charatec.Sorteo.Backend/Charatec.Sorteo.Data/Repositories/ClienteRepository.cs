using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Data.Repositories
{
    internal class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        private readonly SorteoDbContext _dbContext;
        public ClienteRepository(SorteoDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> ExistePorEmailAsync(string email)
        {
            return await _dbContext.Users.AnyAsync(x => x.Email == email);
        }
    }
}
