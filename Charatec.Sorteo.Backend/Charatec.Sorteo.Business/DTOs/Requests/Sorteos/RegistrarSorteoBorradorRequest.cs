using System.ComponentModel.DataAnnotations;

namespace Charatec.Sorteo.Business.DTOs.Requests.Sorteos
{
    public sealed record CrearSorteoBorradorRequest : IValidatableObject
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

        [Url(ErrorMessage = "La imagen debe contener una URL válida.")]
        [StringLength(
            1000,
            ErrorMessage = "La URL de la imagen no puede superar los 1000 caracteres."
        )]
        public string? ImagenPortadaUrl { get; init; }


        [Range(
            typeof(decimal),
            "0.01",
            "99999999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El aporte mínimo debe estar entre 0.01 y 99999999.99."
        )]
        public decimal AporteMinimo { get; init; }

        [Range(
            typeof(decimal),
            "0.01",
            "999999999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "La meta de recaudación debe ser mayor que cero."
        )]
        public decimal? MetaRecaudacion { get; init; }


        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTimeOffset? FechaInicio { get; init; }

        [Required(ErrorMessage = "La fecha de cierre es obligatoria.")]
        public DateTimeOffset? FechaCierre { get; init; }

        public DateTimeOffset? FechaSorteo { get; init; }

        // ==========================================
        // VALIDACIONES PERSONALIZADAS
        // ==========================================

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Titulo))
            {
                yield return new ValidationResult(
                    "El título no puede estar vacío.",
                    [nameof(Titulo)]
                );
            }

            if (string.IsNullOrWhiteSpace(Descripcion))
            {
                yield return new ValidationResult(
                    "La descripción no puede estar vacía.",
                    [nameof(Descripcion)]
                );
            }

            // La meta no puede ser inferior al aporte mínimo.
            if (MetaRecaudacion.HasValue &&
                MetaRecaudacion.Value < AporteMinimo)
            {
                yield return new ValidationResult(
                    "La meta de recaudación no puede ser menor al aporte mínimo.",
                    [nameof(MetaRecaudacion)]
                );
            }

            // Inicio debe ser anterior al cierre.
            if (FechaInicio.HasValue &&
                FechaCierre.HasValue &&
                FechaCierre.Value <= FechaInicio.Value)
            {
                yield return new ValidationResult(
                    "La fecha de cierre debe ser posterior a la fecha de inicio.",
                    [nameof(FechaCierre)]
                );
            }

            // Sorteo debe realizarse después del cierre.
            if (FechaSorteo.HasValue &&
                FechaCierre.HasValue &&
                FechaSorteo.Value <= FechaCierre.Value)
            {
                yield return new ValidationResult(
                    "La fecha del sorteo debe ser posterior a la fecha de cierre.",
                    [nameof(FechaSorteo)]
                );
            }
        }
    }
}
