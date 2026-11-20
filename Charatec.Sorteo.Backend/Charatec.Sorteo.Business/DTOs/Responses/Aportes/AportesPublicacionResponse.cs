using Charatec.Sorteo.Common.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Responses.Aportes
{

    public sealed record AportesPublicacionResponse(
        Guid SorteoId,
        string TituloSorteo,
        decimal MontoJuntado,
        ResultadoPaginado<AportePublicacionResponse> Aportes
    );

}
