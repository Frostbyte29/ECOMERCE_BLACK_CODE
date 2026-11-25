namespace BlackCode.Application.Features.Pedidos;
public interface INotificacionPedidoService
{
    string Confirmacion(string codigo, bool pagoInmediato);
}
// Notificación en pantalla: no envía correos ni llama a servicios externos.
public sealed class NotificacionPedidoService : INotificacionPedidoService
{
    public string Confirmacion(string codigo, bool pagoInmediato) => pagoInmediato
        ? $"¡Compra confirmada! Tu pedido {codigo} ya está registrado y en proceso."
        : $"Pedido {codigo} recibido. En cuanto confirmemos tu pago, comenzamos a prepararlo.";
}
