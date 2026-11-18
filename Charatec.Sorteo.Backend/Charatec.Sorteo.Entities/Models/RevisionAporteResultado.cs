using Charatec.Sorteo.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Entities.Models
{
    public sealed record RevisionAporteResultado(
    ResultadoRevisionAporte Resultado,
    Guid AporteId,
    decimal Monto,
    EstadoAporte Estado,
    DateTimeOffset? FechaRevision
);
}
