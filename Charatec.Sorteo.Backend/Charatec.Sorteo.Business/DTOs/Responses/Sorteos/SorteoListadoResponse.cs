using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Responses.Sorteos
{
    public sealed record SorteoListadoResponse(
        Guid Id,
        Guid AdministradorUsuarioId,
        string Titulo,
        string Descripcion,
        string? ImagenPortadaUrl,
        decimal AporteMinimo,
        decimal? MetaRecaudacion,
        DateTimeOffset FechaInicio,
        DateTimeOffset FechaCierre,
        DateTimeOffset? FechaSorteo,
        string Estado,
        DateTimeOffset FechaCreacion
    );
}
