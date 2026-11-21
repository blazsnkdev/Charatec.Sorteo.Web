using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Responses.Sorteos
{
    public sealed record PortadaSorteoResponse(
        Guid SorteoId,
        string ImagenPortadaUrl
    );
}
