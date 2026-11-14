
using Microsoft.AspNetCore.Identity;

namespace Charatec.Sorteo.Data.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTimeOffset FechaRegistro { get; set; }
        = DateTimeOffset.UtcNow;

    public bool Activo { get; set; } = true;
}
