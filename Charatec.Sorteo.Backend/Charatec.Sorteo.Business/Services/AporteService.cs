using Charatec.Sorteo.Business.DTOs.Requests.Participaciones;
using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Extensions;
using Charatec.Sorteo.Data.Interfaces;

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
    }
}
