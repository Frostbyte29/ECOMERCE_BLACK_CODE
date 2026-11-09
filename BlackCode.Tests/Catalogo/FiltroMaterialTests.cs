using BlackCode.Application.Features.Catalogo;
using BlackCode.Domain.Catalogo;
using BlackCode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Tests.Catalogo;
public sealed class FiltroMaterialTests
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    [Fact]
    public async Task Filtrar_por_material_devuelve_solo_productos_de_ese_material()
    {
        await using var db = Db();
        var categoria = new Categoria { Nombre = "Anillos", Slug = "anillos", Activo = true };
        var color = new Color { Nombre = "Plateado", CodigoHex = "#cccccc" };
        var acero = new Material { Nombre = "Acero inoxidable", Descripcion = "Duradero y antialérgico" };
        var plata = new Material { Nombre = "Plata 925", Descripcion = "Pura" };
        db.Productos.AddRange(
            new Producto { Nombre = "Anillo Acero", Precio = 39m, Estado = EstadoProducto.Publicado, Categoria = categoria, Color = color, Material = acero },
            new Producto { Nombre = "Anillo Plata", Precio = 89m, Estado = EstadoProducto.Publicado, Categoria = categoria, Color = color, Material = plata });
        await db.SaveChangesAsync();
        var servicio = new CatalogoService(db, [new FiltroMaterial()]);
        var resultado = await servicio.ConsultarAsync(new FiltroCatalogo(Material: acero.Id));
        Assert.Single(resultado.Productos);
        Assert.Equal("Anillo Acero", resultado.Productos[0].Nombre);
    }
    [Fact]
    public async Task Filtrar_por_material_sin_coincidencia_devuelve_lista_vacia()
    {
        await using var db = Db();
        var categoria = new Categoria { Nombre = "Anillos", Slug = "anillos-2", Activo = true };
        var color = new Color { Nombre = "Negro", CodigoHex = "#000000" };
        var acero = new Material { Nombre = "Acero", Descripcion = "Resistente" };
        db.Productos.Add(new Producto { Nombre = "Anillo Acero", Precio = 39m, Estado = EstadoProducto.Publicado, Categoria = categoria, Color = color, Material = acero });
        await db.SaveChangesAsync();
        var servicio = new CatalogoService(db, [new FiltroMaterial()]);
        var resultado = await servicio.ConsultarAsync(new FiltroCatalogo(Material: 9999));
        Assert.Empty(resultado.Productos);
    }
    [Fact]
    public async Task Filtrar_por_material_y_color_combina_criterios()
    {
        await using var db = Db();
        var categoria = new Categoria { Nombre = "Anillos", Slug = "anillos-3", Activo = true };
        var plateado = new Color { Nombre = "Plateado", CodigoHex = "#cccccc" };
        var dorado = new Color { Nombre = "Dorado", CodigoHex = "#ffd700" };
        var acero = new Material { Nombre = "Acero inoxidable", Descripcion = "Resistente" };
        var plata = new Material { Nombre = "Plata", Descripcion = "Pura" };
        db.Productos.AddRange(
            new Producto { Nombre = "Acero Plateado", Precio = 39m, Estado = EstadoProducto.Publicado, Categoria = categoria, Color = plateado, Material = acero },
            new Producto { Nombre = "Acero Dorado", Precio = 49m, Estado = EstadoProducto.Publicado, Categoria = categoria, Color = dorado, Material = acero },
            new Producto { Nombre = "Plata Plateado", Precio = 89m, Estado = EstadoProducto.Publicado, Categoria = categoria, Color = plateado, Material = plata });
        await db.SaveChangesAsync();
        var servicio = new CatalogoService(db, [new FiltroMaterial(), new FiltroColor()]);
        var resultado = await servicio.ConsultarAsync(new FiltroCatalogo(Material: acero.Id, Color: plateado.Id));
        Assert.Single(resultado.Productos);
        Assert.Equal("Acero Plateado", resultado.Productos[0].Nombre);
    }
}
