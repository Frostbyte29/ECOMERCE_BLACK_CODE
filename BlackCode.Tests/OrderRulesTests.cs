using BlackCode.Web.Models;
using BlackCode.Web.Services;

namespace BlackCode.Tests;

public class OrderRulesTests
{
    [Fact]
    public void SoloLosPedidosPendientesPuedenSerCanceladosPorElCliente()
    {
        Assert.True(OrderService.CustomerCanCancel(new Order { Status = OrderStatus.PendientePago }));
        Assert.False(OrderService.CustomerCanCancel(new Order { Status = OrderStatus.Pagado }));
        Assert.False(OrderService.CustomerCanCancel(new Order { Status = OrderStatus.Enviado }));
    }

    [Fact]
    public void ElPlazoDePagoEsDeTresDiasDesdeLaCreacion()
    {
        var created = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal(created.AddDays(3), OrderService.PaymentDueAt(new Order { CreatedAt = created }));
    }

    [Fact]
    public void UnPedidoEnviadoNoPuedeVolverAPreparacion()
    {
        Assert.Equal([OrderStatus.Entregado], OrderService.AllowedTransitions(OrderStatus.Enviado));
        Assert.Empty(OrderService.AllowedTransitions(OrderStatus.Cancelado));
    }
}
