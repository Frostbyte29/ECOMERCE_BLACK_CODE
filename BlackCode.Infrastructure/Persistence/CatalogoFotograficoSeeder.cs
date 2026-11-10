using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Configuracion;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Infrastructure.Persistence;

// SRP: incorpora una colección inicial una sola vez sin reescribir la edición administrativa.
public static class CatalogoFotograficoSeeder
{
    public const string ClaveVersion = "Catalogo:Fotografias:v1";

    public static async Task AplicarAsync(AppDbContext db, TimeProvider reloj, CancellationToken ct = default)
    {
        if (await db.ConfiguracionesTienda.AnyAsync(c => c.Clave == ClaveVersion, ct)) return;

        var categoria = await db.Categorias.SingleOrDefaultAsync(c => c.Slug == "anillos", ct);
        if (categoria is null)
        {
            categoria = new Categoria { Nombre = "Anillos", Slug = "anillos", Orden = 1 };
            db.Categorias.Add(categoria);
        }
        categoria.UrlImagen = "/img/productos/catalogo/anillo-carmesi.jpg";
        var material = await db.Materiales.FirstOrDefaultAsync(m => m.Nombre == "Acero inoxidable 316L", ct);
        if (material is null)
        {
            material = new Material { Nombre = "Acero inoxidable 316L", Descripcion = "Acabado metálico de la colección." };
            db.Materiales.Add(material);
        }
        var colores = await db.Colores.ToListAsync(ct);
        var existentes = await db.Productos.Include(p => p.Imagenes).ToListAsync(ct);
        string[] iniciales = ["anillo-profano", "anillo-aureo", "anillo-bruma", "anillo-tizzo", "anillo-vertice", "anillo-eslabon"];
        foreach (var producto in existentes.Where(p => iniciales.Contains(p.Slug)
                     && p.Imagenes.Any(i => i.Url == $"/img/productos/{p.Slug}.svg")))
            producto.Estado = EstadoProducto.Borrador;

        foreach (var entrada in CatalogoFotografico.Productos)
        {
            if (existentes.Any(p => p.Slug == entrada.Slug)) continue;
            var color = colores.FirstOrDefault(c => c.Nombre == entrada.Color);
            if (color is null)
            {
                color = new Color { Nombre = entrada.Color, CodigoHex = entrada.CodigoHex };
                colores.Add(color);
                db.Colores.Add(color);
            }
            db.Productos.Add(new Producto
            {
                Nombre = entrada.Nombre, Slug = entrada.Slug, Precio = entrada.Precio,
                Descripcion = entrada.Descripcion, Color = color, Categoria = categoria, Material = material,
                Destacado = entrada.Destacado, Estado = EstadoProducto.Publicado, CreadoUtc = reloj.GetUtcNow().UtcDateTime,
                Imagenes = [new ImagenProducto { Url = $"/img/productos/catalogo/{entrada.Slug}.jpg", Orden = 1 }],
                Variantes = new[] { "7", "8", "9", "10", "11" }.Select(talla => new VarianteProducto
                {
                    Talla = talla, Sku = $"FOTO-{entrada.Slug.ToUpperInvariant()}-{talla}", Stock = 10, Activo = true
                }).ToList()
            });
        }
        db.ConfiguracionesTienda.Add(new ConfiguracionTienda { Clave = ClaveVersion, Valor = "1" });
        await db.SaveChangesAsync(ct);
    }
}
