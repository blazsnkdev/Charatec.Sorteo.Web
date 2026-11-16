using System.ComponentModel.DataAnnotations;

namespace Charatec.Sorteo.Business.DTOs.Requests.Auth
{
    public sealed record ClienteRegistrarRequest
    (
        [param: Required(
        ErrorMessage = "Los nombres son obligatorios.")]
    [param: StringLength(100, MinimumLength = 2,
        ErrorMessage = "Los nombres deben tener entre 2 y 100 caracteres.")]
    string Nombres,

        [param: Required(
        ErrorMessage = "Los apellidos son obligatorios.")]
    [param: StringLength(100, MinimumLength = 2,
        ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
    string Apellidos,

        [param: Required(
        ErrorMessage = "El correo electrónico es obligatorio.")]
    [param: EmailAddress(
        ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [param: StringLength(254)]
    string Email,

        [param: Required(
        ErrorMessage = "El teléfono es obligatorio.")]
    [param: RegularExpression(
        @"^9\d{8}$",
        ErrorMessage = "El celular debe tener 9 dígitos y comenzar con 9.")]
    string Telefono,

    [param: Required(
        ErrorMessage = "La contraseña es obligatoria.")]
    [param: MinLength(
        6,
        ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    string Password
    );
}
