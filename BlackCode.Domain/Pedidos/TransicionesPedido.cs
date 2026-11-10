namespace BlackCode.Domain.Pedidos;

/// <summary>
/// OCP: las transiciones permitidas son DATOS, no if regados por el código.
/// Agregar un estado nuevo = agregar una entrada aquí, sin tocar la lógica.
/// </summary>
public static class TransicionesPedido
{
    private static readonly Dictionary<EstadoPedido, EstadoPedido[]> Permitidas = new()
    {
        [EstadoPedido.PendientePago] = [EstadoPedido.Pagado, EstadoPedido.Cancelado],
        [EstadoPedido.Pagado]        = [EstadoPedido.EnPreparacion, EstadoPedido.Cancelado],
        [EstadoPedido.EnPreparacion] = [EstadoPedido.Enviado, EstadoPedido.Cancelado],
        [EstadoPedido.Enviado]       = [EstadoPedido.Entregado],
    };

    public static bool PuedeMover(EstadoPedido desde, EstadoPedido hacia)
        => Permitidas.TryGetValue(desde, out var destinos) && destinos.Contains(hacia);

    public static IReadOnlyList<EstadoPedido> SiguientesEstados(EstadoPedido desde)
        => Permitidas.TryGetValue(desde, out var destinos) ? destinos : Array.Empty<EstadoPedido>();
}
