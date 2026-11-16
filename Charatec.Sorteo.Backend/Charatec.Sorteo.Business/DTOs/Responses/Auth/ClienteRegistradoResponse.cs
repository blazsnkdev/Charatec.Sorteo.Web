
namespace Charatec.Sorteo.Business.DTOs.Responses.Auth
{
    public sealed record ClienteRegistradoResponse
    (
        Guid Id,
        string Nombres,
        string Apellidos,
        string Email,
        string Telefono,
        DateTimeOffset FechaRegistro,
        bool Activo,
        string Rol
    );
}
