using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Responses.Cliente
{
    public sealed class SorteoDisponibleResponse
    {
        public Guid Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string? ImagenPortadaUrl { get; set; }

        public decimal AporteMinimo { get; set; }

        public decimal? MetaRecaudacion { get; set; }

        public DateTimeOffset FechaInicio { get; set; }

        public DateTimeOffset FechaCierre { get; set; }

        public DateTimeOffset? FechaSorteo { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}
