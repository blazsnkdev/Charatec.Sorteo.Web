using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Data.Repositories
{
    public class AporteRepository : IAporteRepository
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

        public IQueryable<AporteRevisionConsulta> ConsultarPorSorteo(
            Guid sorteoId)
        {
            var consulta =
                from aporte in _dbContext.Set<Aporte>().AsNoTracking()

                join participacion in _dbContext.Set<Participacion>()
                    on aporte.ParticipacionId equals participacion.Id

                join cliente in _dbContext.Set<Cliente>()
                    on participacion.ClienteId equals cliente.Id

                join evidencia in _dbContext.Set<EvidenciaPagoYape>()
                    on aporte.Id equals evidencia.AporteId
                    into evidencias

                from evidencia in evidencias.DefaultIfEmpty()

                where participacion.SorteoId == sorteoId

                select new AporteRevisionConsulta
                {
                    AporteId = aporte.Id,

                    SorteoId = participacion.SorteoId,

                    ClienteId = cliente.Id,

                    Nombres = cliente.Nombres,
                    Apellidos = cliente.Apellidos,

                    NumeroWhatsapp = evidencia != null
                        ? evidencia.NumeroWhatsapp
                        : cliente.Telefono,

                    Monto = aporte.Monto,

                    CodigoOperacionYape = evidencia != null
                        ? evidencia.CodigoOperacionYape
                        : null,

                    Estado = aporte.Estado,

                    FechaCreacion = aporte.FechaCreacion
                };

            return consulta;
        }

    }
}
