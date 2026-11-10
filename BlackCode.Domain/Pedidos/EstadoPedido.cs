namespace BlackCode.Domain.Pedidos;

public enum EstadoPedido
{
    PendientePago = 1,
    Pagado = 2,
    EnPreparacion = 3,
    Enviado = 4,
    Entregado = 5,
    Cancelado = 6
}
