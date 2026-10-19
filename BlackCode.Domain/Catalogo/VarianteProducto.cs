namespace BlackCode.Domain.Catalogo;

// Anillos: una variante por talla ("7", "8", "9"...).
// Cadenas y pulseras: una sola variante con Talla = "Única" de momento.
public class VarianteProducto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public string Talla { get; set; } = "Única";
    public string Sku { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool Activo { get; set; } = true;
}
