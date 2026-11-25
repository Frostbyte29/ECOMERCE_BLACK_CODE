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
        await EnsureConfigDefaultsAsync(db, ct);

        if (await db.Usuarios.AnyAsync(ct))
        {
            await CatalogoFotograficoSeeder.AplicarAsync(db, services.GetRequiredService<TimeProvider>(), ct);
            return;
        }

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

        // Las claves de configuración se siembran en EnsureConfigDefaultsAsync
        // para que también lleguen a bases ya existentes.

        await db.SaveChangesAsync(ct);
        await CatalogoFotograficoSeeder.AplicarAsync(db, services.GetRequiredService<TimeProvider>(), ct);
    }

    /// <summary>
    /// Siembra los valores por defecto de configuracion_tienda que falten.
    /// Se llama siempre (antes del guard de usuarios) para que las bases ya existentes
    /// reciban las claves nuevas sin necesidad de Drop-Database.
    /// </summary>
    private static async Task EnsureConfigDefaultsAsync(AppDbContext db, CancellationToken ct)
    {
        var existentes = await db.ConfiguracionesTienda.Select(c => c.Clave).ToListAsync(ct);
        var pendientes = new List<ConfiguracionTienda>();

        void Agregar(string clave, string valor)
        {
            if (!existentes.Contains(clave))
                pendientes.Add(new ConfiguracionTienda { Clave = clave, Valor = valor });
        }

        // Envío — claves originales conservadas sin renombrar
        Agregar("Shipping:FreeThreshold", "199.00");
        Agregar("Shipping:FlatCost",      "15.00");

        // Política comercial
        Agregar("Policy:ShippingDays", "2 a 4 días hábiles");
        Agregar("Policy:ReturnDays",   "7 días calendario");

        // Barra de anuncio superior (vacío = no se muestra)
        Agregar("Banner:Message1", "ENVÍOS A TODO EL PERÚ");
        Agregar("Banner:Message2", "CAMBIOS FÁCILES");
        Agregar("Banner:Message3", "PAGA SEGURO");

        // Contacto y redes sociales (vacío = no se muestra)
        Agregar("Contact:WhatsApp",  string.Empty);
        Agregar("Social:Instagram",  string.Empty);
        Agregar("Social:Tiktok",     string.Empty);
        Agregar("Social:Facebook",   string.Empty);

        if (pendientes.Count > 0)
        {
            db.ConfiguracionesTienda.AddRange(pendientes);
            await db.SaveChangesAsync(ct);
        }
    }
}
