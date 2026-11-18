
using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Business.Options;
using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Common.Models;
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
        #region CONSULTAR SORTEO PARA PARTICIPAR
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
        #endregion

        public async Task<Result<RegistrarParticipacionYapeResponse>>
            RegistrarAsync(
                Guid usuarioId,
                Guid sorteoId,
                RegistrarParticipacionYapeRequest request,
                CancellationToken cancellationToken = default)
        {
            // ==========================================
            // 1. VALIDAR DATOS DEL FORMULARIO
            // ==========================================

            if (usuarioId == Guid.Empty ||
                sorteoId == Guid.Empty ||
                request.SolicitudId == Guid.Empty)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation("La solicitud contiene identificadores inválidos."));
            }

            if (!request.AceptaTerminos)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation("Debes aceptar los términos de participación."));
            }

            if (string.IsNullOrWhiteSpace(request.CodigoOperacionYape))
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation("Ingresa el código de operación Yape."));
            }

            // ==========================================
            // 2. VALIDAR SORTEO PUBLICADO
            // ==========================================

            var sorteo = await _unitOfWork._sorteoRepository
                .ObtenerPublicadoPorIdAsync(
                    sorteoId,
                    cancellationToken
                );

            if (sorteo is null)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.NotFound("El sorteo no existe o no está publicado."));
            }

            var ahora = DateTimeOffset.UtcNow;

            if (ahora < sorteo.FechaInicio ||
                ahora >= sorteo.FechaCierre)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation("El sorteo no está dentro del periodo habilitado."));
            }

            // ==========================================
            // 3. VALIDAR MONTO
            // ==========================================

            if (request.Monto < sorteo.AporteMinimo ||
                request.Monto > _options.AporteMaximo ||
                decimal.Round(request.Monto, 2) != request.Monto)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation("El monto seleccionado no es válido."));
            }

            // ==========================================
            // 4. REGISTRAR MEDIANTE IUNITOFWORK
            // ==========================================

            var solicitud = new RegistrarPagoYapeData(
                UsuarioId: usuarioId,
                SorteoId: sorteoId,
                SolicitudId: request.SolicitudId,
                Monto: request.Monto,
                NumeroWhatsapp: request.NumeroWhatsapp.Trim(),
                Email: request.Email.Trim(),
                CodigoOperacionYape: request.CodigoOperacionYape.Trim()
            );

            var registro = await _unitOfWork._participacionesYape
                .RegistrarPendienteAsync(
                    solicitud,
                    cancellationToken
                );

            // ==========================================
            // 5. CONSTRUIR RESPUESTA
            // ==========================================

            var response = new RegistrarParticipacionYapeResponse
            {
                ParticipacionId = registro.ParticipacionId,
                SorteoId = registro.SorteoId,
                OrdenPagoId = registro.OrdenPagoId,
                AporteId = registro.AporteId,
                Monto = registro.Monto,
                Moneda = "PEN",
                EstadoAporte = "Pendiente",
                EstadoVerificacion = "Pendiente",
                Mensaje = "Tu pago por Yape está pendiente de verificación."
            };

            return Result<RegistrarParticipacionYapeResponse>
                .Success(response);
        }

    }
}
