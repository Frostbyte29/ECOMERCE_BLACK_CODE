using BlackCode.Domain.Common;
using BlackCode.Domain.Pedidos;
using Xunit;

namespace BlackCode.Tests.Domain;

public class TransicionesPedidoTests
{
    [Theory]
    [InlineData(EstadoPedido.PendientePago, EstadoPedido.Pagado)]
    [InlineData(EstadoPedido.Pagado, EstadoPedido.EnPreparacion)]
    [InlineData(EstadoPedido.EnPreparacion, EstadoPedido.Enviado)]
    [InlineData(EstadoPedido.Enviado, EstadoPedido.Entregado)]
    [InlineData(EstadoPedido.Pagado, EstadoPedido.Cancelado)]
    public void Permite_transiciones_validas(EstadoPedido from, EstadoPedido to)
        => Assert.True(TransicionesPedido.PuedeMover(from, to));

    [Theory]
    [InlineData(EstadoPedido.Entregado, EstadoPedido.Pagado)]
    [InlineData(EstadoPedido.Enviado, EstadoPedido.Cancelado)]
    [InlineData(EstadoPedido.Cancelado, EstadoPedido.Pagado)]
    [InlineData(EstadoPedido.PendientePago, EstadoPedido.Enviado)]
    public void Rechaza_transiciones_invalidas(EstadoPedido from, EstadoPedido to)
        => Assert.False(TransicionesPedido.PuedeMover(from, to));

    [Fact]
    public void CambiarEstado_invalido_lanza_excepcion_y_no_cambia_nada()
    {
        var pedido = new Pedido { Estado = EstadoPedido.Entregado };

        Assert.Throws<DomainException>(() => pedido.CambiarEstado(EstadoPedido.Pagado, 1, DateTime.UtcNow));

        Assert.Equal(EstadoPedido.Entregado, pedido.Estado);
        Assert.Empty(pedido.Historial);
    }

    [Fact]
    public void CambiarEstado_valido_cambia_estado_y_deja_historial()
    {
        var pedido = new Pedido { Estado = EstadoPedido.Pagado };

        pedido.CambiarEstado(EstadoPedido.EnPreparacion, cambiadoPorUsuarioId: 1, DateTime.UtcNow, "Empacando");

        Assert.Equal(EstadoPedido.EnPreparacion, pedido.Estado);
        var entrada = Assert.Single(pedido.Historial);
        Assert.Equal(EstadoPedido.Pagado, entrada.EstadoAnterior);
        Assert.Equal(EstadoPedido.EnPreparacion, entrada.EstadoNuevo);
    }
}
