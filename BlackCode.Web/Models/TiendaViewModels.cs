using System.ComponentModel.DataAnnotations;
using BlackCode.Application.Features.Catalogo;
using BlackCode.Application.Features.Carrito;
using BlackCode.Application.Features.Pedidos;
using BlackCode.Application.Features.Administracion;
namespace BlackCode.Web.Models;

public record OpcionViewModel(int Id, string Nombre);
public record VarianteViewModel(int Id, string Talla, int Stock, bool Activo);
public record ProductoViewModel(int Id, string Nombre, string Slug, string Descripcion, decimal Precio, string Categoria, string Color, string Imagen, bool Destacado, IReadOnlyList<VarianteViewModel> Variantes);
public class CatalogoViewModel
{
    public string? Buscar { get; set; }
    public int? Categoria { get; set; }
    public int? Color { get; set; }
    public decimal? Minimo { get; set; }
    public decimal? Maximo { get; set; }
    public string? Talla { get; set; }
    public string Orden { get; set; } = "relevancia";
    public IReadOnlyList<ProductoViewModel> Productos { get; set; } = [];
    public IReadOnlyList<OpcionViewModel> Categorias { get; set; } = []; public IReadOnlyList<OpcionViewModel> Colores { get; set; } = [];
}
public record LineaCarritoViewModel(int VarianteId, int ProductoId, string Nombre, string Talla, string Imagen, decimal Precio, int Cantidad, int Stock, bool Disponible) { public decimal Total => Precio * Cantidad; }
public record TotalesViewModel(decimal Subtotal, decimal Descuento, decimal Envio, decimal Total);
public record CarritoViewModel(IReadOnlyList<LineaCarritoViewModel> Lineas, TotalesViewModel Totales);
public class CheckoutViewModel
{
    [Required(ErrorMessage = "Escribe el nombre de quien recibe."), StringLength(120)] public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "Escribe un teléfono."), StringLength(20), RegularExpression(@"^\d{9}$", ErrorMessage = "Ingresa un teléfono de 9 dígitos.")] public string Telefono { get; set; } = "";
    [Required(ErrorMessage = "Escribe la dirección."), StringLength(200)] public string Direccion { get; set; } = "";
    [Required(ErrorMessage = "Escribe el distrito."), StringLength(80)] public string Distrito { get; set; } = "";
    [StringLength(200)] public string? Referencia { get; set; }
    public bool SimularRechazo { get; set; }
    public bool EsDesarrollo { get; set; }
    [Required] public string MetodoPago { get; set; } = "tarjeta";
    public IReadOnlyList<MetodoPagoViewModel> Metodos { get; set; } = [];
    public CarritoViewModel Carrito { get; set; } = new([], new(0, 0, 0, 0));
}
public record LineaPedidoViewModel(string Producto, string Talla, decimal Precio, int Cantidad, decimal Total);
public record MetodoPagoViewModel(string Codigo, string Nombre);
public record HistorialViewModel(string Estado, DateTime Fecha);
public record PedidoViewModel(int Id, string Codigo, string Cliente, string Estado, decimal Subtotal, decimal Envio, decimal Total, DateTime Fecha, string Direccion, string Distrito, IReadOnlyList<LineaPedidoViewModel> Lineas, IReadOnlyList<HistorialViewModel> Historial, IReadOnlyList<string> Siguientes);
public class VarianteEdicionViewModel
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string Talla { get; set; } = "Única";
    [Range(0, 100000)] public int Stock { get; set; }
    public bool Activo { get; set; } = true;
}
public class ProductoEdicionViewModel
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Nombre { get; set; } = "";
    [Required, StringLength(140), RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Usa minúsculas, números y guiones.")] public string Slug { get; set; } = "";
    [Required, StringLength(2000)] public string Descripcion { get; set; } = "";
    [Range(typeof(decimal), "0.01", "99999999")] public decimal Precio { get; set; }
    public int CategoriaId { get; set; }
    public int ColorId { get; set; }
    public int MaterialId { get; set; }
    public bool Publicado { get; set; } = true; public bool Destacado { get; set; }
    [Required] public string Imagen { get; set; } = "/img/productos/anillo-profano.svg";
    public List<VarianteEdicionViewModel> Variantes { get; set; } = [new()];
    public IReadOnlyList<OpcionViewModel> Categorias { get; set; } = []; public IReadOnlyList<OpcionViewModel> Colores { get; set; } = []; public IReadOnlyList<OpcionViewModel> Materiales { get; set; } = [];
}
public record AdminViewModel(int Productos, int Pedidos, int Clientes, int CarritosInvitados, decimal Ventas, IReadOnlyList<ProductoEdicionViewModel> Lista);
public static class TiendaMapper
{
    public static ProductoViewModel Producto(ProductoDto p) => new(p.Id, p.Nombre, p.Slug, p.Descripcion, p.Precio, p.Categoria, p.Color, p.Imagen, p.Destacado, p.Variantes.Select(v => new VarianteViewModel(v.Id, v.Talla, v.Stock, v.Activo)).ToList());
    public static CarritoViewModel Carrito(CarritoDto c) => new(c.Lineas.Select(l => new LineaCarritoViewModel(l.VarianteId, l.ProductoId, l.Nombre, l.Talla, l.Imagen, l.Precio, l.Cantidad, l.Stock, l.Disponible)).ToList(), new(c.Totales.Subtotal, c.Totales.Descuento, c.Totales.Envio, c.Totales.Total));
    public static PedidoViewModel Pedido(PedidoDto p) => new(p.Id, p.Codigo, p.Cliente, p.Estado.ToString(), p.Subtotal, p.Envio, p.Total, p.Fecha, p.Direccion, p.Distrito, p.Lineas.Select(l => new LineaPedidoViewModel(l.Producto, l.Talla, l.Precio, l.Cantidad, l.Total)).ToList(), p.Historial.Select(h => new HistorialViewModel(h.Estado, h.Fecha)).ToList(), BlackCode.Domain.Pedidos.TransicionesPedido.SiguientesEstados(p.Estado).Select(e => e.ToString()).ToList());
    public static ProductoEdicionViewModel Edicion(ProductoEdicionDto p) => new() { Id = p.Id, Nombre = p.Nombre, Slug = p.Slug, Descripcion = p.Descripcion, Precio = p.Precio, CategoriaId = p.CategoriaId, ColorId = p.ColorId, MaterialId = p.MaterialId, Publicado = p.Publicado, Destacado = p.Destacado, Imagen = p.Imagen, Variantes = p.Variantes.Select(v => new VarianteEdicionViewModel { Id = v.Id, Talla = v.Talla, Stock = v.Stock, Activo = v.Activo }).ToList() };
    public static string Estado(string valor) => valor switch { "PendientePago" => "Pendiente de pago", "EnPreparacion" => "En preparación", _ => valor };
    public static string Dinero(decimal valor) => "S/ " + valor.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("es-PE"));
    private static readonly TimeSpan OffsetLima = TimeSpan.FromHours(-5); // Perú no tiene horario de verano
    public static string Fecha(DateTime utc) => utc.Add(OffsetLima).ToString("dd/MM/yyyy HH:mm");
}
