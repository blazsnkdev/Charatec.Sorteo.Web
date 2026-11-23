using Charatec.Sorteo.Business.DTOs.Requests.Sorteos;
using Charatec.Sorteo.Business.DTOs.Responses.Sorteos;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Extensions;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Enums;
using Charatec.Sorteo.Entities.Models;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Charatec.Sorteo.Business.Services
{
    public class SorteoService : ISorteoService
    {
        private readonly IConfiguration _configuration;
        private readonly long _tamanoMaximo;
        private readonly IUnitOfWork _uow;
        private readonly IImagenStorageService _imagenStorageService;
        public SorteoService(
            IUnitOfWork uow,
            IImagenStorageService imagenStorageService,
            IConfiguration configuration)
        {
            _uow = uow;
            _imagenStorageService = imagenStorageService;
            _configuration = configuration;
            var tamanoMaximoMB =
        _configuration.GetValue<int>(
            "Imagenes:TamanoMaximoMB", 2);

            if (tamanoMaximoMB <= 0)
            {
                throw new InvalidOperationException(
                    "El tamaño máximo de imágenes debe ser mayor a cero.");
            }

            _tamanoMaximo = checked(
                (long)tamanoMaximoMB * 1024 * 1024);
        }
        #region LISTAR POR ESTADO PAGINADO
        public async Task<Result<ResultadoPaginado<SorteoListadoResponse>>>ListarSorteosAsync(ListarSorteosRequest request, CancellationToken cancellationToken = default)
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
            
            if (id == Guid.Empty)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "El identificador del sorteo no es válido."
                    )
                );
            }

            
            var sorteo = await _uow._sorteoRepository.ObtenerPorIdAsync(
                id,
                cancellationToken
            );

            
            if (sorteo is null)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "NOT_FOUND",
                        "No se encontró el sorteo solicitado."
                    )
                );
            }
            return Result<SorteoListadoResponse>.Success(new SorteoListadoResponse(
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
            ));
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

            if (sorteo.AdministradorUsuarioId != administradorId)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "FORBIDDEN",
                        "No tienes permisos para modificar este sorteo."
                    )
                );
            }

            if (sorteo.Estado != EstadoSorteo.Borrador)
            {
                return Result<SorteoListadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "Solo puedes modificar sorteos en estado Borrador."
                    )
                );
            }

            
            sorteo.Titulo = request.Titulo.Trim();
            sorteo.Descripcion = request.Descripcion.Trim();

            sorteo.AporteMinimo = request.AporteMinimo;
            sorteo.MetaRecaudacion = request.MetaRecaudacion;

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

            var sorteo = await _uow._sorteoRepository.ObtenerParaActualizarAsync(
                sorteoId,
                cancellationToken
            );

            if (sorteo is null)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "NOT_FOUND",
                        "No se encontró el sorteo."
                    )
                );
            }

            if (sorteo.AdministradorUsuarioId != administradorId)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "FORBIDDEN",
                        "No tienes permisos para publicar este sorteo."
                    )
                );
            }

            if (sorteo.Estado != EstadoSorteo.Borrador)
            {
                return Result<SorteoPublicadoResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "Solo se pueden publicar sorteos en estado Borrador."
                    )
                );
            }

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

            sorteo.Estado = EstadoSorteo.Publicado;
            sorteo.FechaActualizacion = ahora;

            await _uow.SaveChangesAsync(cancellationToken);

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
            cancellationToken.ThrowIfCancellationRequested();

            if (request is null)
            {
                return Result<BorradorSorteoRegistradoResponse>
                    .Failure(Error.Validation(
                        "Los datos del sorteo son obligatorios."
                    ));
            }

            if (administradorId == Guid.Empty)
            {
                return Result<BorradorSorteoRegistradoResponse>
                    .Failure(Error.Validation(
                        "El identificador del administrador no es válido."
                    ));
            }

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
                    resultadosValidacion
                        .Select(x => x.ErrorMessage)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                );

                return Result<BorradorSorteoRegistradoResponse>
                    .Failure(Error.Validation(errores));
            }


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

            var sorteo = new TblSorteo
            {
                Id = Guid.NewGuid(),
                AdministradorUsuarioId = administradorId,
                Titulo = titulo,
                Descripcion = descripcion,
                ImagenPortadaUrl = imagenPortadaUrl,
                AporteMinimo = request.AporteMinimo,
                MetaRecaudacion = request.MetaRecaudacion,
                FechaInicio = request.FechaInicio!.Value.ToUniversalTime(),
                FechaCierre = request.FechaCierre!.Value.ToUniversalTime(),
                FechaSorteo = request.FechaSorteo?.ToUniversalTime(),
                Estado = EstadoSorteo.Borrador,
                FechaCreacion = DateTimeOffset.UtcNow,
                FechaActualizacion = null
            };

            await _uow._sorteoRepository.AddAsync(
                sorteo,
                cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result<BorradorSorteoRegistradoResponse>.Success(
                new BorradorSorteoRegistradoResponse(
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
            ));
        }

        #endregion
        public async Task<Result<PortadaSorteoResponse>> ActualizarPortadaAsync(
            Guid sorteoId,
            Guid administradorId,
            Stream imagen,
            long tamanoBytes,
            CancellationToken cancellationToken = default)
        {
            // 1. Validar identificador.
            if (sorteoId == Guid.Empty)
            {
                return Result<PortadaSorteoResponse>.Failure(
                    new Error("Sorteo.IdInvalido", "El ID del sorteo no es válido."));
            }



            // 2. Validar tamaño.
            if (tamanoBytes <= 0 || tamanoBytes > _tamanoMaximo)
            {
                return Result<PortadaSorteoResponse>.Failure(
                    new Error(
                        "Portada.TamanoInvalido",
                        "La imagen debe tener un tamaño máximo de 2 MB."));
            }

            if (imagen is null || !imagen.CanRead)
            {
                return Result<PortadaSorteoResponse>.Failure(
                    new Error(
                        "Portada.ArchivoInvalido",
                        "Debes proporcionar una imagen válida."));
            }

            // 3. Buscar sorteo usando el repositorio.
            // Ajustar al nombre real de tu repositorio y método.
            var sorteo = await _uow._sorteoRepository
                .ObtenerPorIdAsync(sorteoId, cancellationToken);

            if (sorteo is null)
            {
                return Result<PortadaSorteoResponse>.Failure(
                    new Error(
                        "Sorteo.NoEncontrado",
                        "No se encontró el sorteo seleccionado."));
            }

            if (sorteo.AdministradorUsuarioId != administradorId)
            {
                return Result<PortadaSorteoResponse>.Failure(
                    new Error(
                        "Sorteo.SinPermiso",
                        "No tienes permiso para modificar este sorteo."
                    )
                );
            }
            // 4. Verificar la firma del archivo.
            // Leer el archivo para identificar su formato real.
            await using var memoria = new MemoryStream();

            await imagen.CopyToAsync(memoria, cancellationToken);

            if (memoria.Length == 0 || memoria.Length > _tamanoMaximo)
            {
                return Result<PortadaSorteoResponse>.Failure(
                    new Error(
                        "Portada.TamanoInvalido",
                        "La imagen debe tener un tamaño máximo de 2 MB."));
            }

            var extension = DetectarExtension(memoria.ToArray());

            if (extension is null)
            {
                return Result<PortadaSorteoResponse>.Failure(
                    new Error(
                        "Portada.FormatoInvalido",
                        "Solamente se permiten imágenes JPG, PNG o WEBP."));
            }

            memoria.Position = 0;

            // 5. Guardar la imagen en almacenamiento local.
            var rutaAnterior = sorteo.ImagenPortadaUrl;

            var rutaNueva = await _imagenStorageService.GuardarPortadaAsync(
                sorteoId,
                memoria,
                extension,
                cancellationToken);

            try
            {
                // 6. Actualizar solamente los campos necesarios.
                sorteo.ImagenPortadaUrl = rutaNueva;
                sorteo.FechaActualizacion = DateTimeOffset.UtcNow;

                // 7. Persistir mediante Unit of Work.
                await _uow.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Si PostgreSQL falla, borrar el archivo nuevo.
                await _imagenStorageService.EliminarPortadaAsync(
                    rutaNueva,
                    CancellationToken.None);

                throw;
            }

            // 8. Borrar portada anterior una vez guardada la nueva.
            // Solo borrar archivos del almacenamiento local.
            if (!string.IsNullOrWhiteSpace(rutaAnterior) &&
                rutaAnterior.StartsWith(
                    "/uploads/sorteos/",
                    StringComparison.Ordinal))
            {
                try
                {
                    await _imagenStorageService.EliminarPortadaAsync(
                        rutaAnterior,
                        cancellationToken);
                }
                catch
                {
                    // El cambio en BD ya se guardó.
                    // Aquí conviene registrar un warning con ILogger.
                }
            }

            // 9. Devolver resultado.
            return Result<PortadaSorteoResponse>.Success(
                new PortadaSorteoResponse(
                    sorteoId,
                    rutaNueva));
        }

        private static string? DetectarExtension(byte[] bytes)
        {
            if (bytes.Length >= 3 &&
                bytes[0] == 0xFF &&
                bytes[1] == 0xD8 &&
                bytes[2] == 0xFF)
            {
                return ".jpg";
            }

            ReadOnlySpan<byte> firmaPng =
                stackalloc byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

            if (bytes.AsSpan().StartsWith(firmaPng))
            {
                return ".png";
            }

            if (bytes.Length >= 12 &&
                bytes[0] == (byte)'R' &&
                bytes[1] == (byte)'I' &&
                bytes[2] == (byte)'F' &&
                bytes[3] == (byte)'F' &&
                bytes[8] == (byte)'W' &&
                bytes[9] == (byte)'E' &&
                bytes[10] == (byte)'B' &&
                bytes[11] == (byte)'P')
            {
                return ".webp";
            }

            return null;
        }
    }
}
