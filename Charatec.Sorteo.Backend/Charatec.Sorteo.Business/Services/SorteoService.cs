using System.ComponentModel.DataAnnotations;
using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.DTOs.Responses.Sorteos;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Extensions;
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
        #region LISTADO PAGINADO POR ESTADO
        public async Task<Result<ResultadoPaginado<SorteoListadoResponse>>>
            ListarSorteosAsync(
                ListarSorteosRequest request,
                CancellationToken cancellationToken = default)
        {
            var consulta = _uow._sorteoRepository.Consultar();

            if (request.Estado.HasValue)
            {
                consulta = consulta.Where(
                    x => x.Estado == request.Estado.Value
                );
            }

            consulta = consulta
                .OrderByDescending(x => x.FechaCreacion)
                .ThenByDescending(x => x.Id);

            var datosPaginados = await consulta.PaginarAsync(
                request.ObtenerPaginacion(),
                cancellationToken
            );

            var registros = datosPaginados.Registros
                .Select(s => new SorteoListadoResponse(
                    Id: s.Id,
                    AdministradorUsuarioId: s.AdministradorUsuarioId,
                    Titulo: s.Titulo,
                    Descripcion: s.Descripcion,
                    ImagenPortadaUrl: s.ImagenPortadaUrl,
                    AporteMinimo: s.AporteMinimo,
                    MetaRecaudacion: s.MetaRecaudacion,
                    FechaInicio: s.FechaInicio,
                    FechaCierre: s.FechaCierre,
                    FechaSorteo: s.FechaSorteo,
                    Estado: s.Estado.ToString(),
                    FechaCreacion: s.FechaCreacion
                ))
                .ToList();

            var respuesta = new ResultadoPaginado<SorteoListadoResponse>(
                Registros: registros,
                Pagina: datosPaginados.Pagina,
                TamanoPagina: datosPaginados.TamanoPagina,
                TotalRegistros: datosPaginados.TotalRegistros
            );

            return Result<ResultadoPaginado<SorteoListadoResponse>>
                .Success(respuesta);
        }
        #endregion
        #region BUSCAR POR ID
        public async Task<Result<SorteoListadoResponse>> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // 1. Validar identificador
            if (id == Guid.Empty)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "El identificador del sorteo no es válido."
                    )
                );
            }

            // 2. Consultar repositorio
            var sorteo = await _uow._sorteoRepository.ObtenerPorIdAsync(
                id,
                cancellationToken
            );

            // 3. Comprobar existencia
            if (sorteo is null)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "NOT_FOUND",
                        "No se encontró el sorteo solicitado."
                    )
                );
            }

            // 4. Construir DTO de respuesta
            var response = new SorteoListadoResponse(
                Id: sorteo.Id,
                AdministradorUsuarioId: sorteo.AdministradorUsuarioId,
                Titulo: sorteo.Titulo,
                Descripcion: sorteo.Descripcion,
                ImagenPortadaUrl: sorteo.ImagenPortadaUrl,
                AporteMinimo: sorteo.AporteMinimo,
                MetaRecaudacion: sorteo.MetaRecaudacion,
                FechaInicio: sorteo.FechaInicio,
                FechaCierre: sorteo.FechaCierre,
                FechaSorteo: sorteo.FechaSorteo,
                Estado: sorteo.Estado.ToString(),
                FechaCreacion: sorteo.FechaCreacion
            );

            // 5. Retornar resultado exitoso
            return Result<SorteoListadoResponse>.Success(response);
        }
        #endregion
        #region ACTUALIZAR EL BORRADOR 
        public async Task<Result<SorteoListadoResponse>>
            ActualizarBorradorAsync(
                Guid sorteoId,
                Guid administradorId,
                ActualizarSorteoBorradorRequest request,
                CancellationToken cancellationToken = default)
        {
            // 1. Validar identificadores
            if (sorteoId == Guid.Empty ||
                administradorId == Guid.Empty)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "Los identificadores no son válidos."
                    )
                );
            }

            // 2. Consultar entidad con seguimiento de EF Core
            var sorteo = await _uow._sorteoRepository.ObtenerParaActualizarAsync(
                sorteoId,
                cancellationToken
            );

            if (sorteo is null)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "NOT_FOUND",
                        "No se encontró el sorteo."
                    )
                );
            }

            // 3. Comprobar propietario
            if (sorteo.AdministradorUsuarioId != administradorId)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "FORBIDDEN",
                        "No tienes permisos para modificar este sorteo."
                    )
                );
            }

            // 4. Solo pueden modificarse borradores
            if (sorteo.Estado != EstadoSorteo.Borrador)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "Solo puedes modificar sorteos en estado Borrador."
                    )
                );
            }

            // 5. Actualizar los campos permitidos
            sorteo.Titulo = request.Titulo.Trim();
            sorteo.Descripcion = request.Descripcion.Trim();

            sorteo.AporteMinimo = request.AporteMinimo;
            sorteo.MetaRecaudacion = request.MetaRecaudacion;

            // PostgreSQL necesita DateTimeOffset en UTC
            sorteo.FechaInicio =
                request.FechaInicio!.Value.ToUniversalTime();

            sorteo.FechaCierre =
                request.FechaCierre!.Value.ToUniversalTime();

            sorteo.FechaSorteo =
                request.FechaSorteo?.ToUniversalTime();

            sorteo.FechaActualizacion = DateTimeOffset.UtcNow;

            // No modificar ImagenPortadaUrl aquí
            // No modificar Estado aquí

            // 6. Guardar en PostgreSQL
            await _uow.SaveChangesAsync(cancellationToken);

            // 7. Crear respuesta
            var response = new SorteoListadoResponse(
                Id: sorteo.Id,
                AdministradorUsuarioId: sorteo.AdministradorUsuarioId,
                Titulo: sorteo.Titulo,
                Descripcion: sorteo.Descripcion,
                ImagenPortadaUrl: sorteo.ImagenPortadaUrl,
                AporteMinimo: sorteo.AporteMinimo,
                MetaRecaudacion: sorteo.MetaRecaudacion,
                FechaInicio: sorteo.FechaInicio,
                FechaCierre: sorteo.FechaCierre,
                FechaSorteo: sorteo.FechaSorteo,
                Estado: sorteo.Estado.ToString(),
                FechaCreacion: sorteo.FechaCreacion
            );

            return Result<SorteoListadoResponse>.Success(response);
        }
        #endregion
        #region PUBLICAR CAMBIAR ESTADO DEL SORTEO 

        public async Task<Result<SorteoPublicadoResponse>> PublicarSorteoAsync(
            Guid sorteoId,
            Guid administradorId,
            CancellationToken cancellationToken = default)
        {

            if (sorteoId == Guid.Empty || administradorId == Guid.Empty)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "Los identificadores no son válidos."
                    )
                );
            }

            // 2. Obtener sorteo con seguimiento de EF Core
            var sorteo = await _uow._sorteoRepository.ObtenerParaActualizarAsync(
                sorteoId,
                cancellationToken
            );

            // 3. Comprobar existencia
            if (sorteo is null)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "NOT_FOUND",
                        "No se encontró el sorteo."
                    )
                );
            }

            // 4. Verificar que pertenece al administrador
            if (sorteo.AdministradorUsuarioId != administradorId)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "FORBIDDEN",
                        "No tienes permisos para publicar este sorteo."
                    )
                );
            }

            // 5. Solo se permite publicar borradores
            if (sorteo.Estado != EstadoSorteo.Borrador)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "Solo se pueden publicar sorteos en estado Borrador."
                    )
                );
            }

            // 6. Validar información obligatoria
            if (string.IsNullOrWhiteSpace(sorteo.Titulo) ||
                string.IsNullOrWhiteSpace(sorteo.Descripcion))
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "El sorteo necesita título y descripción."
                    )
                );
            }

            if (sorteo.AporteMinimo <= 0)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "El aporte mínimo debe ser mayor que cero."
                    )
                );
            }

            // 7. Validar fechas
            var ahora = DateTimeOffset.UtcNow;

            if (sorteo.FechaInicio <= ahora)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "La fecha de inicio debe ser futura."
                    )
                );
            }

            if (sorteo.FechaCierre <= sorteo.FechaInicio)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "La fecha de cierre debe ser posterior al inicio."
                    )
                );
            }

            if (sorteo.FechaSorteo.HasValue &&
                sorteo.FechaSorteo.Value <= sorteo.FechaCierre)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "La fecha del sorteo debe ser posterior al cierre."
                    )
                );
            }

            // 8. Confirmar publicación
            sorteo.Estado = EstadoSorteo.Publicado;
            sorteo.FechaActualizacion = ahora;

            // 9. Guardar cambios en PostgreSQL
            await _uow.SaveChangesAsync(cancellationToken);

            // 10. Devolver respuesta
            var response = new SorteoPublicadoResponse(
                sorteo.Id,
                sorteo.Titulo,
                sorteo.Estado.ToString(),
                sorteo.FechaActualizacion.Value
            );

            return Result<SorteoPublicadoResponse>.Success(response);
        }
        #endregion
        #region REGISTRAR SORTEO BORRADOR
        public async Task<Result<BorradorSorteoRegistradoResponse>> RegistrarSorteoAsync(
            CrearSorteoBorradorRequest request,
            Guid administradorId,
            CancellationToken cancellationToken = default)
        {
            if (administradorId == Guid.Empty)
            {
                return Result<BorradorSorteoRegistradoResponse>.Failure(Error.Validation(""));
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
                return Result<BorradorSorteoRegistradoResponse>.Failure(Error.Validation("Debe tener entre 5 a 180 caracateres"));
            }

            if (descripcion.Length < 10 || descripcion.Length > 3000)
            {
                return Result<BorradorSorteoRegistradoResponse>.Failure(Error.Validation("Debe tener entre 10 a 3000 caracateres"));
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
#endregion
    }
}
