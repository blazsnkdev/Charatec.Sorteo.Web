using Charatec.Sorteo.Entities.Models;

namespace Charatec.Sorteo.Data.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<bool> ExistePorEmailAsync(string email);
    }
}
