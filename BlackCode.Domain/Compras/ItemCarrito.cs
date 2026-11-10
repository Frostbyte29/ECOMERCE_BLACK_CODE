using BlackCode.Domain.Catalogo;

namespace BlackCode.Domain.Compras;

// Ojo: el carrito NO guarda precios. El precio siempre se lee del producto al momento de pagar.
public class ItemCarrito
{
    public int Id { get; set; }
    public int CarritoId { get; set; }
    public Carrito Carrito { get; set; } = null!;

    public int VarianteId { get; set; }
    public VarianteProducto Variante { get; set; } = null!;

    public int Cantidad { get; set; }
}
