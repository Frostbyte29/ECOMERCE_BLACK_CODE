using System.Globalization;
using BlackCode.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Application.Features.Carrito;
// SRP: calcula importes sin modificar artículos. OCP: nuevas reglas se inyectan mediante IReglaDescuento.
public sealed class CalculoCarritoService(IAppDbContext db, IEnumerable<IReglaDescuento> reglas) : ICalculoCarritoService
{
    public async Task<TotalesDto> CalcularAsync(IReadOnlyList<LineaCarritoDto> lineas)
    {
        var subtotal = lineas.Sum(l => l.Total);
        var descuento = Math.Round(Math.Clamp(reglas.Sum(r => r.Calcular(lineas)), 0, subtotal), 2, MidpointRounding.AwayFromZero);
        var valores = await db.ConfiguracionesTienda.AsNoTracking().Where(c => c.Clave.StartsWith("Shipping:")).ToDictionaryAsync(c => c.Clave,c => c.Valor);
        decimal Leer(string clave, decimal defecto) => valores.TryGetValue(clave,out var texto) && decimal.TryParse(texto,NumberStyles.Number,CultureInfo.InvariantCulture,out var valor) && valor >= 0 ? valor : defecto;
        var envio = lineas.Count == 0 || subtotal - descuento >= Leer("Shipping:FreeThreshold",199m) ? 0 : Leer("Shipping:FlatCost",15m);
        return new(subtotal,descuento,envio);
    }
}
public sealed class SinDescuento : IReglaDescuento
{
    public decimal Calcular(IReadOnlyList<LineaCarritoDto> lineas) => 0m;
}
// Ejemplo de extensión; no se registra por defecto para no inventar promociones comerciales.
public sealed class DescuentoPorCantidad(int minimo, decimal porcentaje) : IReglaDescuento
{
    public decimal Calcular(IReadOnlyList<LineaCarritoDto> lineas) =>
        lineas.Sum(l => l.Cantidad) >= minimo ? lineas.Sum(l => l.Total) * Math.Clamp(porcentaje,0,100) / 100m : 0m;
}
