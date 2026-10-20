namespace BlackCode.Web.Services;

// Las fechas se guardan en UTC y se muestran en hora de Perú (UTC-5, sin horario de verano).
public static class PeruTime
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(-5);

    public static DateTime Now => DateTime.UtcNow + Offset;
    public static DateTime Today => Now.Date;

    public static DateTime FromUtc(DateTime utc) => utc + Offset;
    public static DateTime ToUtc(DateTime peru) => peru - Offset;

    public static string Format(DateTime utc) => FromUtc(utc).ToString("dd/MM/yyyy HH:mm");
    public static string FormatDate(DateTime utc) => FromUtc(utc).ToString("dd/MM/yyyy");
}
