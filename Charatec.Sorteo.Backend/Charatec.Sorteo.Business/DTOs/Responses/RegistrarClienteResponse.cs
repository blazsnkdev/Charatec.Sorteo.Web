namespace Charatec.Sorteo.Business.DTOs.Responses
{
    public sealed record RegistrarClienteResponse
    (
        string Id,
        string Nombre,
        string Apellidos,
        string Email,
        string Mensaje
    );
}
