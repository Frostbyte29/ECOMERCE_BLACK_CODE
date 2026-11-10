namespace BlackCode.Domain.Pedidos;

public class HistorialEstadoPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public EstadoPedido EstadoAnterior { get; set; }
    public EstadoPedido EstadoNuevo { get; set; }
    public int CambiadoPorUsuarioId { get; set; }
    public DateTime CambiadoUtc { get; set; }
    public string? Nota { get; set; }
}
