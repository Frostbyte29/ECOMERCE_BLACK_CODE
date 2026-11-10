using BlackCode.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Infrastructure.Persistence;

/// <summary>
/// UPDATE condicional en una sola sentencia: si dos personas compran la última unidad
/// al mismo tiempo, MySQL deja pasar solo a una (la otra recibe 0 filas → false).
/// Los {valores} de ExecuteSqlInterpolatedAsync viajan como parámetros: NO es SQL concatenado.
/// </summary>
public sealed class MySqlStockGateway(AppDbContext db) : IStockGateway
{
    public async Task<bool> IntentarDescontarAsync(int varianteId, int cantidad, CancellationToken cancellationToken = default)
    {
        if (cantidad <= 0) return false;

        var rows = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE variantes_producto SET stock = stock - {cantidad} WHERE id = {varianteId} AND activo = 1 AND stock >= {cantidad}",
            cancellationToken);

        return rows == 1;
    }

    public async Task RestituirAsync(int varianteId, int cantidad, CancellationToken cancellationToken = default)
    {
        if (cantidad <= 0) return;

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE variantes_producto SET stock = stock + {cantidad} WHERE id = {varianteId}",
            cancellationToken);
    }
}
