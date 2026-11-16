using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Common.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Charatec.Sorteo.Data.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public JwtGenerado Generar(Guid usuarioId, string email, IEnumerable<string> roles)
        {
            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "No se configuró Jwt:Issuer.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "No se configuró Jwt:Audience.");

            var encodedKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "No se configuró Jwt:Key.");

            var minutos = _configuration.GetValue<int>(
                "Jwt:ExpirationMinutes");

            if (minutos is < 1 or > 1440)
            {
                throw new InvalidOperationException(
                    "La expiración JWT debe estar entre 1 y 1440 minutos.");
            }



            var keyBytes = Convert.FromBase64String(encodedKey);

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "La clave JWT debe tener al menos 32 bytes.");
            }

            var securityKey = new SymmetricSecurityKey(
                keyBytes);

            var ahora = DateTime.UtcNow;
            var expiracion = ahora.AddMinutes(minutos);

            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    usuarioId.ToString()),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuarioId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    email)
            };

            foreach (var rol in roles.Distinct())
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, rol));
            }

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: ahora,
                expires: expiracion,
                signingCredentials: credentials);

            var accessToken =
               new JwtSecurityTokenHandler()
                   .WriteToken(jwt);

            return new JwtGenerado(
                AccessToken: accessToken,
                ExpiresIn: minutos * 60
            );
        }
    }
}
