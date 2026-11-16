using Charatec.Sorteo.Entities.Enums;

namespace Charatec.Sorteo.Entities.Models
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty!;
        public string Email { get; set; } = string.Empty!;
        public string NumeroCelular { get; set; }
        public string Direccion { get; set; }
        public string PasswordHashed { get; set; } = string.Empty!;
        public Rol Rol { get; set; }
        public bool Activo { get; set; }
        public DateTimeOffset FechaRegistro { get; set; }

        public ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
        public ICollection<Participacion> Participaciones { get; set; } = new List<Participacion>();
    }
}
