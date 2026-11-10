namespace BlackCode.Application.Features.Pedidos;
public interface INotificacionPedidoService
{
    string Confirmacion(string codigo);
}
// Notificación en pantalla: no envía correos ni llama a servicios externos.
public sealed class NotificacionPedidoService : INotificacionPedidoService
{
    public string Confirmacion(string codigo) => $"¡Compra confirmada! Tu pedido {codigo} ya está registrado.";
}
