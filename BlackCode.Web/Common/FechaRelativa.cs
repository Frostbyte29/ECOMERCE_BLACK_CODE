using System.Globalization;

namespace BlackCode.Web.Common;

/// <summary>
/// SRP: formatear una fecha UTC como texto relativo. Estático puro, sin dependencias.
/// </summary>
public static class FechaRelativa
{
    private static readonly CultureInfo Cultura = new("es-PE");

    public static string Formatear(DateTime? fechaUtc, DateTimeOffset ahora)
    {
        if (fechaUtc is null) return "Nunca";

        var local = fechaUtc.Value.ToLocalTime();
        var ahoraLocal = ahora.LocalDateTime;
        var dias = (ahoraLocal.Date - local.Date).TotalDays;

        return dias switch
        {
            0 => $"Hoy, {local:HH:mm}",
            1 => $"Ayer, {local:HH:mm}",
            _ => local.ToString("dd MMM yyyy", Cultura)
        };
    }
}
