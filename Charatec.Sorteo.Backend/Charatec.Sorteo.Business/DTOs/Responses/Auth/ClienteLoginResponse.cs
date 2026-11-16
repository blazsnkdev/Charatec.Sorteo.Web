
namespace Charatec.Sorteo.Business.DTOs.Responses.Auth;

public sealed record ClienteLoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    UsuarioAutenticadoResponse Usuario
);
