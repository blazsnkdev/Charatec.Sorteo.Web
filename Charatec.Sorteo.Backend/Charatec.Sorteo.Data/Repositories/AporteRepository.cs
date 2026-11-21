using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Enums;
using Charatec.Sorteo.Entities.Models;
using Microsoft.EntityFrameworkCore;

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

        public IQueryable<AporteRevisionConsulta> ConsultarPorSorteo(Guid sorteoId)
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
        public async Task<RevisionAporteResultado> VerificarAsync(
            Guid aporteId,
            Guid administradorUsuarioId,
            bool aprobar,
            string? observacion,
            CancellationToken cancellationToken = default)
        {
            var ahora = DateTimeOffset.UtcNow;

            RevisionAporteResultado Resultado(
                ResultadoRevisionAporte codigo,
                decimal monto = 0m,
                EstadoAporte estado = EstadoAporte.Pendiente,
                DateTimeOffset? fecha = null)
            {
                return new RevisionAporteResultado(
                    codigo,
                    aporteId,
                    monto,
                    estado,
                    fecha
                );
            }

            await using var transaccion =
                await _dbContext.Database.BeginTransactionAsync(
                    cancellationToken
                );

            var aporte = await _dbContext.Set<Aporte>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == aporteId,
                    cancellationToken
                );

            if (aporte is null)
            {
                return Resultado(
                    ResultadoRevisionAporte.NoEncontrado
                );
            }

            if (aporte.Estado != EstadoAporte.Pendiente)
            {
                return Resultado(
                    ResultadoRevisionAporte.YaRevisado,
                    aporte.Monto,
                    aporte.Estado,
                    aporte.FechaConfirmacion
                );
            }

            var orden = await _dbContext.Set<OrdenPago>()
                .FirstOrDefaultAsync(
                    x => x.Id == aporte.OrdenPagoId,
                    cancellationToken
                );

            var evidencia = await _dbContext.Set<EvidenciaPagoYape>()
                .FirstOrDefaultAsync(
                    x => x.AporteId == aporteId,
                    cancellationToken
                );

            if (orden is null || evidencia is null)
            {
                return Resultado(
                    ResultadoRevisionAporte.EvidenciaNoEncontrada,
                    aporte.Monto
                );
            }

            if (orden.Estado != EstadoOrdenPago.Pendiente ||
                orden.ClienteId == Guid.Empty)
            {
                return Resultado(
                    ResultadoRevisionAporte.YaRevisado,
                    aporte.Monto,
                    aporte.Estado
                );
            }

            var nuevoEstado = aprobar
                ? EstadoAporte.Confirmado
                : EstadoAporte.Rechazado;

            int filasActualizadas;

            if (aprobar)
            {
                filasActualizadas = await _dbContext.Set<Aporte>()
                    .Where(x =>
                        x.Id == aporteId &&
                        x.Estado == EstadoAporte.Pendiente
                    )
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                x => x.Estado,
                                EstadoAporte.Confirmado
                            )
                            .SetProperty(
                                x => x.FechaConfirmacion,
                                (DateTimeOffset?)ahora
                            ),
                        cancellationToken
                    );
            }
            else
            {
                filasActualizadas = await _dbContext.Set<Aporte>()
                    .Where(x =>
                        x.Id == aporteId &&
                        x.Estado == EstadoAporte.Pendiente
                    )
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                x => x.Estado,
                                EstadoAporte.Rechazado
                            )
                            .SetProperty(
                                x => x.FechaConfirmacion,
                                (DateTimeOffset?)null
                            ),
                        cancellationToken
                    );
            }
            if (filasActualizadas == 0)
            {
                return Resultado(
                    ResultadoRevisionAporte.YaRevisado,
                    aporte.Monto,
                    aporte.Estado
                );
            }

            orden.Estado = aprobar
                ? EstadoOrdenPago.Pagada
                : EstadoOrdenPago.Fallida;

            orden.FechaActualizacion = ahora;
            evidencia.Verificado = aprobar;
            evidencia.FechaVerificacion = ahora;//TODO: aqui lo esta seteando la fecha de verificacion
            evidencia.AdministradorRevisorId =
                administradorUsuarioId;
            evidencia.Observacion = observacion;


            if (aprobar)
            {
                var participacion = await _dbContext.Set<Participacion>()
                    .FirstOrDefaultAsync(
                        x => x.Id == aporte.ParticipacionId,
                        cancellationToken
                    );

                if (participacion is null)
                {
                    await transaccion.RollbackAsync(cancellationToken);

                    return Resultado(
                        ResultadoRevisionAporte.ParticipacionNoEncontrada,
                        aporte.Monto,
                        aporte.Estado
                    );
                }

                participacion.Estado = EstadoParticipacion.Habilitada;
            }


            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaccion.CommitAsync(cancellationToken);

            return Resultado(
                ResultadoRevisionAporte.Actualizado,
                aporte.Monto,
                nuevoEstado,
                ahora
            );
        }
    }
}
