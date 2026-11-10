namespace BlackCode.Domain.Pedidos;

// Snapshot: guarda nombre, talla y precio TAL COMO ESTABAN al comprar.
// Si mañana el admin cambia el precio, los pedidos viejos no cambian.
public class ItemPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public int VarianteId { get; set; }
    public string NombreProductoCopia { get; set; } = string.Empty;
    public string TallaCopia { get; set; } = string.Empty;

    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal TotalLinea { get; set; }
}
