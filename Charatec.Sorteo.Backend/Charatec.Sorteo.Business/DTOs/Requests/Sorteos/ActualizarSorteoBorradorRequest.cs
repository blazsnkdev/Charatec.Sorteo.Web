
using System.ComponentModel.DataAnnotations;

namespace Charatec.Sorteo.Business.DTOs.Requests.Sorteos;

public sealed record ActualizarSorteoBorradorRequest
    : IValidatableObject
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(
        180,
        MinimumLength = 5,
        ErrorMessage = "El título debe tener entre 5 y 180 caracteres."
    )]
    public required string Titulo { get; init; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(
        3000,
        MinimumLength = 10,
        ErrorMessage = "La descripción debe tener entre 10 y 3000 caracteres."
    )]
    public required string Descripcion { get; init; }

    [Range(
        typeof(decimal),
        "0.01",
        "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "El aporte mínimo debe ser mayor que cero."
    )]
    public decimal AporteMinimo { get; init; }

    [Range(
        typeof(decimal),
        "0.01",
        "999999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "La meta debe ser mayor que cero."
    )]
    public decimal? MetaRecaudacion { get; init; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTimeOffset? FechaInicio { get; init; }

    [Required(ErrorMessage = "La fecha de cierre es obligatoria.")]
    public DateTimeOffset? FechaCierre { get; init; }

    public DateTimeOffset? FechaSorteo { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Titulo) ||
            Titulo.Trim().Length < 5)
        {
            yield return new ValidationResult(
                "El título debe tener al menos 5 caracteres.",
                [nameof(Titulo)]
            );
        }

        if (string.IsNullOrWhiteSpace(Descripcion) ||
            Descripcion.Trim().Length < 10)
        {
            yield return new ValidationResult(
                "La descripción debe tener al menos 10 caracteres.",
                [nameof(Descripcion)]
            );
        }

        if (MetaRecaudacion.HasValue &&
            MetaRecaudacion.Value < AporteMinimo)
        {
            yield return new ValidationResult(
                "La meta no puede ser menor al aporte mínimo.",
                [nameof(MetaRecaudacion)]
            );
        }

        if (FechaInicio.HasValue &&
            FechaCierre.HasValue &&
            FechaCierre.Value <= FechaInicio.Value)
        {
            yield return new ValidationResult(
                "La fecha de cierre debe ser posterior al inicio.",
                [nameof(FechaCierre)]
            );
        }

        if (FechaSorteo.HasValue &&
            FechaCierre.HasValue &&
            FechaSorteo.Value <= FechaCierre.Value)
        {
            yield return new ValidationResult(
                "La fecha del sorteo debe ser posterior al cierre.",
                [nameof(FechaSorteo)]
            );
        }
    }
}
