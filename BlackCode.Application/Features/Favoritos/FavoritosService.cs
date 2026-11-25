using BlackCode.Application.Abstractions;
using BlackCode.Domain.Compras;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Favoritos;

/// <summary>
/// SRP: gestiona únicamente la lista de favoritos del cliente.
/// DIP: depende de IAppDbContext, no de AppDbContext.
/// </summary>
public sealed class FavoritosService(IAppDbContext db, TimeProvider reloj) : IFavoritosService
{
    public async Task ToggleAsync(int usuarioId, int productoId)
    {
        var existente = await db.Favoritos
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.ProductoId == productoId);

        if (existente is null)
            db.Favoritos.Add(new Favorito { UsuarioId = usuarioId, ProductoId = productoId, CreadoUtc = reloj.GetUtcNow().UtcDateTime });
        else
            db.Favoritos.Remove(existente);

        await db.SaveChangesAsync();
    }

    public Task<bool> EsFavoritoAsync(int usuarioId, int productoId)
        => db.Favoritos.AnyAsync(f => f.UsuarioId == usuarioId && f.ProductoId == productoId);

    public async Task<IReadOnlyList<FavoritoDto>> ListarAsync(int usuarioId)
        => await db.Favoritos
            .AsNoTracking()
            .Where(f => f.UsuarioId == usuarioId)
            .Select(f => new FavoritoDto(
                f.Producto.Id,
                f.Producto.Nombre,
                f.Producto.Slug,
                f.Producto.Imagenes.OrderBy(i => i.Orden).Select(i => i.Url).FirstOrDefault() ?? "/img/productos/anillo-profano.svg",
                f.Producto.Precio,
                f.Producto.Categoria.Nombre,
                f.Producto.Variantes.Any(v => v.Activo && v.Stock > 0)))
            .ToListAsync();

    public async Task<HashSet<int>> ObtenerIdsAsync(int usuarioId)
        => (await db.Favoritos
            .AsNoTracking()
            .Where(f => f.UsuarioId == usuarioId)
            .Select(f => f.ProductoId)
            .ToListAsync()).ToHashSet();
}
