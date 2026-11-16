using Charatec.Sorteo.Common.Results;

namespace Charatec.Sorteo.Common.Interfaces
{
    public interface IJwtTokenService
    {
        JwtGenerado Generar(
            Guid usuarioId,
            string email,
            IEnumerable<string> roles);
    }
}
