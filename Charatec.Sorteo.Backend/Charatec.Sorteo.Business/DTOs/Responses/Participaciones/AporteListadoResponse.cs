namespace Charatec.Sorteo.Business.DTOs.Responses.Participaciones
{
    public sealed record AporteListadoResponse(
    Guid Id,
    Guid SorteoId,
    string TituloSorteo,
    decimal Monto,
    string Estado,
    DateTimeOffset FechaCreacion
);
}
