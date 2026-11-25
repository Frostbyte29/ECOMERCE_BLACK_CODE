using BlackCode.Application.Features.Favoritos;
using BlackCode.Domain.Compras;
using BlackCode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Tests.Favoritos;

public sealed class FavoritosServiceTests
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Toggle_agrega_favorito_si_no_existe()
    {
        await using var db = Db();
        await new FavoritosService(db, TimeProvider.System).ToggleAsync(1, 10);
        Assert.True(await db.Favoritos.AnyAsync(f => f.UsuarioId == 1 && f.ProductoId == 10));
    }

    [Fact]
    public async Task Toggle_quita_favorito_si_ya_existe()
    {
        await using var db = Db();
        db.Favoritos.Add(new Favorito { UsuarioId = 1, ProductoId = 10, CreadoUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        await new FavoritosService(db, TimeProvider.System).ToggleAsync(1, 10);
        Assert.False(await db.Favoritos.AnyAsync(f => f.UsuarioId == 1 && f.ProductoId == 10));
    }

    [Fact]
    public async Task EsFavorito_devuelve_true_cuando_existe_y_false_cuando_no()
    {
        await using var db = Db();
        db.Favoritos.Add(new Favorito { UsuarioId = 2, ProductoId = 5, CreadoUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var svc = new FavoritosService(db, TimeProvider.System);
        Assert.True(await svc.EsFavoritoAsync(2, 5));
        Assert.False(await svc.EsFavoritoAsync(2, 99));
    }

    [Fact]
    public async Task ObtenerIds_devuelve_conjunto_de_ids_del_usuario()
    {
        await using var db = Db();
        db.Favoritos.AddRange(
            new Favorito { UsuarioId = 1, ProductoId = 3, CreadoUtc = DateTime.UtcNow },
            new Favorito { UsuarioId = 1, ProductoId = 7, CreadoUtc = DateTime.UtcNow },
            new Favorito { UsuarioId = 2, ProductoId = 3, CreadoUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var ids = await new FavoritosService(db, TimeProvider.System).ObtenerIdsAsync(1);
        Assert.Equal(2, ids.Count);
        Assert.Contains(3, ids);
        Assert.Contains(7, ids);
        Assert.DoesNotContain(99, ids);
    }
}
