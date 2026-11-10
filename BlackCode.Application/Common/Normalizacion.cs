namespace BlackCode.Application.Common;

/// <summary>
/// Utilidades de normalización de texto compartidas entre servicios de Application.
/// </summary>
internal static class Normalizacion
{
    internal static string Correo(string correo) => correo.Trim().ToLowerInvariant();
}
