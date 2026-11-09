using BlackCode.Application.Abstractions;
using BlackCode.Domain.Common;

namespace BlackCode.Application.Features.Administracion;

public record ArchivoImagenDto(Stream Contenido, string Nombre);

public interface IImagenProductoService
{
    Task<string> GuardarAsync(ArchivoImagenDto archivo);
    Task EliminarAsync(string ruta);
}

// SRP: valida los archivos de imagen antes de delegar su almacenamiento.
public sealed class ImagenProductoService(IAlmacenImagenesProducto almacen, IUsuarioActual usuario,
    IEnumerable<IFormatoImagenProducto> formatos) : IImagenProductoService
{
    public const int MaximoBytes = 5 * 1024 * 1024;

    public async Task<string> GuardarAsync(ArchivoImagenDto archivo)
    {
        if (!usuario.EsAdmin) throw new DomainException("Acceso restringido.");
        var extension = Path.GetExtension(archivo.Nombre).ToLowerInvariant();
        var formato = formatos.SingleOrDefault(f => f.Extensiones.Contains(extension));
        if (formato is null)
            throw new DomainException("Selecciona una imagen JPG, PNG o WebP.");

        using var memoria = new MemoryStream();
        var bloque = new byte[81920];
        int leidos;
        while ((leidos = await archivo.Contenido.ReadAsync(bloque)) > 0)
        {
            if (memoria.Length + leidos > MaximoBytes)
                throw new DomainException("La imagen no debe superar los 5 MB.");
            await memoria.WriteAsync(bloque.AsMemory(0, leidos));
        }
        var bytes = memoria.ToArray();
        if (!formato.EsValida(bytes))
            throw new DomainException("El archivo no tiene un formato de imagen válido o está incompleto.");
        return await almacen.GuardarAsync(bytes, formato.ExtensionAlmacenamiento);
    }

    public Task EliminarAsync(string ruta)
    {
        if (!usuario.EsAdmin) throw new DomainException("Acceso restringido.");
        return almacen.EliminarAsync(ruta);
    }

}
