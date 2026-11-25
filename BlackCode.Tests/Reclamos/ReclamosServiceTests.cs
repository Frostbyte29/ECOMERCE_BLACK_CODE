using BlackCode.Application.Features.Reclamos;
using BlackCode.Domain.Reclamos;
using BlackCode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Tests.Reclamos;

public sealed class ReclamosServiceTests
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ReclamoDto DtoEjemplo(TipoReclamo tipo = TipoReclamo.Reclamo) => new(
        "Juan Perez", "12345678", "Av. Lima 123", "juan@test.com",
        null, null, tipo, "Anillo de acero", "El producto llegó dañado.", "Solicito cambio.");

    [Fact]
    public async Task Registrar_guarda_reclamo_y_devuelve_id_positivo()
    {
        await using var db = Db();
        var id = await new ReclamosService(db, TimeProvider.System).RegistrarAsync(null, DtoEjemplo());
        Assert.True(id > 0);
        var r = await db.Reclamos.SingleAsync();
        Assert.Equal("Juan Perez", r.NombresCompletos);
        Assert.Equal(TipoReclamo.Reclamo, r.Tipo);
        Assert.Null(r.UsuarioId);
    }

    [Fact]
    public async Task Registrar_con_usuario_autenticado_asocia_id()
    {
        await using var db = Db();
        await new ReclamosService(db, TimeProvider.System).RegistrarAsync(5, DtoEjemplo(TipoReclamo.Queja));
        var r = await db.Reclamos.SingleAsync();
        Assert.Equal(5, r.UsuarioId);
        Assert.Equal(TipoReclamo.Queja, r.Tipo);
    }
}
