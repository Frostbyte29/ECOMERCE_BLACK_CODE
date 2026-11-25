using BlackCode.Application;
using BlackCode.Application.Features.Pedidos;
using BlackCode.Domain.Common;
using BlackCode.Domain.Pedidos;
using Microsoft.Extensions.DependencyInjection;

namespace BlackCode.Tests.Pedidos;

public sealed class PagoServiceTests
{
    [Theory]
    [InlineData("tarjeta", MetodoPago.Tarjeta, "DEMO-TARJETA-", true)]
    [InlineData("transferencia", MetodoPago.Transferencia, "DEMO-TRANSFERENCIA-", false)]
    [InlineData("yape", MetodoPago.Yape, "DEMO-YAPE-", false)]
    [InlineData("plin", MetodoPago.Plin, "DEMO-PLIN-", false)]
    public async Task Registro_resuelve_metodo_y_conserva_su_identidad(string codigo, MetodoPago esperado, string prefijo, bool pagoInmediato)
    {
        using var provider = new ServiceCollection().AddApplication().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var servicio = scope.ServiceProvider.GetRequiredService<IPagoService>();
        var resultado = await servicio.ProcesarAsync(54m, false, codigo);
        Assert.Equal(pagoInmediato, resultado.Aprobado);
        Assert.Equal(!pagoInmediato, resultado.EsPendiente);
        Assert.Equal(esperado, resultado.Metodo);
        Assert.StartsWith(prefijo, resultado.Referencia);
        var rechazado = await servicio.ProcesarAsync(54m, true, codigo);
        Assert.False(rechazado.Aprobado);
        Assert.False(rechazado.EsPendiente);
        Assert.False((await servicio.ProcesarAsync(0m, false, codigo)).Aprobado);
        Assert.False((await servicio.ProcesarAsync(-1m, false, codigo)).Aprobado);
    }

    [Fact]
    public void Listado_del_checkout_proviene_de_los_metodos_registrados()
    {
        using var provider = new ServiceCollection().AddApplication().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var metodos = scope.ServiceProvider.GetRequiredService<IPagoService>().Metodos();
        Assert.Equal(new[] { "tarjeta", "yape", "plin", "transferencia" }, metodos.Select(m => m.Codigo));
        Assert.Equal(new[] { "Tarjeta", "Yape", "Plin", "Transferencia" }, metodos.Select(m => m.Nombre));
    }

    [Fact]
    public async Task Codigo_inexistente_produce_error_controlado()
    {
        var servicio = new PagoService([new PagoYapeSimulado(), new PagoPlinSimulado()]);
        var error = await Assert.ThrowsAsync<DomainException>(() => servicio.ProcesarAsync(54m, false, "inexistente"));
        Assert.Equal("El método de pago seleccionado no está disponible.", error.Message);
    }

    [Fact]
    public void Valores_persistidos_conservan_compatibilidad()
    {
        Assert.Equal(1, (int)MetodoPago.Simulado);
        Assert.Equal(2, (int)MetodoPago.Tarjeta);
        Assert.Equal(3, (int)MetodoPago.Transferencia);
        Assert.Equal(4, (int)MetodoPago.Yape);
        Assert.Equal(5, (int)MetodoPago.Plin);
    }
}
