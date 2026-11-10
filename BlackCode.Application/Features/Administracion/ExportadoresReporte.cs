using System.Globalization;
using System.Text;
using System.Text.Json;
namespace BlackCode.Application.Features.Administracion;
public sealed class ExportadorCsv : IExportadorReporte
{
    public string Formato => "csv";
    public string ContentType => "text/csv; charset=utf-8";
    public byte[] Exportar(IReadOnlyList<ProductoReporteDto> productos)
    {
        static string Celda(string texto)
        {
            // Evita interpretar nombres como fórmulas al abrirlos en una hoja de cálculo.
            if (texto.TrimStart().StartsWith('=') || texto.TrimStart().StartsWith('+') ||
                texto.TrimStart().StartsWith('-') || texto.TrimStart().StartsWith('@'))
                texto = "'" + texto;
            return "\"" + texto.Replace("\"", "\"\"") + "\"";
        }
        var csv = new StringBuilder("Id;Nombre;Precio;Stock;Estado\r\n");
        foreach (var p in productos)
            csv.AppendLine(string.Join(";", p.Id, Celda(p.Nombre), p.Precio.ToString("0.00", CultureInfo.InvariantCulture), p.Stock, Celda(p.Estado)));
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }
}
public sealed class ExportadorJson : IExportadorReporte
{
    public string Formato => "json";
    public string ContentType => "application/json";
    public byte[] Exportar(IReadOnlyList<ProductoReporteDto> productos)
        => JsonSerializer.SerializeToUtf8Bytes(productos, new JsonSerializerOptions { WriteIndented = true });
}
