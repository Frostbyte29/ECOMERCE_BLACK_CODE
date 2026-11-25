using System.Globalization;
using BlackCode.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Configuracion;

/// <summary>
/// Proyección de las claves de configuracion_tienda que afectan la política comercial visible al cliente.
/// </summary>
public record PoliticaComercialDto(
    string PlazoDias,
    string PlazoCambios,
    decimal CostoEnvio,
    decimal UmbralGratis,
    IReadOnlyList<string> MensajesBarra,
    string? ContactoWhatsApp,
    string? InstagramUrl,
    string? TiktokUrl,
    string? FacebookUrl);

/// <summary>
/// DIP: los consumidores (vistas, controladores) dependen de esta abstracción, nunca de la implementación.
/// </summary>
public interface IPoliticaComercialService
{
    /// <summary>Lee las claves actuales de configuracion_tienda y retorna la política consolidada.</summary>
    Task<PoliticaComercialDto> ObtenerAsync(CancellationToken ct = default);
}

/// <summary>
/// SRP: única razón de cambio = qué valores de política comercial se exponen y con qué defectos.
/// </summary>
public sealed class PoliticaComercialService(IAppDbContext db) : IPoliticaComercialService
{
    public async Task<PoliticaComercialDto> ObtenerAsync(CancellationToken ct = default)
    {
        var config = await db.ConfiguracionesTienda.AsNoTracking()
            .ToDictionaryAsync(c => c.Clave, c => c.Valor, ct);

        string Texto(string clave, string defecto) =>
            config.TryGetValue(clave, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : defecto;

        decimal Decimal(string clave, decimal defecto) =>
            config.TryGetValue(clave, out var v) &&
            decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) && d >= 0
                ? d : defecto;

        string? Url(string clave) =>
            config.TryGetValue(clave, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;

        var mensajes = new List<string>();
        for (var i = 1; i <= 3; i++)
        {
            var msg = Texto($"Banner:Message{i}", string.Empty);
            if (!string.IsNullOrWhiteSpace(msg)) mensajes.Add(msg);
        }

        return new PoliticaComercialDto(
            PlazoDias:      Texto("Policy:ShippingDays", "2 a 4 días hábiles"),
            PlazoCambios:   Texto("Policy:ReturnDays",   "7 días calendario"),
            CostoEnvio:     Decimal("Shipping:FlatCost",       15m),
            UmbralGratis:   Decimal("Shipping:FreeThreshold",  199m),
            MensajesBarra:  mensajes,
            ContactoWhatsApp: Url("Contact:WhatsApp"),
            InstagramUrl:   Url("Social:Instagram"),
            TiktokUrl:      Url("Social:Tiktok"),
            FacebookUrl:    Url("Social:Facebook"));
    }
}
