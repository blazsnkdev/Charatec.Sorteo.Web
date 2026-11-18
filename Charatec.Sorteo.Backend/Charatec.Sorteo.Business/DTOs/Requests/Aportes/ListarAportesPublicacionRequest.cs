using Charatec.Sorteo.Common.Pagination;
using Charatec.Sorteo.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Requests.Aportes
{
    public sealed class ListarAportesPublicacionRequest
    {
        [Range(1, int.MaxValue)]
        public int Pagina { get; init; } = 1;

        [Range(1, 100)]
        public int TamanoPagina { get; init; } = 10;

        public EstadoAporte? Estado { get; init; }

        public PaginacionRequest ObtenerPaginacion() => new()
        {
            Pagina = Pagina,
            TamanoPagina = TamanoPagina
        };
    }
}
