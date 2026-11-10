using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Carrito;
using BlackCode.Application.Features.Pedidos;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
using BlackCode.Domain.Compras;
using BlackCode.Domain.Pedidos;
using BlackCode.Domain.Usuarios;
using BlackCode.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Tests.Pedidos;

// SQLite permite comprobar las transacciones del checkout sin utilizar la base de la tienda.
public sealed class CheckoutPagoTests
{
    private static readonly EnvioDto Envio = new("Cliente", "987654321", "Av. Prueba 123", "Lima", null);

    [Theory]
    [InlineData("tarjeta", MetodoPago.Tarjeta)]
    [InlineData("transferencia", MetodoPago.Transferencia)]
    [InlineData("yape", MetodoPago.Yape)]
    [InlineData("plin", MetodoPago.Plin)]
    public async Task Confirmar_guarda_metodo_correcto_y_descontar_stock(string codigo, MetodoPago esperado)
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await PrepararAsync(conexion);
        var servicio = Servicio(db);
        var id = await servicio.ConfirmarAsync(Envio, false, codigo);
        db.ChangeTracker.Clear();
        var pago = await db.Pagos.SingleAsync(p => p.PedidoId == id);
        Assert.Equal(esperado, pago.Metodo);
        Assert.Equal(54m, pago.Monto);
        Assert.Equal(EstadoPago.Aprobado, pago.Estado);
        Assert.Equal(2, await db.VariantesProducto.Select(v => v.Stock).SingleAsync());
        Assert.Empty(await db.Carritos.ToListAsync());
        var pedido = await servicio.ObtenerAsync(id);
        Assert.NotNull(pedido);
        Assert.Equal(EstadoPedido.Pagado, pedido.Estado);
        Assert.Equal(54m, pedido.Total);
    }

    [Theory]
    [InlineData("tarjeta", true)]
    [InlineData("transferencia", true)]
    [InlineData("yape", true)]
    [InlineData("plin", true)]
    [InlineData("inexistente", false)]
    public async Task Rechazo_o_metodo_invalido_no_crea_pedido_y_restaura_carrito_y_stock(string codigo, bool rechazo)
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await PrepararAsync(conexion);
        await Assert.ThrowsAsync<DomainException>(() => Servicio(db).ConfirmarAsync(Envio, rechazo, codigo));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Pedidos.ToListAsync());
        Assert.Empty(await db.Pagos.ToListAsync());
        Assert.Single(await db.Carritos.ToListAsync());
        Assert.Single(await db.ItemsCarrito.ToListAsync());
        Assert.Equal(3, await db.VariantesProducto.Select(v => v.Stock).SingleAsync());
    }

    [Fact]
    public async Task Pedido_historico_con_metodo_uno_sigue_disponible()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await PrepararAsync(conexion);
        var pedido = new Pedido
        {
            Codigo = "BC-HISTORICO", UsuarioId = 1, Total = 54m,
            Pago = new Pago { Metodo = (MetodoPago)1, Estado = EstadoPago.Aprobado, Monto = 54m, Referencia = "DEMO-ANTERIOR" }
        };
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.NotNull(await Servicio(db).ObtenerAsync(pedido.Id));
        Assert.Equal(MetodoPago.Simulado, (await db.Pagos.SingleAsync()).Metodo);
    }

    [Fact]
    public void Modelo_mysql_conserva_el_esquema_de_las_migraciones()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseMySQL("Server=localhost;Database=solo_modelo;Uid=sin_conexion;").Options);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    private static PedidoService Servicio(AppDbContext db) => new(db, new UsuarioPrueba(),
        new CarritoService(db, new CalculoCarritoService(db, [new SinDescuento()]), TimeProvider.System),
        new StockPrueba(db), new PagoService([new PagoTarjetaSimulado(), new PagoTransferenciaSimulado(),
            new PagoYapeSimulado(), new PagoPlinSimulado()]), TimeProvider.System);

    private static async Task<AppDbContext> PrepararAsync(SqliteConnection conexion)
    {
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conexion).Options);
        await db.Database.EnsureCreatedAsync();
        db.Usuarios.Add(new Usuario { Id = 1, NombreCompleto = "Cliente", Correo = "cliente@prueba.local" });
        var variante = new VarianteProducto
        {
            Id = 1, Sku = "QR-PRUEBA", Stock = 3, Talla = "7",
            Producto = new Producto
            {
                Nombre = "Anillo", Slug = "anillo-prueba", Precio = 39m, Estado = EstadoProducto.Publicado,
                Categoria = new Categoria { Nombre = "Anillos", Slug = "anillos" },
                Color = new Color { Nombre = "Plateado", CodigoHex = "#cccccc" },
                Material = new Material { Nombre = "Acero" }
            }
        };
        db.VariantesProducto.Add(variante);
        db.Carritos.Add(new BlackCode.Domain.Compras.Carrito { UsuarioId = 1, Items = [new ItemCarrito { Variante = variante, Cantidad = 1 }] });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class UsuarioPrueba : IUsuarioActual
    {
        public int? UsuarioId => 1;
        public bool EstaAutenticado => true;
        public bool EsAdmin => false;
    }

    // Sustituye el SQL específico de MySQL por una actualización compatible con la base de pruebas.
    private sealed class StockPrueba(AppDbContext db) : IStockGateway
    {
        public async Task<bool> IntentarDescontarAsync(int varianteId, int cantidad, CancellationToken cancellationToken = default)
            => await db.VariantesProducto.Where(v => v.Id == varianteId && v.Stock >= cantidad)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Stock, v => v.Stock - cantidad), cancellationToken) == 1;

        public async Task RestituirAsync(int varianteId, int cantidad, CancellationToken cancellationToken = default)
            => await db.VariantesProducto.Where(v => v.Id == varianteId)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Stock, v => v.Stock + cantidad), cancellationToken);
    }
}
