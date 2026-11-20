using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Business.Options;
using Charatec.Sorteo.Common.Models;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

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
        public async Task<Result<PrepararParticipacionResponse>> ObtenerAsync(Guid usuarioId, string? email, Guid sorteoId, CancellationToken cancellationToken = default)
        {
            var sorteo = await _unitOfWork._sorteoRepository
                .ObtenerPublicadoPorIdAsync(
                    sorteoId,
                    cancellationToken
                );

            if (sorteo is null)
            {
                return Result<PrepararParticipacionResponse>.Failure(
                    Error.NotFound(
                        "El sorteo no existe o no se encuentra publicado."
                    )
                );
            }


            decimal aporteMinimo = sorteo.AporteMinimo;
            decimal aporteMaximo = _options.AporteMaximo;
            decimal pasoAporte = _options.PasoAporte;

            bool configuracionValida =
                aporteMinimo > 0 &&
                aporteMaximo >= aporteMinimo &&
                pasoAporte > 0;

            var fechaActual = DateTimeOffset.UtcNow;

            bool sorteoIniciado =
                fechaActual >= sorteo.FechaInicio;

            bool sorteoNoCerrado =
                fechaActual < sorteo.FechaCierre;

            bool fechasValidas =
                sorteo.FechaInicio < sorteo.FechaCierre;

            bool puedeParticipar =
                configuracionValida &&
                fechasValidas &&
                sorteoIniciado &&
                sorteoNoCerrado;

            string mensajeDisponibilidad;

            if (!configuracionValida)
            {
                mensajeDisponibilidad =
                    "La configuración del aporte no es válida.";
            }
            else if (!fechasValidas)
            {
                mensajeDisponibilidad =
                    "Las fechas del sorteo no son válidas.";
            }
            else if (!sorteoIniciado)
            {
                mensajeDisponibilidad =
                    "El periodo de participación todavía no ha comenzado.";
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

            return Result<PrepararParticipacionResponse>
                .Success(response);
        }
        #endregion

        #region REGISTRAR PAGO DE YAPE PENDIENTE A VALIDACINO DEL ADMIN

        public async Task<Result<RegistrarParticipacionYapeResponse>>RegistrarAsync(Guid usuarioId, Guid sorteoId, RegistrarParticipacionYapeRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request is null ||
                usuarioId == Guid.Empty ||
                sorteoId == Guid.Empty ||
                request.SolicitudId == Guid.Empty)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        "La solicitud contiene identificadores inválidos."
                    ));
            }

            if (!request.AceptaTerminos)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        "Debes aceptar los términos de participación."
                    ));
            }

            var codigoYape =
                request.CodigoOperacionYape?.Trim() ?? string.Empty;

            if (!Regex.IsMatch(codigoYape, @"^[0-9]{8}$"))
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        "El código de operación Yape debe contener exactamente 8 dígitos numéricos."
                    ));
            }

            var whatsapp =
                request.NumeroWhatsapp?.Trim() ?? string.Empty;

            var email =
                request.Email?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(whatsapp))
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        "Ingresa tu número de WhatsApp."
                    ));
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        "Ingresa tu correo electrónico."
                    ));
            }

            var sorteo = await _unitOfWork._sorteoRepository
                .ObtenerPublicadoPorIdAsync(
                    sorteoId,
                    cancellationToken
                );

            if (sorteo is null)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.NotFound(
                        "El sorteo no existe o no está publicado."
                    ));
            }

            var ahora = DateTimeOffset.UtcNow;

            if (sorteo.FechaInicio >= sorteo.FechaCierre)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        "Las fechas del sorteo no son válidas."
                    ));
            }

            if (ahora < sorteo.FechaInicio ||
                ahora >= sorteo.FechaCierre)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        "El sorteo no está dentro del periodo habilitado."
                    ));
            }

            if (request.Monto < sorteo.AporteMinimo ||
                request.Monto > _options.AporteMaximo ||
                decimal.Round(request.Monto, 2) != request.Monto)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        "El monto seleccionado no es válido."
                    ));
            }

            var solicitud = new RegistrarPagoYapeData(
                UsuarioId: usuarioId,
                SorteoId: sorteoId,
                SolicitudId: request.SolicitudId,
                Monto: request.Monto,
                NumeroWhatsapp: whatsapp,
                Email: email,
                CodigoOperacionYape: codigoYape
            );


            var registro = await _unitOfWork._participacionesYape
                .RegistrarPendienteAsync(
                    solicitud,
                    cancellationToken
                );

            if (!registro.Exitoso)
            {
                return Result<RegistrarParticipacionYapeResponse>
                    .Failure(Error.Validation(
                        registro.MensajeError ??
                        "No se pudo registrar el pago Yape."
                    ));
            }

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

                Mensaje =
                    "Tu pago por Yape está pendiente de verificación."
            };

            return Result<RegistrarParticipacionYapeResponse>
                .Success(response);
        }

        #endregion
    }
}
