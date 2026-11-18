using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Data.Repositories
{
    internal class AporteRepository : IAporteRepository
    {
        private readonly SorteoDbContext _dbContext;

        public AporteRepository(SorteoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        //TODO: este metodo es para el paginado as not tragking no trae ref de otras tablers
        public IQueryable<Aporte> Consultar()
        {
            return _dbContext.Set<Aporte>().AsNoTracking();
        }
    }
}
