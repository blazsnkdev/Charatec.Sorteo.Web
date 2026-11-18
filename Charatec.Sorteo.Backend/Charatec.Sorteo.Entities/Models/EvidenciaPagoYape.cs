using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Entities.Models
{
    public sealed class EvidenciaPagoYape
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SolicitudId { get; set; }

        public Guid UsuarioId { get; set; }

        public Guid SorteoId { get; set; }

        public Guid OrdenPagoId { get; set; }

        public Guid AporteId { get; set; }

        public string NumeroWhatsapp { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string CodigoOperacionYape { get; set; }
            = string.Empty;

        public DateTimeOffset FechaRegistro { get; set; }
            = DateTimeOffset.UtcNow;

        public bool Verificado { get; set; } = false;

        public DateTimeOffset? FechaVerificacion { get; set; }

        public Guid? AdministradorRevisorId { get; set; }

        public string? Observacion { get; set; }
    }
}
