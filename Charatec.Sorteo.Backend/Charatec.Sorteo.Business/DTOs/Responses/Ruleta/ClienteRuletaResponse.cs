namespace Charatec.Sorteo.Business.DTOs.Responses.Ruleta
{
    public sealed record ClienteRuletaResponse(
        Guid ClienteId,
        string NombreCliente,
        decimal TotalAportado,
        DateTimeOffset? FechaUltimaAprobacion
    );

}
