using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Charatec.Sorteo.Business.DTOs.Requests.Pagos
{
    public sealed class RegistrarComprobanteYapeRequest
    {
        [Required]
        [MaxLength(100)]
        public string CodigoOperacion { get; set; }
            = string.Empty;

        [Required]
        public DateTimeOffset? FechaPago { get; set; }
    }
}
