using BlackCode.Application.Abstractions;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Configuracion;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlackCode.Infrastructure.Persistence;

/// <summary>
/// Solo en Desarrollo: aplica las migraciones pendientes y carga datos de demo.
/// Es idempotente: si ya hay usuarios, no vuelve a sembrar.
/// Para empezar de cero: en la Consola del Administrador de paquetes, Drop-Database y luego F5.
/// </summary>
public static class DbSeeder
{
    private static readonly string[] RingSizes = ["7", "8", "9", "10", "11"];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);

        if (await db.Usuarios.AnyAsync(ct)) return;

        var passwords = services.GetRequiredService<IContrasenaService>();
        var config = services.GetRequiredService<IConfiguration>();
        var now = services.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;

        string Secret(string key) => config[key]
            ?? throw new InvalidOperationException(
                $"Falta '{key}' en los secretos de usuario de BlackCode.Web.");

        // ---------- Usuarios de demo (las claves vienen de los secretos, nunca del código) ----------
        db.Usuarios.AddRange(
            new Usuario
            {
                Correo = "admin@blackcode.pe",
                NombreCompleto = "Administrador Black Code",
                Rol = RolUsuario.Admin,
                HashContrasena = passwords.Hashear(Secret("Seed:AdminPassword")),
                TerminosAceptadosUtc = now,
                CreadoUtc = now
            },
            new Usuario
            {
                Correo = "cliente@demo.pe",
                NombreCompleto = "Cliente Demo",
                Telefono = "999999999",
                Rol = RolUsuario.Cliente,
                HashContrasena = passwords.Hashear(Secret("Seed:ClientePassword")),
                TerminosAceptadosUtc = now,
                CreadoUtc = now
            });

        // ---------- Catálogos ----------
        var acero = new Material
        {
            Nombre = "Acero inoxidable 316L",
            Descripcion = "Resistente al agua, al desgaste y al uso diario."
        };
        db.Materiales.Add(acero);

        var plateado = new Color { Nombre = "Plateado", CodigoHex = "#C0C0C0" };
        var negro = new Color { Nombre = "Negro", CodigoHex = "#111111" };
        var dorado = new Color { Nombre = "Dorado", CodigoHex = "#C9A24A" };
        db.Colores.AddRange(plateado, negro, dorado);

        var anillos = new Categoria { Nombre = "Anillos", Slug = "anillos", Orden = 1, UrlImagen = "/img/productos/anillo-profano.svg" };
        var cadenas = new Categoria { Nombre = "Cadenas", Slug = "cadenas", Orden = 2, UrlImagen = "/img/productos/cadena-eslabones.svg" };
        var pulseras = new Categoria { Nombre = "Pulseras", Slug = "pulseras", Orden = 3, UrlImagen = "/img/productos/pulsera-eslabones.svg" };
        db.Categorias.AddRange(anillos, cadenas, pulseras);

        // Nombres y precios del proyecto base; ilustraciones SVG locales para este prototipo.
        Producto Ring(string nombre, string slug, decimal precio, Color color, bool destacado, string descripcion) => new()
        {
            Nombre = nombre,
            Slug = slug,
            Precio = precio,
            Descripcion = descripcion,
            Categoria = anillos,
            Material = acero,
            Color = color,
            Destacado = destacado,
            Estado = EstadoProducto.Publicado,
            CreadoUtc = now,
            Variantes = RingSizes
                .Select(talla => new VarianteProducto { Talla = talla, Sku = $"{slug.ToUpperInvariant()}-{talla}", Stock = 10 })
                .ToList(),
            Imagenes = [new ImagenProducto { Url = $"/img/productos/{slug}.svg", Orden = 1 }]
        };

        const string genericRing = "Anillo de acero inoxidable 316L.";

        db.Productos.AddRange(
            Ring("Anillo Profano", "anillo-profano", 39.00m, plateado, true,
                "Diseño sólido y atemporal que refleja carácter. Anillo Profano combina líneas geométricas y un acabado envejecido que resalta su esencia rebelde y única."),
            Ring("Anillo Aureo", "anillo-aureo", 55.00m, negro, true, genericRing),
            Ring("Anillo Bruma", "anillo-bruma", 49.00m, negro, true, genericRing),
            Ring("Anillo Tizzo", "anillo-tizzo", 45.00m, plateado, true, genericRing),
            Ring("Anillo Vértice", "anillo-vertice", 59.00m, negro, false, genericRing),
            Ring("Anillo Eslabón", "anillo-eslabon", 52.00m, plateado, false, genericRing));

        Producto Accesorio(string nombre, string slug, Categoria categoria, decimal precio) => new()
        {
            Nombre = nombre, Slug = slug, Precio = precio, Categoria = categoria,
            Material = acero, Color = plateado, Destacado = false,
            Descripcion = "Eslabones de acero inoxidable. Una pieza versátil para expresar tu identidad.",
            Estado = EstadoProducto.Publicado, CreadoUtc = now,
            Variantes = [new VarianteProducto { Talla = "Única", Sku = slug.ToUpperInvariant(), Stock = 10 }],
            Imagenes = [new ImagenProducto { Url = $"/img/productos/{slug}.svg", Orden = 1 }]
        };
        db.Productos.AddRange(Accesorio("Cadena Eslabones", "cadena-eslabones", cadenas, 89m),
            Accesorio("Pulsera Eslabones", "pulsera-eslabones", pulseras, 69m));

        // ---------- Configuración de la tienda ----------
        db.ConfiguracionesTienda.AddRange(
            new ConfiguracionTienda { Clave = "Shipping:FreeThreshold", Valor = "199.00" },
            new ConfiguracionTienda { Clave = "Shipping:FlatCost", Valor = "15.00" });

        await db.SaveChangesAsync(ct);
    }
}
