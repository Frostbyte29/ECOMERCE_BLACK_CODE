namespace BlackCode.Application.Features.Carrito;
public record PropietarioCarrito(int? UsuarioId, string? TokenAnonimo);
public record LineaCarritoDto(int VarianteId, int ProductoId, string Nombre, string Talla, string Imagen, decimal Precio, int Cantidad, int Stock, bool Disponible)
{
    public decimal Total => Precio * Cantidad;
}
public record TotalesDto(decimal Subtotal, decimal Descuento, decimal Envio)
{
    public decimal Total => Subtotal - Descuento + Envio;
}
public record CarritoDto(IReadOnlyList<LineaCarritoDto> Lineas, TotalesDto Totales);
public interface ICarritoService
{
    Task<CarritoDto> ObtenerAsync(PropietarioCarrito propietario);
    Task AgregarAsync(PropietarioCarrito propietario, int varianteId, int cantidad);
    Task ActualizarAsync(PropietarioCarrito propietario, int varianteId, int cantidad);
    Task FusionarAsync(string tokenAnonimo, int usuarioId);
}
public interface ICalculoCarritoService
{
    Task<TotalesDto> CalcularAsync(IReadOnlyList<LineaCarritoDto> lineas);
}
public interface IReglaDescuento
{
    decimal Calcular(IReadOnlyList<LineaCarritoDto> lineas);
}
