using BlackCode.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Seguimiento;

/// <summary>
/// SRP: consulta públicamente el estado y el historial de un pedido por su código.
/// DIP: depende de IAppDbContext, no de AppDbContext.
/// </summary>
public sealed class SeguimientoService(IAppDbContext db) : ISeguimientoService
{
    public async Task<SeguimientoDto?> ObtenerPorCodigoAsync(string codigo)
    {
        var pedido = await db.Pedidos
            .AsNoTracking()
            .Include(p => p.Historial)
            .FirstOrDefaultAsync(p => p.Codigo == codigo.ToUpperInvariant());

        if (pedido is null) return null;

        var seguimiento = await db.SeguimientosPedido
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.PedidoId == pedido.Id);

        var historial = pedido.Historial
            .OrderBy(h => h.CambiadoUtc)
            .Select(h => new EventoSeguimientoDto(h.EstadoNuevo.ToString(), h.CambiadoUtc))
            .ToList();

        return new SeguimientoDto(
            pedido.Codigo,
            pedido.Estado.ToString(),
            historial,
            seguimiento?.Transportista,
            seguimiento?.CodigoRastreo);
    }
}
