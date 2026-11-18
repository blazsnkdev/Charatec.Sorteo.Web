namespace Charatec.Sorteo.Common.Models
{
    public sealed record RegistrarPagoYapeData(
    Guid UsuarioId,
    Guid SorteoId,
    Guid SolicitudId,
    decimal Monto,
    string NumeroWhatsapp,
    string Email,
    string CodigoOperacionYape
);

    public sealed record RegistroPagoYapeData(
        Guid ParticipacionId,
        Guid OrdenPagoId,
        Guid AporteId,
        Guid SorteoId,
        decimal Monto
    );
}
