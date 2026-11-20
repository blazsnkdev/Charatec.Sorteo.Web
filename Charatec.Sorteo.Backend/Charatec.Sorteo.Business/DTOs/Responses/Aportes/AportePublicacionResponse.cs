namespace Charatec.Sorteo.Business.DTOs.Responses.Aportes
{
    public sealed record AportePublicacionResponse(
        Guid AporteId,
        Guid SorteoId,
        Guid ClienteId,
        string NombreCliente,
        string? NumeroWhatsapp,
        decimal Monto,
        string? CodigoOperacionYape,
        string Estado,
        DateTimeOffset FechaCreacion
    );
}
