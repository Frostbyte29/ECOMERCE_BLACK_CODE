namespace BlackCode.Domain.Pedidos;

// SRP: almacena el código de rastreo de la empresa de transporte asignado al pedido.
public class SeguimientoPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public string? Transportista { get; set; }
    public string? CodigoRastreo { get; set; }
    public DateTime CreadoUtc { get; set; }
}
