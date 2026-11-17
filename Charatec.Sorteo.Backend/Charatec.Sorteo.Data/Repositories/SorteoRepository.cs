using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Data.Repositories
{
    public class SorteoRepository : Repository<TblSorteo>, ISorteoRepository
    {
        public SorteoRepository(SorteoDbContext dbContext) : base(dbContext)
        {
        }
    }
}
