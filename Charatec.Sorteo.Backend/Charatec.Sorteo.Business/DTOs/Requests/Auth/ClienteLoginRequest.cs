using System.ComponentModel.DataAnnotations;

namespace Charatec.Sorteo.Business.DTOs.Requests.Auth;

public sealed record ClienteLoginRequest(
    [param: Required(ErrorMessage = "El correo es obligatorio.")]
    [param: EmailAddress(ErrorMessage = "El correo no es válido.")]
    string Email,

    [param: Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password
);
