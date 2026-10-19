namespace BlackCode.Application.Common;

/// <summary>Resultado de una acción que puede fallar por una regla del negocio (sin lanzar excepciones).</summary>
public sealed record ResultadoOperacion(bool Exitoso, string? Error = null)
{
    public static ResultadoOperacion Ok() => new(true);
    public static ResultadoOperacion Fallo(string error) => new(false, error);
}
