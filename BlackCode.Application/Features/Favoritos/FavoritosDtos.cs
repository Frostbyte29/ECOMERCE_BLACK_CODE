namespace BlackCode.Application.Features.Favoritos;

public record FavoritoDto(int ProductoId, string Nombre, string Slug, string Imagen, decimal Precio, string Categoria, bool HayStock);

public interface IFavoritosService
{
    Task ToggleAsync(int usuarioId, int productoId);
    Task<bool> EsFavoritoAsync(int usuarioId, int productoId);
    Task<IReadOnlyList<FavoritoDto>> ListarAsync(int usuarioId);
    Task<HashSet<int>> ObtenerIdsAsync(int usuarioId);
}
