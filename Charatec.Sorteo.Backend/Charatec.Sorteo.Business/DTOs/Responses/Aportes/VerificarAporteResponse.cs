using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Responses.Aportes
{
    public sealed record VerificarAporteResponse(
    Guid AporteId,
    string Estado,
    decimal Monto,
    DateTimeOffset FechaRevision,
    string Mensaje
);
}
