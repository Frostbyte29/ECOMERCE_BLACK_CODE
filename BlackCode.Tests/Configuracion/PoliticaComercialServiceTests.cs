using BlackCode.Application.Features.Configuracion;
using BlackCode.Domain.Configuracion;
using BlackCode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Tests.Configuracion;

public sealed class PoliticaComercialServiceTests
{
    private static AppDbContext Db() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Devuelve_valores_almacenados_en_base_de_datos()
    {
        await using var db = Db();
        db.ConfiguracionesTienda.AddRange(
            new ConfiguracionTienda { Clave = "Policy:ShippingDays", Valor = "3 días hábiles" },
            new ConfiguracionTienda { Clave = "Policy:ReturnDays",   Valor = "14 días calendario" },
            new ConfiguracionTienda { Clave = "Shipping:FlatCost",   Valor = "20.00" },
            new ConfiguracionTienda { Clave = "Shipping:FreeThreshold", Valor = "250.00" });
        await db.SaveChangesAsync();

        var politica = await new PoliticaComercialService(db).ObtenerAsync();

        Assert.Equal("3 días hábiles",     politica.PlazoDias);
        Assert.Equal("14 días calendario", politica.PlazoCambios);
        Assert.Equal(20m,                  politica.CostoEnvio);
        Assert.Equal(250m,                 politica.UmbralGratis);
    }

    [Fact]
    public async Task Usa_valores_por_defecto_cuando_no_existen_claves()
    {
        await using var db = Db();

        var politica = await new PoliticaComercialService(db).ObtenerAsync();

        Assert.Equal("2 a 4 días hábiles", politica.PlazoDias);
        Assert.Equal("7 días calendario",  politica.PlazoCambios);
        Assert.Equal(15m,                  politica.CostoEnvio);
        Assert.Equal(199m,                 politica.UmbralGratis);
        Assert.Empty(politica.MensajesBarra);
        Assert.Null(politica.InstagramUrl);
        Assert.Null(politica.TiktokUrl);
        Assert.Null(politica.FacebookUrl);
        Assert.Null(politica.ContactoWhatsApp);
    }

    [Fact]
    public async Task Url_vacia_o_espacios_devuelve_null()
    {
        await using var db = Db();
        db.ConfiguracionesTienda.AddRange(
            new ConfiguracionTienda { Clave = "Social:Instagram", Valor = "" },
            new ConfiguracionTienda { Clave = "Social:Tiktok",    Valor = "   " },
            new ConfiguracionTienda { Clave = "Social:Facebook",  Valor = "https://facebook.com/blackcode" });
        await db.SaveChangesAsync();

        var politica = await new PoliticaComercialService(db).ObtenerAsync();

        Assert.Null(politica.InstagramUrl);
        Assert.Null(politica.TiktokUrl);
        Assert.Equal("https://facebook.com/blackcode", politica.FacebookUrl);
    }

    [Fact]
    public async Task Mensajes_barra_excluye_vacios_y_preserva_orden()
    {
        await using var db = Db();
        db.ConfiguracionesTienda.AddRange(
            new ConfiguracionTienda { Clave = "Banner:Message1", Valor = "MENSAJE A" },
            new ConfiguracionTienda { Clave = "Banner:Message2", Valor = "" },
            new ConfiguracionTienda { Clave = "Banner:Message3", Valor = "MENSAJE C" });
        await db.SaveChangesAsync();

        var politica = await new PoliticaComercialService(db).ObtenerAsync();

        Assert.Equal(2, politica.MensajesBarra.Count);
        Assert.Equal("MENSAJE A", politica.MensajesBarra[0]);
        Assert.Equal("MENSAJE C", politica.MensajesBarra[1]);
    }
}
