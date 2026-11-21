using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Common.Models;
using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Entities.Enums;
using Charatec.Sorteo.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Data.Repositories
{
    public sealed class ParticipacionYapeRepository(SorteoDbContext context) : IParticipacionYapeRepository
    {
        public async Task<RegistroPagoYapeData> RegistrarPendienteAsync(
            RegistrarPagoYapeData solicitud,
            CancellationToken cancellationToken = default)
        {
            await using var transaccion =
                await context.Database.BeginTransactionAsync(
                    cancellationToken
                );

            // 1. Detectar solicitud previamente registrada.
            var anterior = await context.Set<EvidenciaPagoYape>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.SolicitudId == solicitud.SolicitudId,
                    cancellationToken
                );

            if (anterior is not null)
            {
                var ordenAnterior = await context.Set<OrdenPago>()
                    .AsNoTracking()
                    .FirstAsync(
                        x => x.Id == anterior.OrdenPagoId,
                        cancellationToken
                    );

                if (anterior.UsuarioId != solicitud.UsuarioId ||
                    anterior.SorteoId != solicitud.SorteoId ||
                    ordenAnterior.Monto != solicitud.Monto ||
                    anterior.CodigoOperacionYape != solicitud.CodigoOperacionYape)
                {
                    throw new InvalidOperationException(
                        "La solicitud ya fue utilizada con otros datos."
                    );
                }

                return new RegistroPagoYapeData(
                    anterior.AporteId == Guid.Empty
                        ? throw new InvalidOperationException(
                            "El aporte anterior no es válido.")
                        : await context.Set<Aporte>()
                            .Where(x => x.Id == anterior.AporteId)
                            .Select(x => x.ParticipacionId)
                            .SingleAsync(cancellationToken),
                    anterior.OrdenPagoId,
                    anterior.AporteId,
                    anterior.SorteoId,
                    ordenAnterior.Monto
                );
            }

            // 2. Obtener el cliente real desde Identity.
            var cliente = await context.Set<Cliente>()
                .FirstOrDefaultAsync(
                    x => x.UsuarioId == solicitud.UsuarioId,
                    cancellationToken
                );

            if (cliente is null || !cliente.Activo)
            {
                throw new InvalidOperationException(
                    "El perfil de cliente no está habilitado."
                );
            }

            // 3. Revalidar sorteo y periodo.
            var sorteo = await context.Set<TblSorteo>()
                .FirstOrDefaultAsync(
                    x => x.Id == solicitud.SorteoId,
                    cancellationToken
                );

            var ahora = DateTimeOffset.UtcNow;

            if (sorteo is null ||
                sorteo.Estado != EstadoSorteo.Publicado ||
                ahora < sorteo.FechaInicio ||
                ahora >= sorteo.FechaCierre ||
                solicitud.Monto < sorteo.AporteMinimo)
            {
                throw new InvalidOperationException(
                    "El sorteo no admite esta participación."
                );
            }

            // 4. Evitar reutilizar códigos.
            var codigoUtilizado =
                await context.Set<EvidenciaPagoYape>()
                    .AnyAsync(
                        x => x.CodigoOperacionYape ==
                             solicitud.CodigoOperacionYape,
                        cancellationToken
                    );

            if (codigoUtilizado)
            {
                throw new InvalidOperationException(
                    "Este código de operación ya fue registrado."
                );
            }

            // 5. Reutilizar participación existente.
            var participacion = await context.Set<Participacion>()
                .FirstOrDefaultAsync(
                    x => x.ClienteId == cliente.Id &&
                         x.SorteoId == sorteo.Id,
                    cancellationToken
                );

            if (participacion is null)
            {
                participacion = new Participacion
                {
                    Id = Guid.NewGuid(),
                    ClienteId = cliente.Id,
                    SorteoId = sorteo.Id,
                    FechaInscripcion = ahora,
                    Estado = EstadoParticipacion.Inscrita
                };

                context.Set<Participacion>().Add(participacion);
            }

            // 6. Crear orden pendiente.
            var orden = new OrdenPago
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente.Id,
                Tipo = TipoOrdenPago.Aporte,
                Monto = solicitud.Monto,
                Moneda = "PEN",
                Estado = EstadoOrdenPago.Pendiente,
                IdempotencyKey = solicitud.SolicitudId.ToString("D")
            };

            context.Set<OrdenPago>().Add(orden);

            // 7. Crear aporte pendiente.
            var aporte = new Aporte
            {
                Id = Guid.NewGuid(),
                ParticipacionId = participacion.Id,
                OrdenPagoId = orden.Id,
                Monto = solicitud.Monto,
                Estado = EstadoAporte.Pendiente,
                FechaCreacion = ahora
            };

            context.Set<Aporte>().Add(aporte);

            // 8. Registrar declaración de Yape.
            var evidencia = new EvidenciaPagoYape
            {
                Id = Guid.NewGuid(),
                SolicitudId = solicitud.SolicitudId,
                UsuarioId = solicitud.UsuarioId,
                SorteoId = sorteo.Id,
                OrdenPagoId = orden.Id,
                AporteId = aporte.Id,
                NumeroWhatsapp = solicitud.NumeroWhatsapp,
                Email = solicitud.Email,
                CodigoOperacionYape = solicitud.CodigoOperacionYape,
                FechaRegistro = ahora,
                Verificado = false
            };

            context.Set<EvidenciaPagoYape>().Add(evidencia);

            // 9. Guardar todo de manera atómica.
            await context.SaveChangesAsync(cancellationToken);

            await transaccion.CommitAsync(cancellationToken);

            // 10. Responder.
            return new RegistroPagoYapeData(
                participacion.Id,
                orden.Id,
                aporte.Id,
                sorteo.Id,
                solicitud.Monto
            );
        }
    }
}
