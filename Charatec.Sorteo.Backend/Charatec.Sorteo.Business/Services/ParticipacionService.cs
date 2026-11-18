
using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Business.Options;
using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Interfaces;
using Microsoft.Extensions.Options;

namespace Charatec.Sorteo.Business.Services
{
    public sealed class ParticipacionService : IParticipacionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ParticipacionOptions _options;

        public ParticipacionService(
            IUnitOfWork unitOfWork,
            IOptions<ParticipacionOptions> options)
        {
            _unitOfWork = unitOfWork;
            _options = options.Value;
        }

        public async Task<Result<PrepararParticipacionResponse>> ObtenerAsync(
            Guid usuarioId,
            string? email,
            Guid sorteoId,
            CancellationToken cancellationToken = default)
        {
            // ==========================================
            // 1. CONSULTAR EL SORTEO PUBLICADO
            // ==========================================

            var sorteo = await _unitOfWork._sorteoRepository
                .ObtenerPublicadoPorIdAsync(
                    sorteoId,
                    cancellationToken
                );

            if (sorteo is null)
            {
                return Result<PrepararParticipacionResponse>
                    .Failure(Error.NotFound("El sorteo no existe"));
            }

            // ==========================================
            // 2. CONFIGURACIÓN DEL APORTE
            // ==========================================

            decimal aporteMinimo = sorteo.AporteMinimo;
            decimal aporteMaximo = _options.AporteMaximo;
            decimal pasoAporte = _options.PasoAporte;

            bool configuracionValida =
                aporteMinimo > 0 &&
                aporteMaximo >= aporteMinimo &&
                pasoAporte > 0;

            // ==========================================
            // 3. VALIDAR FECHAS DEL SORTEO
            // ==========================================

            var fechaActual = DateTimeOffset.UtcNow;

            bool sorteoIniciado =
                fechaActual >= sorteo.FechaInicio;

            bool sorteoNoCerrado =
                fechaActual < sorteo.FechaCierre;

            bool dentroDelPeriodo =
                sorteoIniciado && sorteoNoCerrado;

            bool puedeParticipar =
                configuracionValida && dentroDelPeriodo;

            // ==========================================
            // 4. MENSAJE DE DISPONIBILIDAD
            // ==========================================

            string mensajeDisponibilidad;

            if (!configuracionValida)
            {
                mensajeDisponibilidad =
                    "La configuración del aporte no es válida.";
            }
            else if (!sorteoIniciado)
            {
                mensajeDisponibilidad =
                    "El sorteo todavía no ha comenzado.";
            }
            else if (!sorteoNoCerrado)
            {
                mensajeDisponibilidad =
                    "El periodo de participación ha finalizado.";
            }
            else
            {
                mensajeDisponibilidad =
                    "El sorteo está disponible para participar.";
            }

            // ==========================================
            // 5. CONSTRUIR RESPUESTA
            // ==========================================

            var response = new PrepararParticipacionResponse
            {
                SorteoId = sorteo.Id,

                Titulo = sorteo.Titulo,
                Descripcion = sorteo.Descripcion,
                ImagenPortadaUrl = sorteo.ImagenPortadaUrl,

                FechaInicio = sorteo.FechaInicio,
                FechaCierre = sorteo.FechaCierre,
                FechaSorteo = sorteo.FechaSorteo,

                AporteMinimo = aporteMinimo,
                AporteMaximo = aporteMaximo,
                PasoAporte = pasoAporte,
                MontoInicial = aporteMinimo,

                MetaRecaudacion = sorteo.MetaRecaudacion,

                UsuarioId = usuarioId,
                Email = email,

                PuedeParticipar = puedeParticipar,
                MensajeDisponibilidad = mensajeDisponibilidad,

                Moneda = "PEN"
            };

            // ==========================================
            // 6. RETORNAR RESULTADO
            // ==========================================

            return Result<PrepararParticipacionResponse>
                .Success(response);
        }
    }
}
