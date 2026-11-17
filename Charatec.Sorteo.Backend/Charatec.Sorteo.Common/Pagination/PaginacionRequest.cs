using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Charatec.Sorteo.Common.Pagination
{
    public sealed record PaginacionRequest
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La página debe ser mayor que cero."
        )]
        public int Pagina { get; init; } = 1;

        [Range(
            1,
            100,
            ErrorMessage = "La cantidad por página debe estar entre 1 y 100."
        )]
        public int TamanoPagina { get; init; } = 10;
    }
}
