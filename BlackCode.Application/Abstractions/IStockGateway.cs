namespace BlackCode.Application.Abstractions;

/// <summary>
/// Descuento de stock atómico: evita que dos personas compren la última unidad a la vez.
/// Úsalo dentro de una transacción (IAppDbContext.BeginTransactionAsync) en el checkout.
/// </summary>
public interface IStockGateway
{
    /// <returns>true si había stock suficiente y se descontó; false si no alcanzaba.</returns>
    Task<bool> IntentarDescontarAsync(int varianteId, int cantidad, CancellationToken cancellationToken = default);

    /// <summary>Devuelve stock, ej. al cancelar un pedido.</summary>
    Task RestituirAsync(int varianteId, int cantidad, CancellationToken cancellationToken = default);
}
