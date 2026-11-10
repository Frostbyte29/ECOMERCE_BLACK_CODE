using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Configuracion;
using BlackCode.Domain.Compras;
using BlackCode.Domain.Pedidos;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BlackCode.Application.Abstractions;

/// <summary>
/// DIP: Application pide "una base de datos con estas tablas" sin saber que es MySQL.
/// La implementación concreta (AppDbContext + MySQL) vive en Infrastructure.
/// </summary>
public interface IAppDbContext
{
    DbSet<Usuario> Usuarios { get; }

    DbSet<Categoria> Categorias { get; }
    DbSet<Material> Materiales { get; }
    DbSet<Color> Colores { get; }
    DbSet<Producto> Productos { get; }
    DbSet<VarianteProducto> VariantesProducto { get; }
    DbSet<ImagenProducto> ImagenesProducto { get; }

    DbSet<Favorito> Favoritos { get; }
    DbSet<Carrito> Carritos { get; }
    DbSet<ItemCarrito> ItemsCarrito { get; }

    DbSet<Pedido> Pedidos { get; }
    DbSet<ItemPedido> ItemsPedido { get; }
    DbSet<HistorialEstadoPedido> HistorialPedidos { get; }
    DbSet<Pago> Pagos { get; }

    DbSet<ConfiguracionTienda> ConfiguracionesTienda { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Para operaciones que deben ser todo o nada (ej. el checkout)
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
