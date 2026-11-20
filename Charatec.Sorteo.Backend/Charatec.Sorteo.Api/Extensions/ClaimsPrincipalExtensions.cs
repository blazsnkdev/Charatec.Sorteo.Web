using System.Security.Claims;

namespace Charatec.Sorteo.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        //TODO:  este meotod lo que hace es traer el id usuario de la sesion que este en nameIdentifier o sino un guid vacio
        public static bool TryObtenerUsuarioId(this ClaimsPrincipal usuario, out Guid usuarioId)
        {
            var usuarioIdText = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(usuarioIdText, out usuarioId) && usuarioId != Guid.Empty;
        }
    }
}
