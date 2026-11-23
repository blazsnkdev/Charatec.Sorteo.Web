using Microsoft.AspNetCore.Hosting;
using Charatec.Sorteo.Common.Interfaces;

public sealed class LocalImagenStorageService : IImagenStorageService
{
    private readonly string _directorioPortadas;

    public LocalImagenStorageService(
        IWebHostEnvironment environment)
    {
        _directorioPortadas = Path.Combine(
            environment.ContentRootPath,
            "wwwroot",
            "uploads",
            "sorteos"
        );
    }

    public async Task<string> GuardarPortadaAsync(
        Guid sorteoId,
        Stream contenido,
        string extension,
        CancellationToken cancellationToken = default)
    {
        // Validar extensión.
        extension = extension.ToLowerInvariant();

        if (extension is not (".jpg" or ".png" or ".webp"))
        {
            throw new ArgumentException(
                "El formato de imagen no está permitido."
            );
        }

        if (sorteoId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del sorteo no es válido."
            );
        }

        // Crear carpeta automáticamente.
        Directory.CreateDirectory(_directorioPortadas);

        // Generar un nombre único.
        var nombreArchivo =
            $"{sorteoId:N}_{Guid.NewGuid():N}{extension}";

        var rutaCompleta = Path.Combine(
            _directorioPortadas,
            nombreArchivo
        );

        try
        {
            // Guardar imagen físicamente.
            await using var archivo = new FileStream(
                rutaCompleta,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true
            );

            await contenido.CopyToAsync(
                archivo,
                cancellationToken
            );
        }
        catch
        {
            // Si falla la escritura, eliminar archivo incompleto.
            if (File.Exists(rutaCompleta))
            {
                File.Delete(rutaCompleta);
            }

            throw;
        }

        // Se guarda esta ruta en PostgreSQL.
        return $"/uploads/sorteos/{nombreArchivo}";
    }

    public Task EliminarPortadaAsync(
        string rutaRelativa,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            return Task.CompletedTask;
        }

        const string prefijo = "/uploads/sorteos/";

        if (!rutaRelativa.StartsWith(
            prefijo,
            StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "La ruta de la portada no es válida."
            );
        }

        var nombreArchivo = rutaRelativa[prefijo.Length..];

        // Evitar rutas como ../../archivo.
        if (string.IsNullOrWhiteSpace(nombreArchivo) ||
            nombreArchivo != Path.GetFileName(nombreArchivo))
        {
            throw new ArgumentException(
                "El nombre de archivo no es válido."
            );
        }

        var rutaCompleta = Path.Combine(
            _directorioPortadas,
            nombreArchivo
        );

        if (File.Exists(rutaCompleta))
        {
            File.Delete(rutaCompleta);
        }

        return Task.CompletedTask;
    }
}
