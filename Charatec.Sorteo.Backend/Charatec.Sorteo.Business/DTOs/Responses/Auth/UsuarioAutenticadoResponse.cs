namespace Charatec.Sorteo.Business.DTOs.Responses.Auth;

public sealed record UsuarioAutenticadoResponse(
    Guid Id,
    string Email,
    string? Nombres,
    string? Apellidos,
    string[] Roles
);
