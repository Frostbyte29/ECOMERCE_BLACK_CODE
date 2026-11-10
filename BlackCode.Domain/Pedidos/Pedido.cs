using BlackCode.Domain.Common;

namespace BlackCode.Domain.Pedidos;

public class Pedido
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;   // ej. "BC-000123"
    public int UsuarioId { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.PendientePago;

    public decimal Subtotal { get; set; }
    public decimal CostoEnvio { get; set; }
    public decimal Total { get; set; }

    // Copia de la dirección al momento de comprar (si el cliente la cambia luego, el pedido no cambia)
    public string EnvioNombreCompleto { get; set; } = string.Empty;
    public string EnvioTelefono { get; set; } = string.Empty;
    public string EnvioDireccion { get; set; } = string.Empty;
    public string EnvioDistrito { get; set; } = string.Empty;
    public string? EnvioReferencia { get; set; }

    public DateTime CreadoUtc { get; set; }

    public List<ItemPedido> Items { get; set; } = [];
    public List<HistorialEstadoPedido> Historial { get; set; } = [];
    public Pago? Pago { get; set; }

    /// <summary>
    /// Única forma de cambiar el estado: valida la transición y deja historial.
    /// </summary>
    public void CambiarEstado(EstadoPedido hacia, int cambiadoPorUsuarioId, DateTime ahoraUtc, string? nota = null)
    {
        if (!TransicionesPedido.PuedeMover(Estado, hacia))
            throw new DomainException($"No se puede pasar un pedido de {Estado} a {hacia}.");

        Historial.Add(new HistorialEstadoPedido
        {
            EstadoAnterior = Estado,
            EstadoNuevo = hacia,
            CambiadoPorUsuarioId = cambiadoPorUsuarioId,
            CambiadoUtc = ahoraUtc,
            Nota = nota
        });

        Estado = hacia;
    }
}
