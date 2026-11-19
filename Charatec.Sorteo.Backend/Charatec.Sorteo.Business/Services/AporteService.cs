using Charatec.Sorteo.Business.DTOs.Requests.Aportes;
using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Aportes;
using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Ruleta;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Extensions;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Business.Services
{
    public class AporteService : IAporteService
    {
        private readonly IUnitOfWork _uow;
        public AporteService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<Result<ResultadoPaginado<AporteListadoResponse>>> ListarMisAportesAsync(Guid usuarioId, ListarAportesRequest request, CancellationToken cancellationToken = default)
        {
            var consulta = _uow._aporteRepository
                                .Consultar()
                                .Where(a =>
                                    a.Participacion.Cliente.UsuarioId == usuarioId
                                );

            // ==========================================
            // 2. FILTRAR POR ESTADO (OPCIONAL)
            // ==========================================

            if (request.Estado.HasValue)
            {
                consulta = consulta.Where(
                    a => a.Estado == request.Estado.Value
                );
            }

            // ==========================================
            // 3. ORDENAR POR FECHA
            // ==========================================

            var consultaListado = consulta
                .OrderByDescending(a => a.FechaCreacion)
                .ThenByDescending(a => a.Id)

                // Proyectar antes de paginar:
                // EF Core obtiene solo los campos necesarios.
                .Select(a => new
                {
                    a.Id,
                    SorteoId = a.Participacion.SorteoId,
                    TituloSorteo = a.Participacion.Sorteo.Titulo,
                    a.Monto,
                    a.Estado,
                    a.FechaCreacion
                });

            // ==========================================
            // 4. APLICAR PAGINACIÓN EXISTENTE
            // ==========================================

            var datosPaginados = await consultaListado.PaginarAsync(
                request.ObtenerPaginacion(),
                cancellationToken
            );

            // ==========================================
            // 5. MAPEAR AL DTO DE RESPUESTA
            // ==========================================

            var registros = datosPaginados.Registros
                .Select(a => new AporteListadoResponse(
                    Id: a.Id,
                    SorteoId: a.SorteoId,
                    TituloSorteo: a.TituloSorteo,
                    Monto: a.Monto,
                    Estado: a.Estado.ToString(),
                    FechaCreacion: a.FechaCreacion
                ))
                .ToList();

            // ==========================================
            // 6. CONSTRUIR RESPUESTA PAGINADA
            // ==========================================

            var respuesta =
                new ResultadoPaginado<AporteListadoResponse>(
                    Registros: registros,
                    Pagina: datosPaginados.Pagina,
                    TamanoPagina: datosPaginados.TamanoPagina,
                    TotalRegistros: datosPaginados.TotalRegistros
                );

            // ==========================================
            // 7. RETORNAR RESULTADO
            // ==========================================

            return Result<ResultadoPaginado<AporteListadoResponse>>
                .Success(respuesta);
        }

        public async Task<Result<ResultadoPaginado<AportePublicacionResponse>>>
            ListarPorPublicacionAsync(
                Guid sorteoId,
                ListarAportesPublicacionRequest request,
                CancellationToken cancellationToken = default)
        {
            // 1. Consultar los aportes de una publicación.
            var consulta = _uow._aporteRepository
                .ConsultarPorSorteo(sorteoId);

            // 2. Filtrar por estado.
            if (request.Estado.HasValue)
            {
                consulta = consulta.Where(
                    x => x.Estado == request.Estado.Value
                );
            }

            // 3. Ordenar.
            consulta = consulta
                .OrderByDescending(x => x.FechaCreacion)
                .ThenByDescending(x => x.AporteId);

            // 4. Paginar con tu extensión existente.
            var datosPaginados = await consulta.PaginarAsync(
                request.ObtenerPaginacion(),
                cancellationToken
            );

            // 5. Convertir a DTO.
            var registros = datosPaginados.Registros
                .Select(a => new AportePublicacionResponse(
                    AporteId: a.AporteId,
                    SorteoId: a.SorteoId,
                    ClienteId: a.ClienteId,
                    NombreCliente:
                        $"{a.Nombres} {a.Apellidos}".Trim(),
                    NumeroWhatsapp: a.NumeroWhatsapp,
                    Monto: a.Monto,
                    CodigoOperacionYape: a.CodigoOperacionYape,
                    Estado: a.Estado.ToString(),
                    FechaCreacion: a.FechaCreacion
                ))
                .ToList();

            // 6. Respuesta paginada.
            var respuesta =
                new ResultadoPaginado<AportePublicacionResponse>(
                    Registros: registros,
                    Pagina: datosPaginados.Pagina,
                    TamanoPagina: datosPaginados.TamanoPagina,
                    TotalRegistros: datosPaginados.TotalRegistros
                );

            return Result<ResultadoPaginado<AportePublicacionResponse>>
                .Success(respuesta);
        }

        public async Task<Result<VerificarAporteResponse>> VerificarAsync(
            Guid aporteId,
            Guid administradorUsuarioId,
            VerificarAporteRequest request,
            CancellationToken cancellationToken = default)
        {
            // ==========================================
            // 1. VALIDAR IDENTIFICADORES
            // ==========================================

            if (aporteId == Guid.Empty ||
                administradorUsuarioId == Guid.Empty)
            {
                return Result<VerificarAporteResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "Los identificadores no son válidos."
                    )
                );
            }

            // ==========================================
            // 2. VALIDAR DECISIÓN
            // ==========================================

            var decision = request.Decision?.Trim();

            if (decision != "Aprobar" &&
                decision != "Rechazar")
            {
                return Result<VerificarAporteResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "Debes seleccionar Aprobar o Rechazar."
                    )
                );
            }

            var aprobar = decision == "Aprobar";
            var observacion = request.Observacion?.Trim();

            if (!aprobar &&
                string.IsNullOrWhiteSpace(observacion))
            {
                return Result<VerificarAporteResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "Debes indicar el motivo del rechazo."
                    )
                );
            }

            // ==========================================
            // 3. EJECUTAR REVISIÓN
            // ==========================================

            var revision = await _uow._aporteRepository
                .VerificarAsync(
                    aporteId,
                    administradorUsuarioId,
                    aprobar,
                    observacion,
                    cancellationToken
                );

            // ==========================================
            // 4. RESULTADOS DE NEGOCIO
            // ==========================================

            if (revision.Resultado ==
                ResultadoRevisionAporte.NoEncontrado)
            {
                return Result<VerificarAporteResponse>.Failure(
                    new Error(
                        "NOT_FOUND",
                        "No se encontró el aporte."
                    )
                );
            }

            if (revision.Resultado ==
                ResultadoRevisionAporte.YaRevisado)
            {
                return Result<VerificarAporteResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "Este aporte ya fue revisado o no está pendiente."
                    )
                );
            }

            if (revision.Resultado ==
                ResultadoRevisionAporte.EvidenciaNoEncontrada)
            {
                return Result<VerificarAporteResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "No se encontró la información completa del pago Yape."
                    )
                );
            }

            // ==========================================
            // 5. RESPUESTA EXITOSA
            // ==========================================

            var respuesta = new VerificarAporteResponse(
                AporteId: revision.AporteId,
                Estado: revision.Estado.ToString(),
                Monto: revision.Monto,
                FechaRevision: revision.FechaRevision ?? DateTimeOffset.UtcNow,
                Mensaje: aprobar
                    ? "El aporte fue confirmado correctamente."
                    : "El aporte fue rechazado correctamente."
            );

            return Result<VerificarAporteResponse>.Success(
                respuesta
            );
        }



        public async Task<Result<RuletaDatosResponse>> ObtenerDatosRuletaAsync(
            Guid sorteoId,
            CancellationToken cancellationToken = default)
        {
            // ==========================================
            // 1. CONSULTAR PUBLICACIÓN
            // ==========================================

            var sorteo = await _uow._sorteoRepository
                .Consultar()
                .Where(s => s.Id == sorteoId)
                .Select(s => new
                {
                    s.Id,
                    s.Titulo,
                    s.Estado,
                    s.FechaInicio,
                    s.FechaSorteo
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (sorteo is null)
            {
                return Result<RuletaDatosResponse>.Failure(
                    new Error(
                        "NOT_FOUND",
                        "No se encontró la publicación."
                    )
                );
            }

            // ==========================================
            // 2. VALIDAR ESTADO PUBLICADO
            // ==========================================

            if (sorteo.Estado != EstadoSorteo.Publicado)
            {
                return Result<RuletaDatosResponse>.Failure(
                    new Error(
                        "BUSINESS_RULE_VIOLATION",
                        "La publicación debe estar en estado Publicado."
                    )
                );
            }

            // ==========================================
            // 3. CONSULTAR APORTES CONFIRMADOS
            // ==========================================
            // Agrupa por cliente y suma TODOS sus aportes
            // confirmados de la misma publicación.

            var datosClientes = await _uow._aporteRepository
    .Consultar()
    .Where(a =>
        a.Participacion.SorteoId == sorteoId &&
        a.Estado == EstadoAporte.Confirmado &&
        a.Monto > 0
    )
    .GroupBy(a => new
    {
        a.Participacion.ClienteId,
        a.Participacion.Cliente.Nombres,
        a.Participacion.Cliente.Apellidos
    })
    .Select(g => new
    {
        ClienteId = g.Key.ClienteId,
        Nombres = g.Key.Nombres,
        Apellidos = g.Key.Apellidos,
        TotalAportado = g.Sum(a => a.Monto)
    })
    .ToListAsync(cancellationToken);

            // ==========================================
            // 4. CONVERTIR A DTO EN MEMORIA
            // ==========================================
            // Esta parte ya se ejecuta en C#, no en SQL.

            var clientes = datosClientes
                .Select(c => new ClienteRuletaResponse(
                    ClienteId: c.ClienteId,
                    NombreCliente:
                        $"{c.Nombres} {c.Apellidos}".Trim(),
                    TotalAportado: c.TotalAportado
                ))
                .OrderBy(c => c.NombreCliente)
                .ThenBy(c => c.ClienteId)
                .ToList();

            // ==========================================
            // 4. CALCULAR TOTAL CONFIRMADO
            // ==========================================

            decimal totalConfirmado = clientes.Sum(
                c => c.TotalAportado
            );

            // ==========================================
            // 5. VALIDAR FECHA
            // ==========================================

            var ahora = DateTimeOffset.UtcNow;

            bool puedeSortear =
                ahora >= sorteo.FechaInicio &&
                sorteo.FechaSorteo.HasValue &&
                ahora >= sorteo.FechaSorteo.Value &&
                clientes.Count > 0;

            // ==========================================
            // 6. RESPUESTA
            // ==========================================

            var respuesta = new RuletaDatosResponse(
                SorteoId: sorteo.Id,
                TituloSorteo: sorteo.Titulo,
                Estado: sorteo.Estado.ToString(),
                FechaInicio: sorteo.FechaInicio,
                FechaSorteo: sorteo.FechaSorteo,
                FechaServidor: ahora,
                PuedeSortear: puedeSortear,
                TotalConfirmado: totalConfirmado,
                TotalClientes: clientes.Count,
                Clientes: clientes
            );

            return Result<RuletaDatosResponse>.Success(
                respuesta
            );
        }



    }
}
