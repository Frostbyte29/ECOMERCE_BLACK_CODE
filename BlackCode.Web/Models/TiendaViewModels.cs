using System.ComponentModel.DataAnnotations;
using BlackCode.Application.Features.Catalogo;
using BlackCode.Application.Features.Carrito;
using BlackCode.Application.Features.Pedidos;
using BlackCode.Application.Features.Administracion;
using BlackCode.Application.Features.Direcciones;
using BlackCode.Domain.Reclamos;
namespace BlackCode.Web.Models;

public record OpcionViewModel(int Id, string Nombre);

// Favoritos
public record FavoritoViewModel(int ProductoId, string Nombre, string Slug, string Imagen, decimal Precio, string Categoria, bool HayStock);

// Mi cuenta — Mis datos
public class MisDatosViewModel
{
    [Required(ErrorMessage = "Escribe tu nombre completo."), StringLength(120)]
    public string NombreCompleto { get; set; } = "";
    [StringLength(20), RegularExpression(@"^\d{9}$", ErrorMessage = "Ingresa un teléfono de 9 dígitos.")]
    public string? Telefono { get; set; }
}

// Direcciones guardadas
public class DireccionFormViewModel
{
    [Required(ErrorMessage = "Escribe el nombre de quien recibe."), StringLength(120)]
    public string NombreCompleto { get; set; } = "";
    [Required(ErrorMessage = "Escribe un teléfono."), StringLength(20), RegularExpression(@"^\d{9}$", ErrorMessage = "Ingresa un teléfono de 9 dígitos.")]
    public string Telefono { get; set; } = "";
    [Required(ErrorMessage = "Escribe la dirección."), StringLength(200)]
    public string Direccion { get; set; } = "";
    [Required(ErrorMessage = "Escribe el distrito."), StringLength(80)]
    public string Distrito { get; set; } = "";
    [StringLength(200)]
    public string? Referencia { get; set; }
    public bool EsPredeterminada { get; set; }
    public int Id { get; set; }   // 0 para crear
}
public record DireccionListaViewModel(int Id, string NombreCompleto, string Telefono, string Direccion, string Distrito, string? Referencia, bool EsPredeterminada);

// Reseñas
public class ResenaFormViewModel
{
    public int ProductoId { get; set; }
    [Range(1, 5)] public int Calificacion { get; set; } = 5;
    [StringLength(500)] public string? Comentario { get; set; }
}
public record ResenaListaViewModel(string Autor, int Calificacion, string? Comentario, DateTime Fecha);

// Seguimiento
public record EventoSeguimientoViewModel(string Estado, DateTime Fecha);
public record SeguimientoViewModel(string Codigo, string EstadoActual, IReadOnlyList<EventoSeguimientoViewModel> Historial, string? Transportista, string? CodigoRastreo);

// Libro de reclamaciones
public class ReclamoFormViewModel
{
    [Required(ErrorMessage = "Escribe tu nombre completo."), StringLength(120)]
    public string NombresCompletos { get; set; } = "";
    [Required(ErrorMessage = "Escribe tu DNI o RUC."), StringLength(20)]
    public string Documento { get; set; } = "";
    [Required(ErrorMessage = "Escribe tu domicilio."), StringLength(200)]
    public string Domicilio { get; set; } = "";
    [Required(ErrorMessage = "Escribe tu correo."), EmailAddress, StringLength(150)]
    public string Correo { get; set; } = "";
    [StringLength(20), RegularExpression(@"^\d{7,9}$", ErrorMessage = "Teléfono no válido.")]
    public string? Telefono { get; set; }
    [StringLength(20)]
    public string? NumeroPedido { get; set; }
    [Required] public TipoReclamo Tipo { get; set; } = TipoReclamo.Reclamo;
    [Required(ErrorMessage = "Describe el producto o servicio."), StringLength(300)]
    public string DescripcionBien { get; set; } = "";
    [Required(ErrorMessage = "Detalla lo ocurrido."), StringLength(1000)]
    public string Detalle { get; set; } = "";
    [Required(ErrorMessage = "Indica qué solicitas."), StringLength(1000)]
    public string PedidoCliente { get; set; } = "";
}
public record VarianteViewModel(int Id, string Talla, int Stock, bool Activo);
public record ProductoViewModel(int Id, string Nombre, string Slug, string Descripcion, decimal Precio, string Categoria, string Color, string Material, string? MaterialDescripcion, string Imagen, bool Destacado, IReadOnlyList<VarianteViewModel> Variantes, int CategoriaId = 0, IReadOnlyList<string>? Imagenes = null, bool EsFavorito = false);
public class CatalogoViewModel
{
    public string? Buscar { get; set; }
    public int? Categoria { get; set; }
    public int? Color { get; set; }
    public int? Material { get; set; }
    public decimal? Minimo { get; set; }
    public decimal? Maximo { get; set; }
    public string? Talla { get; set; }
    public string Orden { get; set; } = "relevancia";
    public IReadOnlyList<ProductoViewModel> Productos { get; set; } = [];
    public IReadOnlyList<OpcionViewModel> Categorias { get; set; } = []; public IReadOnlyList<OpcionViewModel> Colores { get; set; } = []; public IReadOnlyList<OpcionViewModel> Materiales { get; set; } = [];
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
    public bool MostrarRechazo { get; set; }
    [Required] public string MetodoPago { get; set; } = "tarjeta";
    [StringLength(20)] public string? NumeroOperacion { get; set; }
    [StringLength(20)] public string? TarjetaMarca { get; set; }
    [StringLength(4)] public string? TarjetaUltimos { get; set; }
    public IReadOnlyList<MetodoPagoViewModel> Metodos { get; set; } = [];
    public CarritoViewModel Carrito { get; set; } = new([], new(0, 0, 0, 0));
    // Selector de dirección guardada (opcional)
    public IReadOnlyList<DireccionListaViewModel> DireccionesGuardadas { get; set; } = [];
    public int? DireccionGuardadaId { get; set; }
}
public record LineaPedidoViewModel(string Producto, string Talla, decimal Precio, int Cantidad, decimal Total, string? ProductoSlug = null);
public record MetodoPagoViewModel(string Codigo, string Nombre, string? ImagenQr = null, bool QrDisponible = false);
public record HistorialViewModel(string Estado, DateTime Fecha);
public record PedidoViewModel(int Id, string Codigo, string Cliente, string Estado, decimal Subtotal, decimal Envio, decimal Total, DateTime Fecha, string Direccion, string Distrito, IReadOnlyList<LineaPedidoViewModel> Lineas, IReadOnlyList<HistorialViewModel> Historial, IReadOnlyList<string> Siguientes, string? PagoMetodo = null, string? PagoEstado = null, string? PagoReferencia = null);
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
    [Range(typeof(decimal), "0.01", "99999999", ParseLimitsInInvariantCulture = true)] public decimal Precio { get; set; }
    public int CategoriaId { get; set; }
    public int ColorId { get; set; }
    public int MaterialId { get; set; }
    public bool Publicado { get; set; } = true; public bool Destacado { get; set; }
    [Required] public string Imagen { get; set; } = "/img/productos/catalogo/anillo-carmesi.jpg";
    [Display(Name = "Imagen del producto")] public IFormFile? ArchivoImagen { get; set; }
    public List<VarianteEdicionViewModel> Variantes { get; set; } = [new()];
    public IReadOnlyList<OpcionViewModel> Categorias { get; set; } = []; public IReadOnlyList<OpcionViewModel> Colores { get; set; } = []; public IReadOnlyList<OpcionViewModel> Materiales { get; set; } = [];
}
public record AdminViewModel(int Productos, int Pedidos, int Clientes, int CarritosInvitados, decimal Ventas, IReadOnlyList<ProductoEdicionViewModel> Lista);
public static class TiendaMapper
{
    public static ProductoViewModel Producto(ProductoDto p, bool esFavorito = false) => new(p.Id, p.Nombre, p.Slug, p.Descripcion, p.Precio, p.Categoria, p.Color, p.Material, p.MaterialDescripcion, p.Imagen, p.Destacado, p.Variantes.Select(v => new VarianteViewModel(v.Id, v.Talla, v.Stock, v.Activo)).ToList(), p.CategoriaId, p.Imagenes, esFavorito);
    public static CarritoViewModel Carrito(CarritoDto c) => new(c.Lineas.Select(l => new LineaCarritoViewModel(l.VarianteId, l.ProductoId, l.Nombre, l.Talla, l.Imagen, l.Precio, l.Cantidad, l.Stock, l.Disponible)).ToList(), new(c.Totales.Subtotal, c.Totales.Descuento, c.Totales.Envio, c.Totales.Total));
    public static PedidoViewModel Pedido(PedidoDto p) => new(p.Id, p.Codigo, p.Cliente, p.Estado.ToString(), p.Subtotal, p.Envio, p.Total, p.Fecha, p.Direccion, p.Distrito, p.Lineas.Select(l => new LineaPedidoViewModel(l.Producto, l.Talla, l.Precio, l.Cantidad, l.Total, l.ProductoSlug)).ToList(), p.Historial.Select(h => new HistorialViewModel(h.Estado, h.Fecha)).ToList(), BlackCode.Domain.Pedidos.TransicionesPedido.SiguientesEstados(p.Estado).Select(e => e.ToString()).ToList(), p.PagoMetodo?.ToString(), p.PagoEstado?.ToString(), p.PagoReferencia);
    public static ProductoEdicionViewModel Edicion(ProductoEdicionDto p) => new() { Id = p.Id, Nombre = p.Nombre, Slug = p.Slug, Descripcion = p.Descripcion, Precio = p.Precio, CategoriaId = p.CategoriaId, ColorId = p.ColorId, MaterialId = p.MaterialId, Publicado = p.Publicado, Destacado = p.Destacado, Imagen = p.Imagen, Variantes = p.Variantes.Select(v => new VarianteEdicionViewModel { Id = v.Id, Talla = v.Talla, Stock = v.Stock, Activo = v.Activo }).ToList() };
    public static FavoritoViewModel Favorito(BlackCode.Application.Features.Favoritos.FavoritoDto f) => new(f.ProductoId, f.Nombre, f.Slug, f.Imagen, f.Precio, f.Categoria, f.HayStock);
    public static DireccionListaViewModel Direccion(DireccionDto d) => new(d.Id, d.NombreCompleto, d.Telefono, d.Direccion, d.Distrito, d.Referencia, d.EsPredeterminada);
    public static ResenaListaViewModel Resena(BlackCode.Application.Features.Resenas.ResenaDto r) => new(r.Autor, r.Calificacion, r.Comentario, r.Fecha);
    public static SeguimientoViewModel Seguimiento(BlackCode.Application.Features.Seguimiento.SeguimientoDto s) => new(s.Codigo, s.EstadoActual, s.Historial.Select(e => new EventoSeguimientoViewModel(e.Estado, e.Fecha)).ToList(), s.Transportista, s.CodigoRastreo);
    public static string Estado(string valor) => valor switch { "PendientePago" => "Pendiente de pago", "Pagado" => "Pago confirmado", "EnPreparacion" => "Preparando tu pedido", "Enviado" => "En camino", "Entregado" => "Entregado", "Cancelado" => "Cancelado", "Aprobado" => "Aprobado", "Pendiente" => "Pendiente de confirmación", "Rechazado" => "Rechazado", "Tarjeta" => "Tarjeta", "Yape" => "Yape", "Plin" => "Plin", "Transferencia" => "Transferencia", _ => valor };
    public static string Dinero(decimal valor) => "S/ " + valor.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("es-PE"));
    private static readonly TimeSpan OffsetLima = TimeSpan.FromHours(-5); // Perú no tiene horario de verano
    public static string Fecha(DateTime utc) => utc.Add(OffsetLima).ToString("dd/MM/yyyy HH:mm");
}
