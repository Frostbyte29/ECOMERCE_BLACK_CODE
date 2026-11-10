namespace BlackCode.Domain.Common;

/// <summary>
/// Se lanza cuando se intenta romper una regla del negocio
/// (ej. pasar un pedido de Entregado a Pagado).
/// </summary>
public sealed class DomainException(string message) : Exception(message);
