
namespace Charatec.Sorteo.Business.DTOs.Responses.Ruleta;

public sealed record RuletaDatosResponse(
    Guid SorteoId,
    string TituloSorteo,
    string Estado,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaSorteo,
    DateTimeOffset FechaServidor,
    bool PuedeSortear,
    decimal TotalConfirmado,
    int TotalClientes,
    IReadOnlyList<ClienteRuletaResponse> Clientes
);
