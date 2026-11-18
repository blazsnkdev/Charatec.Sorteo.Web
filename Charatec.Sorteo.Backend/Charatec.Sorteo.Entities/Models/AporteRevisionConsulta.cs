using Charatec.Sorteo.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Entities.Models
{
    public sealed class AporteRevisionConsulta
    {
        public Guid AporteId { get; set; }
        public Guid SorteoId { get; set; }

        public Guid ClienteId { get; set; }

        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        public string? NumeroWhatsapp { get; set; }

        public decimal Monto { get; set; }

        public string? CodigoOperacionYape { get; set; }

        public EstadoAporte Estado { get; set; }

        public DateTimeOffset FechaCreacion { get; set; }
    }
}
