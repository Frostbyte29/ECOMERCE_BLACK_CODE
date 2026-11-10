namespace BlackCode.Application.Features.Administracion;

public interface IEdicionProductoService
{
    Task GuardarAsync(ProductoEdicionDto producto, ArchivoImagenDto? archivo);
}

// SRP: coordina el guardado del producto y compensa una carga si los datos son rechazados.
public sealed class EdicionProductoService(IAdministracionService productos, IImagenProductoService imagenes) : IEdicionProductoService
{
    public async Task GuardarAsync(ProductoEdicionDto producto, ArchivoImagenDto? archivo)
    {
        string? nuevaImagen = null;
        try
        {
            if (archivo is not null)
            {
                nuevaImagen = await imagenes.GuardarAsync(archivo);
                producto = producto with { Imagen = nuevaImagen };
            }
            await productos.GuardarAsync(producto);
        }
        catch
        {
            if (nuevaImagen is not null) await imagenes.EliminarAsync(nuevaImagen);
            throw;
        }
    }
}
