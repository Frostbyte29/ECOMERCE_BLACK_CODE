using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Administracion;
using BlackCode.Domain.Common;

namespace BlackCode.Tests.Catalogo;

public sealed class ImagenProductoTests
{
    private static byte[] Png() => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a1eYAAAAASUVORK5CYII=");
    private static ImagenProductoService Servicio(AlmacenPrueba almacen, bool admin = true) =>
        new(almacen, new UsuarioPrueba(admin), [new FormatoImagenJpeg(), new FormatoImagenPng(), new FormatoImagenWebp()]);

    [Fact]
    public async Task Guarda_png_sin_usar_el_nombre_del_cliente_como_ruta()
    {
        var almacen = new AlmacenPrueba();
        using var contenido = new MemoryStream(Png());
        var ruta = await Servicio(almacen).GuardarAsync(new(contenido, "../../foto.PNG"));
        Assert.Equal("/img/productos/cargas/nueva.png", ruta);
        Assert.Equal(Png(), almacen.Bytes);
        Assert.Equal(".png", almacen.Extension);
    }

    [Theory]
    [InlineData("foto.jpg")]
    [InlineData("foto.svg")]
    [InlineData("foto.exe")]
    public async Task Rechaza_extension_falsa_o_no_admitida(string nombre)
    {
        var almacen = new AlmacenPrueba();
        using var contenido = new MemoryStream(Png());
        await Assert.ThrowsAsync<DomainException>(() => Servicio(almacen).GuardarAsync(new(contenido, nombre)));
        Assert.Null(almacen.Bytes);
    }

    [Fact]
    public async Task Rechaza_png_truncado()
    {
        using var contenido = new MemoryStream(Png()[..20]);
        await Assert.ThrowsAsync<DomainException>(() => Servicio(new()).GuardarAsync(new(contenido, "foto.png")));
    }

    [Fact]
    public async Task Limita_los_bytes_reales_del_stream()
    {
        var almacen = new AlmacenPrueba();
        using var contenido = new MemoryStream(new byte[ImagenProductoService.MaximoBytes + 1]);
        var error = await Assert.ThrowsAsync<DomainException>(() => Servicio(almacen).GuardarAsync(new(contenido, "foto.png")));
        Assert.Contains("5 MB", error.Message);
        Assert.Null(almacen.Bytes);
    }

    [Fact]
    public async Task Cliente_no_puede_subir_imagenes()
    {
        var almacen = new AlmacenPrueba();
        using var contenido = new MemoryStream(Png());
        await Assert.ThrowsAsync<DomainException>(() => Servicio(almacen, false).GuardarAsync(new(contenido, "foto.png")));
        Assert.Null(almacen.Bytes);
    }

    [Fact]
    public async Task Compensa_solo_la_nueva_imagen_si_el_producto_es_rechazado()
    {
        var almacen = new AlmacenPrueba();
        var producto = new ProductosPrueba { Rechazar = true };
        var servicio = new EdicionProductoService(producto, Servicio(almacen));
        using var contenido = new MemoryStream(Png());
        await Assert.ThrowsAsync<DomainException>(() => servicio.GuardarAsync(Dto(), new(contenido, "foto.png")));
        Assert.Equal("/img/productos/cargas/nueva.png", Assert.Single(almacen.Eliminadas));
    }

    [Fact]
    public async Task Editar_sin_archivo_conserva_la_imagen_y_no_escribe_disco()
    {
        var almacen = new AlmacenPrueba();
        var productos = new ProductosPrueba();
        await new EdicionProductoService(productos, Servicio(almacen)).GuardarAsync(Dto(), null);
        Assert.Equal(Dto().Imagen, productos.Guardado!.Imagen);
        Assert.Null(almacen.Bytes);
        Assert.Empty(almacen.Eliminadas);
    }

    [Fact]
    public async Task Guardado_exitoso_asocia_la_ruta_nueva_sin_borrar_la_anterior()
    {
        var almacen = new AlmacenPrueba();
        var productos = new ProductosPrueba();
        using var contenido = new MemoryStream(Png());
        await new EdicionProductoService(productos, Servicio(almacen)).GuardarAsync(Dto(), new(contenido, "foto.png"));
        Assert.Equal("/img/productos/cargas/nueva.png", productos.Guardado!.Imagen);
        Assert.Empty(almacen.Eliminadas);
    }

    private static ProductoEdicionDto Dto() => new(1, "Anillo", "anillo", "Descripción", 49m, 1, 1, 1, true, false,
        "/img/productos/catalogo/anillo-carmesi.jpg", [new(1, "7", 4, true)]);
    private sealed class UsuarioPrueba(bool admin) : IUsuarioActual
    {
        public int? UsuarioId => 1;
        public bool EstaAutenticado => true;
        public bool EsAdmin => admin;
    }
    private sealed class AlmacenPrueba : IAlmacenImagenesProducto
    {
        public byte[]? Bytes { get; private set; }
        public string? Extension { get; private set; }
        public List<string> Eliminadas { get; } = [];
        public Task<string> GuardarAsync(byte[] contenido, string extension)
        {
            Bytes = contenido; Extension = extension;
            return Task.FromResult("/img/productos/cargas/nueva" + extension);
        }
        public Task EliminarAsync(string ruta) { Eliminadas.Add(ruta); return Task.CompletedTask; }
    }
    private sealed class ProductosPrueba : IAdministracionService
    {
        public bool Rechazar { get; init; }
        public ProductoEdicionDto? Guardado { get; private set; }
        public Task GuardarAsync(ProductoEdicionDto producto)
        {
            if (Rechazar) throw new DomainException("Identificador repetido.");
            Guardado = producto; return Task.CompletedTask;
        }
        public Task<AdministracionDto> ConsultarAsync() => throw new NotSupportedException();
        public Task EliminarAsync(int id) => throw new NotSupportedException();
    }
}
