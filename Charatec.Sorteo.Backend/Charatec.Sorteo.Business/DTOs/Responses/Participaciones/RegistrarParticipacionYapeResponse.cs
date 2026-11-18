namespace Charatec.Sorteo.Business.DTOs.Responses.Participaciones
{
    public sealed class RegistrarParticipacionYapeResponse
    {
        public Guid ParticipacionId { get; set; }

        public Guid SorteoId { get; set; }

        public Guid OrdenPagoId { get; set; }

        public Guid AporteId { get; set; }

        public decimal Monto { get; set; }

        public string Moneda { get; set; } = "PEN";

        public string EstadoAporte { get; set; } = "Pendiente";

        public string EstadoVerificacion { get; set; }
            = "Pendiente";

        public string Mensaje { get; set; }
            = "Tu operación Yape está pendiente de verificación.";
    }
}
