using BlackCode.Domain.Pedidos;
namespace BlackCode.Application.Features.Pedidos;
public record EnvioDto(string Nombre, string Telefono, string Direccion, string Distrito, string? Referencia);
public record LineaPedidoDto(string Producto, string Talla, decimal Precio, int Cantidad, decimal Total);
public record HistorialDto(string Estado, DateTime Fecha);
public record PedidoDto(int Id,string Codigo,string Cliente,string Correo,EstadoPedido Estado,decimal Subtotal,decimal Envio,decimal Total,
    DateTime Fecha,string Direccion,string Distrito,IReadOnlyList<LineaPedidoDto> Lineas,IReadOnlyList<HistorialDto> Historial);
public record ResultadoPagoDto(bool Aprobado, string Referencia);
public interface IPedidoService {
    Task<int> ConfirmarAsync(EnvioDto envio, bool simularRechazo, string metodo = "tarjeta");
    Task<IReadOnlyList<PedidoDto>> ListarAsync(bool administracion = false);
    Task<PedidoDto?> ObtenerAsync(int id);
    Task CambiarEstadoAsync(int id, EstadoPedido estado);
}
