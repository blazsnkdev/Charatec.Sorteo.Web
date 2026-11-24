namespace Charatec.Sorteo.Business.DTOs.Requests.Sorteos
{
    public sealed record CancelarSorteoRequest
    (
        Guid SorteoId,
        Guid AdministradorId
    );
}
