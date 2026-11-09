using BlackCode.Domain.Catalogo;
using BlackCode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Tests.Catalogo;

public sealed class CatalogoFotograficoTests
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Crea_la_coleccion_con_caracteristicas_y_stock_por_talla()
    {
        await using var db = Db();
        await CatalogoFotograficoSeeder.AplicarAsync(db, TimeProvider.System);
        var productos = await db.Productos.Include(p => p.Variantes).Include(p => p.Imagenes)
            .Include(p => p.Material).Include(p => p.Color).ToListAsync();
        Assert.Equal(19, productos.Count);
        Assert.Equal(4, productos.Count(p => p.Destacado));
        Assert.All(productos, p =>
        {
            Assert.True(p.Precio > 0);
            Assert.NotEmpty(p.Descripcion);
            Assert.NotEmpty(p.Color.Nombre);
            Assert.Equal("Acero inoxidable 316L", p.Material.Nombre);
            Assert.Equal(new[] { "7", "8", "9", "10", "11" }, p.Variantes.Select(v => v.Talla));
            Assert.All(p.Variantes, v => Assert.Equal(10, v.Stock));
            Assert.StartsWith("/img/productos/catalogo/", Assert.Single(p.Imagenes).Url);
        });
    }

    [Fact]
    public async Task Reiniciar_no_duplica_ni_restaura_precio_stock_o_estado_editados()
    {
        await using var db = Db();
        await CatalogoFotograficoSeeder.AplicarAsync(db, TimeProvider.System);
        var producto = await db.Productos.Include(p => p.Variantes).FirstAsync();
        producto.Precio = 123m;
        producto.Estado = EstadoProducto.Borrador;
        producto.Variantes[0].Stock = 2;
        await db.SaveChangesAsync();
        await CatalogoFotograficoSeeder.AplicarAsync(db, TimeProvider.System);
        Assert.Equal(19, await db.Productos.CountAsync());
        Assert.Equal(123m, producto.Precio);
        Assert.Equal(EstadoProducto.Borrador, producto.Estado);
        Assert.Equal(2, producto.Variantes[0].Stock);
    }

    [Fact]
    public async Task Retira_demo_sin_borrar_sus_variantes_ni_productos_personalizados()
    {
        await using var db = Db();
        var anterior = new Producto { Nombre = "Profano", Slug = "anillo-profano", Precio = 39m,
            Estado = EstadoProducto.Publicado, Imagenes = [new() { Url = "/img/productos/anillo-profano.svg" }],
            Variantes = [new() { Talla = "7", Stock = 3, Sku = "ORIGINAL" }] };
        var propio = new Producto { Nombre = "Personalizado", Slug = "propio", Estado = EstadoProducto.Publicado };
        db.Productos.AddRange(anterior, propio);
        await db.SaveChangesAsync();
        var idVariante = anterior.Variantes[0].Id;
        await CatalogoFotograficoSeeder.AplicarAsync(db, TimeProvider.System);
        Assert.Equal(EstadoProducto.Borrador, anterior.Estado);
        Assert.Equal(39m, anterior.Precio);
        Assert.Equal(3, (await db.VariantesProducto.SingleAsync(v => v.Id == idVariante)).Stock);
        Assert.Equal(EstadoProducto.Publicado, propio.Estado);
    }
}
