namespace Charatec.Sorteo.Business.DTOs.Requests
{
    public sealed record RegistrarClienteRequest
    (
        string Nombres,
        string Apellidos,
        string Email,
        string Telefono,
        string Password
    );
}
