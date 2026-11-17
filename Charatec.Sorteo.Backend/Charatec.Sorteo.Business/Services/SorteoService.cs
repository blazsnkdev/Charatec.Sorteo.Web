
using System.ComponentModel.DataAnnotations;
using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.DTOs.Responses.Sorteos;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Enums;
using Charatec.Sorteo.Entities.Models;

namespace Charatec.Sorteo.Business.Services
{
    public class SorteoService : ISorteoService
    {
        private readonly IUnitOfWork _uow;

        public SorteoService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<BorradorSorteoRegistradoResponse>> RegistrarSorteoAsync(
            CrearSorteoBorradorRequest request,
            Guid administradorId,
            CancellationToken cancellationToken = default)
        {
            if (administradorId == Guid.Empty)
            {
                throw new ValidationException(//TODO: corregir a futuro
                    "El identificador del administrador no es válido.");
            }
            ArgumentNullException.ThrowIfNull(request);

            var resultadosValidacion = new List<ValidationResult>();

            var contextoValidacion = new ValidationContext(request);

            bool esValido = Validator.TryValidateObject(
                request,
                contextoValidacion,
                resultadosValidacion,
                validateAllProperties: true);

            if (!esValido)
            {
                var errores = string.Join(
                    "; ",
                    resultadosValidacion.Select(x => x.ErrorMessage));

                throw new ValidationException(errores);
            }

            // 3. Normalizar los campos de texto
            var titulo = request.Titulo.Trim();
            var descripcion = request.Descripcion.Trim();

            var imagenPortadaUrl =
                string.IsNullOrWhiteSpace(request.ImagenPortadaUrl)
                    ? null
                    : request.ImagenPortadaUrl.Trim();

            if (titulo.Length < 5 || titulo.Length > 180)
            {
                throw new ValidationException(
                    "El título debe tener entre 5 y 180 caracteres.");
            }

            if (descripcion.Length < 10 || descripcion.Length > 3000)
            {
                throw new ValidationException(
                    "La descripción debe tener entre 10 y 3000 caracteres.");
            }

            // 4. Crear entidad Sorteo
            var sorteo = new TblSorteo
            {
                Id = Guid.NewGuid(),

                AdministradorUsuarioId = administradorId,

                Titulo = titulo,
                Descripcion = descripcion,
                ImagenPortadaUrl = imagenPortadaUrl,

                AporteMinimo = request.AporteMinimo,
                MetaRecaudacion = request.MetaRecaudacion,

                //TODO: parseo de las fechas
                FechaInicio = request.FechaInicio!.Value.ToUniversalTime(),
                FechaCierre = request.FechaCierre!.Value.ToUniversalTime(),
                FechaSorteo = request.FechaSorteo?.ToUniversalTime(),


                Estado = EstadoSorteo.Borrador,

                FechaCreacion = DateTimeOffset.UtcNow,
                FechaActualizacion = null
            };

            // 5. Agregar el sorteo al repositorio
            await _uow._sorteoRepository.AddAsync(
                sorteo,
                cancellationToken);

            // 6. Confirmar la operación en PostgreSQL
            await _uow.SaveChangesAsync(cancellationToken);

            // 7. Crear DTO de respuesta
            var response = new BorradorSorteoRegistradoResponse(
                sorteo.Id,
                sorteo.AdministradorUsuarioId,
                sorteo.Titulo,
                sorteo.Descripcion,
                sorteo.ImagenPortadaUrl,
                sorteo.AporteMinimo,
                sorteo.MetaRecaudacion,
                sorteo.FechaInicio,
                sorteo.FechaCierre,
                sorteo.FechaSorteo,
                sorteo.Estado.ToString(),
                sorteo.FechaCreacion
            );

            // 8. Retornar resultado de negocio
            return Result<BorradorSorteoRegistradoResponse>.Success(
                response);
        }
    }
}
