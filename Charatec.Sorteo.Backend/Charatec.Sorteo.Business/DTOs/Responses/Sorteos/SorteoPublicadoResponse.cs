
namespace Charatec.Sorteo.Business.DTOs.Responses.Sorteos;

public sealed record SorteoPublicadoResponse(
    Guid Id,
    string Titulo,
    string Estado,
    DateTimeOffset FechaActualizacion
);
