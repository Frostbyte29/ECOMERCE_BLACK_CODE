namespace BlackCode.Domain.Catalogo;

public class ImagenProducto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public int Orden { get; set; }
}
