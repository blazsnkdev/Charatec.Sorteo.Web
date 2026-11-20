using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Requests.Participaciones
{
    public sealed class RegistrarParticipacionYapeRequest
    {
        [Range(
            typeof(decimal),
            "0.01",
            "99999999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El monto debe ser mayor a cero."
        )]
        public decimal Monto { get; set; }


        [Required]
        [RegularExpression(
            @"^9\d{8}$",
            ErrorMessage = "Ingrese un número de celular peruano válido."
        )]
        public string NumeroWhatsapp { get; set; }
            = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; }
            = string.Empty;


        [Required(ErrorMessage = "El código de operación Yape es obligatorio.")]
        [StringLength(8, MinimumLength = 8,
            ErrorMessage = "El código de operación Yape debe tener exactamente 8 dígitos.")]
        [RegularExpression(@"^[0-9]{8}$",
            ErrorMessage = "El código de operación Yape debe contener únicamente 8 números.")]
        public string CodigoOperacionYape { get; set; } = string.Empty;


        public bool AceptaTerminos { get; set; }

        [Required]
        public Guid SolicitudId { get; set; }
    }
}
