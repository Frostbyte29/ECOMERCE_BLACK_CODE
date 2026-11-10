namespace BlackCode.Application.Abstractions;

// DIP: Application desconoce la ubicación física de las imágenes.
public interface IAlmacenImagenesProducto
{
    Task<string> GuardarAsync(byte[] contenido, string extension);
    Task EliminarAsync(string ruta);
}
