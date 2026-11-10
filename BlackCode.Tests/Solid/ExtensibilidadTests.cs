using System.Text;
using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Administracion;
using BlackCode.Application.Features.Catalogo;
using BlackCode.Application.Features.Cuenta;
using BlackCode.Application.Features.Pedidos;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
using BlackCode.Domain.Pedidos;
using BlackCode.Domain.Usuarios;
using BlackCode.Infrastructure.Persistence;
using BlackCode.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Tests.Solid;
public sealed class ExtensibilidadTests
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    [Fact]
    public async Task Catalogo_admite_criterio_nuevo_sin_cambiar_consulta()
    {
        await using var db = Db();
        var categoria = new Categoria { Nombre = "Anillos", Slug = "anillos", Activo = true };
        var color = new Color { Nombre = "Plateado", CodigoHex = "#cccccc" };
        var material = new Material { Nombre = "Acero inoxidable" };
        db.Productos.AddRange(new Producto { Nombre = "Económico", Precio = 39m, Estado = EstadoProducto.Publicado, Categoria = categoria, Color = color, Material = material },
            new Producto { Nombre = "Mayor precio", Precio = 89m, Estado = EstadoProducto.Publicado, Categoria = categoria, Color = color, Material = material });
        await db.SaveChangesAsync();
        var catalogo = new CatalogoService(db, [new FiltroPrueba()]);
        Assert.Equal("Económico", Assert.Single((await catalogo.ConsultarAsync(new())).Productos).Nombre);
    }
    [Theory]
    [InlineData("tarjeta", "DEMO-TARJETA-")]
    [InlineData("transferencia", "DEMO-TRANSFERENCIA-")]
    public async Task Pagos_selecciona_implementacion_y_respeta_rechazo(string codigo, string prefijo)
    {
        var servicio = new PagoService([new PagoTarjetaSimulado(), new PagoTransferenciaSimulado()]);
        var aprobado = await servicio.ProcesarAsync(99m, false, codigo);
        Assert.True(aprobado.Aprobado);
        Assert.StartsWith(prefijo, aprobado.Referencia);
        Assert.False((await servicio.ProcesarAsync(99m, true, codigo)).Aprobado);
    }
    [Fact]
    public async Task Pagos_admite_metodo_adicional_sin_cambiar_coordinador()
    {
        var servicio = new PagoService([new MetodoPrueba()]);
        Assert.Equal("REFERENCIA-PRUEBA", (await servicio.ProcesarAsync(10, false, "prueba")).Referencia);
        await Assert.ThrowsAsync<DomainException>(() => servicio.ProcesarAsync(10, false, "inexistente"));
    }
    [Fact]
    public async Task Reporte_admite_exportador_nuevo_y_rechaza_cliente()
    {
        await using var db = Db();
        var servicio = new ReporteService(db, new UsuarioPrueba(true), [new ExportadorPrueba()]);
        var archivo = await servicio.ProductosAsync("txt");
        Assert.Equal("productos-blackcode.txt", archivo.NombreArchivo);
        Assert.Equal("reporte extensible", Encoding.UTF8.GetString(archivo.Contenido));
        var restringido = new ReporteService(db, new UsuarioPrueba(false), [new ExportadorPrueba()]);
        await Assert.ThrowsAsync<DomainException>(() => restringido.ProductosAsync("txt"));
    }
    [Fact]
    public void Exportadores_conservan_datos_y_csv_no_ejecuta_formulas()
    {
        ProductoReporteDto[] filas = [new(1, "=SUM(1;2)", 49.9m, 4, "Publicado")];
        var csv = Encoding.UTF8.GetString(new ExportadorCsv().Exportar(filas));
        Assert.Contains("'=SUM(1;2)", csv);
        Assert.Contains("49.90", csv);
        var json = System.Text.Json.JsonSerializer.Deserialize<ProductoReporteDto[]>(new ExportadorJson().Exportar(filas));
        Assert.Equal(filas[0], Assert.Single(json!));
    }
    [Fact]
    public void Inventario_valida_tallas_y_permisos_independientemente_del_catalogo()
    {
        var inventario = new InventarioService(new UsuarioPrueba(true));
        var producto = new Producto();
        Assert.Throws<DomainException>(() => inventario.Actualizar(producto, [new(0, "7", -1, true)], true));
        Assert.Throws<DomainException>(() => inventario.Actualizar(producto, [new(0, "7", 1, true), new(0, "7", 2, true)], true));
        inventario.Actualizar(producto, [new(0, "7", 5, true)], true);
        Assert.Equal(5, Assert.Single(producto.Variantes).Stock);
        Assert.Throws<DomainException>(() => new InventarioService(new UsuarioPrueba(false)).Actualizar(producto, [], false));
    }
    private sealed class FiltroPrueba : IFiltroProducto
    {
        public IQueryable<Producto> Aplicar(IQueryable<Producto> productos, FiltroCatalogo entrada) => productos.Where(p => p.Precio < 50m);
    }
    private sealed class MetodoPrueba : IMetodoPago
    {
        public string Codigo => "prueba";
        public string Nombre => "Solo pruebas";
        public MetodoPago Metodo => MetodoPago.Simulado;
        public Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo) => Task.FromResult(new ResultadoPagoDto(true, "REFERENCIA-PRUEBA"));
    }
    private sealed class ExportadorPrueba : IExportadorReporte
    {
        public string Formato => "txt";
        public string ContentType => "text/plain";
        public byte[] Exportar(IReadOnlyList<ProductoReporteDto> productos) => Encoding.UTF8.GetBytes("reporte extensible");
    }
    private sealed class UsuarioPrueba(bool admin) : IUsuarioActual
    {
        public int? UsuarioId => 1;
        public bool EstaAutenticado => true;
        public bool EsAdmin => admin;
    }
}
