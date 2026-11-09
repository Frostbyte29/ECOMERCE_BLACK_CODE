using BlackCode.Application.Features.Carrito;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
using BlackCode.Domain.Configuracion;
using BlackCode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Tests.Carrito;
public sealed class CarritoServiceTests
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    [Theory]
    [InlineData(198.99,15)]
    [InlineData(199,0)]
    [InlineData(250,0)]
    public async Task Envio_respeta_umbral_de_compra(decimal precio,decimal esperado)
    {
        await using var db=Db();
        db.ConfiguracionesTienda.AddRange(new ConfiguracionTienda{Clave="Shipping:FreeThreshold",Valor="199.00"},new ConfiguracionTienda{Clave="Shipping:FlatCost",Valor="15.00"});
        await db.SaveChangesAsync();
        var calculo=new CalculoCarritoService(db,[new SinDescuento()]);
        var total=await calculo.CalcularAsync([new(1,1,"Anillo","7","",precio,1,10,true)]);
        Assert.Equal(esperado,total.Envio);
        Assert.Equal(precio+esperado,total.Total);
    }
    [Fact]
    public async Task Carrito_vacio_no_cobra_envio()
    {
        await using var db=Db();
        var total=await new CalculoCarritoService(db,[new SinDescuento()]).CalcularAsync([]);
        Assert.Equal(0,total.Total);
    }
    [Fact]
    public async Task Regla_nueva_se_inyecta_sin_modificar_calculadora()
    {
        await using var db=Db();
        var calculo=new CalculoCarritoService(db,[new DescuentoPorCantidad(3,10)]);
        var total=await calculo.CalcularAsync([new(1,1,"Anillo","7","",100m,3,10,true)]);
        Assert.Equal(30,total.Descuento);
        Assert.Equal(270,total.Total);
    }
    [Fact]
    public async Task Descuento_excesivo_no_produce_total_negativo()
    {
        await using var db=Db();
        var calculo=new CalculoCarritoService(db,[new DescuentoExcesivo()]);
        var total=await calculo.CalcularAsync([new(1,1,"Anillo","7","",20m,1,10,true)]);
        Assert.Equal(20,total.Descuento);
        Assert.Equal(15,total.Total);
    }
    [Fact]
    public async Task Invitados_tienen_carritos_aislados_y_no_reservan_stock()
    {
        await using var db=Db();
        var variante=new VarianteProducto { Id=1,Talla="7",Stock=3,Producto=new Producto {Id=1,Nombre="Anillo",Precio=39m,Estado=EstadoProducto.Publicado}};
        db.VariantesProducto.Add(variante); await db.SaveChangesAsync();
        var servicio=new CarritoService(db,new CalculoCarritoService(db,[]),TimeProvider.System);
        await servicio.AgregarAsync(new(null,"invitado-a"),1,2);
        Assert.Empty((await servicio.ObtenerAsync(new(null,"invitado-b"))).Lineas);
        Assert.Single((await servicio.ObtenerAsync(new(null,"invitado-a"))).Lineas);
        Assert.Equal(3,variante.Stock);
        await Assert.ThrowsAsync<DomainException>(()=>servicio.AgregarAsync(new(null,"invitado-a"),1,2));
        await Assert.ThrowsAsync<DomainException>(()=>servicio.AgregarAsync(new(null,"invitado-a"),1,-1));
    }
    [Fact]
    public async Task Fusionar_conserva_seleccion_y_retira_identidad_anonima()
    {
        await using var db=Db();
        var variante=new VarianteProducto { Id=1,Talla="7",Stock=10,Producto=new Producto {Id=1,Nombre="Anillo",Precio=39m,Estado=EstadoProducto.Publicado}};
        db.VariantesProducto.Add(variante); await db.SaveChangesAsync();
        var servicio=new CarritoService(db,new CalculoCarritoService(db,[]),TimeProvider.System);
        await servicio.AgregarAsync(new(null,"invitado-a"),1,2);
        await servicio.AgregarAsync(new(42,null),1,1);
        await servicio.FusionarAsync("invitado-a",42);
        Assert.Equal(3,Assert.Single((await servicio.ObtenerAsync(new(42,null))).Lineas).Cantidad);
        Assert.Empty((await servicio.ObtenerAsync(new(null,"invitado-a"))).Lineas);
    }
    private sealed class DescuentoExcesivo : IReglaDescuento
    {
        public decimal Calcular(IReadOnlyList<LineaCarritoDto> lineas)=>999;
    }
}
