using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Requests.Aportes
{
    public sealed class VerificarAporteRequest
    {
        [Required]
        [RegularExpression(
            "^(Aprobar|Rechazar)$",
            ErrorMessage = "La decisión debe ser Aprobar o Rechazar."
        )]
        public string Decision { get; init; } = string.Empty;

        [MaxLength(1000)]
        public string? Observacion { get; init; }
    }
}
