namespace Charatec.Sorteo.Common.Interfaces
{
    public interface IImagenStorageService
    {
        Task<string> GuardarPortadaAsync(
            Guid sorteoId,
            Stream contenido,
            string extension,
            CancellationToken cancellationToken = default);

        Task EliminarPortadaAsync(
            string rutaRelativa,
            CancellationToken cancellationToken = default);
    }
}
