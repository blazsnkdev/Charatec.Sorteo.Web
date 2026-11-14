namespace Charatec.Sorteo.Entities.Models
{
    public class Cliente
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; }//TODO: aqui el login puede ser con el DNI para que garantice que es un usuario persona
        public DateOnly FechaNacimiento { get; set; }
        public DateTimeOffset FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
