using BlackCode.Domain.Pedidos;
using BlackCode.Web.Models;

namespace BlackCode.Tests.Web;

public sealed class TiendaMapperEstadoTests
{
    [Theory]
    [InlineData(EstadoPedido.PendientePago,  "Pendiente de pago")]
    [InlineData(EstadoPedido.Pagado,         "Pago confirmado")]
    [InlineData(EstadoPedido.EnPreparacion,  "Preparando tu pedido")]
    [InlineData(EstadoPedido.Enviado,        "En camino")]
    [InlineData(EstadoPedido.Entregado,      "Entregado")]
    [InlineData(EstadoPedido.Cancelado,      "Cancelado")]
    public void Devuelve_texto_visible_para_cada_estado(EstadoPedido estado, string esperado)
    {
        var resultado = TiendaMapper.Estado(estado.ToString());
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void Todos_los_valores_del_enum_tienen_etiqueta_definida()
    {
        // Verifica que el mapper cubra exactamente los mismos valores que el enum.
        // Si se agrega un EstadoPedido nuevo sin actualizar el mapper, la colección
        // de etiquetas conocidas crece y el Assert.Contains falla.
        var etiquetasConocidas = new HashSet<string>
        {
            "Pendiente de pago",
            "Pago confirmado",
            "Preparando tu pedido",
            "En camino",
            "Entregado",
            "Cancelado"
        };

        foreach (EstadoPedido estado in Enum.GetValues<EstadoPedido>())
        {
            var etiqueta = TiendaMapper.Estado(estado.ToString());
            Assert.Contains(etiqueta, etiquetasConocidas);
        }
    }
}
