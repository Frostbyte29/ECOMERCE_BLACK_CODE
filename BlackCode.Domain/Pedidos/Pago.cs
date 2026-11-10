namespace BlackCode.Domain.Pedidos;

public enum MetodoPago
{
    Simulado = 1, // Conserva el valor de los pagos históricos.
    Tarjeta = 2,
    Transferencia = 3,
    Yape = 4,
    Plin = 5
}

public enum EstadoPago
{
    Aprobado = 1,
    Rechazado = 2
}

// Existe aunque el pago sea simulado: es el punto de extensión para una pasarela real.
public class Pago
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public MetodoPago Metodo { get; set; }
    public EstadoPago Estado { get; set; }
    public decimal Monto { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public DateTime CreadoUtc { get; set; }
}
