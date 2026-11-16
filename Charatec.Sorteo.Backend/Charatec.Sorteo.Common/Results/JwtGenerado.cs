namespace Charatec.Sorteo.Common.Results
{
    public sealed record JwtGenerado(//TODO: el token se va generar a partir de este objeto
        string AccessToken,
        int ExpiresIn
    );
}
