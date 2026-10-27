using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Carrito;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
using BlackCode.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Application.Features.Pedidos;
public sealed class PedidoService(IAppDbContext db, IUsuarioActual usuario, ICarritoService carrito, IStockGateway stock, IPagoService pago, TimeProvider reloj) : IPedidoService
{
    private int UsuarioId => usuario.UsuarioId ?? throw new DomainException("Inicia sesión para continuar.");
    public async Task<int> ConfirmarAsync(EnvioDto envio, bool simularRechazo, string metodo = "tarjeta") {
        var uid = UsuarioId;
        if (string.IsNullOrWhiteSpace(envio.Nombre) || envio.Nombre.Length > 120 || string.IsNullOrWhiteSpace(envio.Telefono) || envio.Telefono.Length > 20 ||
            string.IsNullOrWhiteSpace(envio.Direccion) || envio.Direccion.Length > 200 || string.IsNullOrWhiteSpace(envio.Distrito) || envio.Distrito.Length > 80 || envio.Referencia?.Length > 200)
            throw new DomainException("Completa correctamente los datos de envío.");
        await using var tx = await db.BeginTransactionAsync();
        // Eliminar el carrito dentro de la transacción actúa como reclamo exclusivo del checkout.
        // Un doble envío del formulario debe afectar cero filas y no duplicar el pedido.
        var compra = await carrito.ObtenerAsync(new(uid,null));
        if (compra.Lineas.Count == 0) throw new DomainException("Tu carrito está vacío.");
        if (compra.Lineas.Any(l => !l.Disponible || l.Cantidad < 1 || l.Cantidad > l.Stock)) throw new DomainException("Revisa la disponibilidad de los productos en tu carrito.");
        var eliminados = await db.Carritos.Where(c => c.UsuarioId == uid).ExecuteDeleteAsync();
        if (eliminados != 1) throw new DomainException("La compra ya fue procesada. Revisa tus pedidos.");
        foreach (var linea in compra.Lineas.OrderBy(l => l.VarianteId)) {
            if (!await db.Productos.AnyAsync(p => p.Id == linea.ProductoId && p.Estado == EstadoProducto.Publicado)
                || !await stock.IntentarDescontarAsync(linea.VarianteId,linea.Cantidad))
                throw new DomainException("El stock cambió. Revisa el carrito e inténtalo nuevamente.");
        }
        var resultado = await pago.ProcesarAsync(compra.Totales.Total, simularRechazo, metodo);
        if (!resultado.Aprobado) throw new DomainException("Pago de demostración rechazado. Tu carrito y el stock se mantienen.");
        var now = reloj.GetUtcNow().UtcDateTime;
        var pedido = new Pedido {
            Codigo = "BC-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(), UsuarioId = uid,
            Subtotal = compra.Totales.Subtotal - compra.Totales.Descuento, CostoEnvio = compra.Totales.Envio, Total = compra.Totales.Total,
            EnvioNombreCompleto = envio.Nombre.Trim(), EnvioTelefono = envio.Telefono.Trim(), EnvioDireccion = envio.Direccion.Trim(),
            EnvioDistrito = envio.Distrito.Trim(), EnvioReferencia = envio.Referencia?.Trim(), CreadoUtc = now,
            Items = compra.Lineas.Select(l => new ItemPedido { VarianteId = l.VarianteId, NombreProductoCopia = l.Nombre, TallaCopia = l.Talla,
                PrecioUnitario = l.Precio, Cantidad = l.Cantidad, TotalLinea = l.Total }).ToList(),
            Pago = new Pago { Metodo = MetodoPago.Simulado, Estado = EstadoPago.Aprobado, Monto = compra.Totales.Total, Referencia = resultado.Referencia, CreadoUtc = now }
        };
        pedido.CambiarEstado(EstadoPedido.Pagado,uid,now,"Pago simulado aprobado");
        db.Pedidos.Add(pedido); await db.SaveChangesAsync(); await tx.CommitAsync(); return pedido.Id;
    }
    public async Task<IReadOnlyList<PedidoDto>> ListarAsync(bool administracion = false) {
        var uid = UsuarioId;
        if (administracion && !usuario.EsAdmin) throw new DomainException("Acceso restringido.");
        var query = db.Pedidos.AsNoTracking();
        if (!administracion) query = query.Where(p => p.UsuarioId == uid);
        return (await query.Include(p => p.Items).Include(p => p.Historial).OrderByDescending(p => p.CreadoUtc).ToListAsync()).Select(Mapear).ToList();
    }
    public async Task<PedidoDto?> ObtenerAsync(int id) {
        var uid = UsuarioId;
        var p = await db.Pedidos.AsNoTracking().Include(p => p.Items).Include(p => p.Historial).SingleOrDefaultAsync(p => p.Id == id && (p.UsuarioId == uid || usuario.EsAdmin));
        return p is null ? null : Mapear(p);
    }
    public async Task CambiarEstadoAsync(int id, EstadoPedido estado) {
        if (!usuario.EsAdmin) throw new DomainException("Acceso restringido.");
        await using var tx = await db.BeginTransactionAsync();
        var pedido = await db.Pedidos.Include(p => p.Items).SingleOrDefaultAsync(p => p.Id == id) ?? throw new DomainException("Pedido no encontrado.");
        var anterior = pedido.Estado;
        var now = reloj.GetUtcNow().UtcDateTime;
        pedido.CambiarEstado(estado,UsuarioId,now);
        // Comparación atómica del estado: evita dos cancelaciones y dos reposiciones simultáneas.
        var afectados = await db.Pedidos.Where(p => p.Id == id && p.Estado == anterior).ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado,estado));
        if (afectados != 1) throw new DomainException("Otro administrador actualizó el pedido. Recarga la página.");
        if (estado == EstadoPedido.Cancelado)
            foreach (var item in pedido.Items.OrderBy(i => i.VarianteId)) await stock.RestituirAsync(item.VarianteId,item.Cantidad);
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    private static PedidoDto Mapear(Pedido p) => new(p.Id,p.Codigo,p.EnvioNombreCompleto,"",p.Estado,p.Subtotal,p.CostoEnvio,p.Total,p.CreadoUtc,p.EnvioDireccion,p.EnvioDistrito,
        p.Items.Select(i => new LineaPedidoDto(i.NombreProductoCopia,i.TallaCopia,i.PrecioUnitario,i.Cantidad,i.TotalLinea)).ToList(),
        p.Historial.OrderBy(h => h.CambiadoUtc).Select(h => new HistorialDto(h.EstadoNuevo.ToString(),h.CambiadoUtc)).ToList());
}
