using BlackCode.Application.Abstractions;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Configuracion;
using BlackCode.Domain.Compras;
using BlackCode.Domain.Pedidos;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BlackCode.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Material> Materiales => Set<Material>();
    public DbSet<Color> Colores => Set<Color>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<VarianteProducto> VariantesProducto => Set<VarianteProducto>();
    public DbSet<ImagenProducto> ImagenesProducto => Set<ImagenProducto>();

    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    public DbSet<ItemCarrito> ItemsCarrito => Set<ItemCarrito>();

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItemsPedido => Set<ItemPedido>();
    public DbSet<HistorialEstadoPedido> HistorialPedidos => Set<HistorialEstadoPedido>();
    public DbSet<Pago> Pagos => Set<Pago>();

    public DbSet<ConfiguracionTienda> ConfiguracionesTienda => Set<ConfiguracionTienda>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    // Cada tabla tiene su propia clase de configuración (SRP) en Persistence/Configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
