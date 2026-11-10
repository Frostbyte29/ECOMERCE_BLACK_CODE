using BlackCode.Application.Abstractions;
using BlackCode.Domain.Common;
using Microsoft.AspNetCore.Hosting;

namespace BlackCode.Infrastructure.Storage;

// SRP: persiste imágenes en disco con nombres generados, sin usar rutas del cliente.
public sealed class AlmacenImagenesProducto(IWebHostEnvironment entorno) : IAlmacenImagenesProducto
{
    private const string Prefijo = "/img/productos/cargas/";

    public async Task<string> GuardarAsync(byte[] contenido, string extension)
    {
        if (extension is not (".jpg" or ".png" or ".webp")) throw new DomainException("Formato no admitido.");
        var carpeta = Path.Combine(entorno.WebRootPath, "img", "productos", "cargas");
        Directory.CreateDirectory(carpeta);
        var nombre = Guid.NewGuid().ToString("N") + extension;
        var ruta = Path.Combine(carpeta, nombre);
        try
        {
            await using var archivo = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            await archivo.WriteAsync(contenido);
        }
        catch
        {
            File.Delete(ruta);
            throw;
        }
        return Prefijo + nombre;
    }

    public Task EliminarAsync(string ruta)
    {
        if (!ruta.StartsWith(Prefijo, StringComparison.Ordinal)) throw new DomainException("Ruta de imagen inválida.");
        var nombre = ruta[Prefijo.Length..];
        if (!System.Text.RegularExpressions.Regex.IsMatch(nombre, @"\A[a-f0-9]{32}\.(jpg|png|webp)\z"))
            throw new DomainException("Ruta de imagen inválida.");
        File.Delete(Path.Combine(entorno.WebRootPath, "img", "productos", "cargas", nombre));
        return Task.CompletedTask;
    }
}
