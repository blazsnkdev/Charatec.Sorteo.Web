using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Responses.Sorteos
{
    public sealed class ResultadoSorteosCliente
    {
        public List<SorteoDisponibleResponse> Registros
        { get; set; } = new();

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }

        public bool TienePaginaAnterior { get; set; }

        public bool TienePaginaSiguiente { get; set; }
    }
}
