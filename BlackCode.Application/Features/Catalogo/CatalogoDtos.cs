namespace BlackCode.Application.Features.Catalogo;
public record OpcionDto(int Id, string Nombre);
public record VarianteDto(int Id, string Talla, int Stock, bool Activo);
public record ProductoDto(int Id, string Nombre, string Slug, string Descripcion, decimal Precio,
    string Categoria, string Color, string Imagen, bool Destacado, IReadOnlyList<VarianteDto> Variantes);
public record CatalogoDto(IReadOnlyList<ProductoDto> Productos, IReadOnlyList<OpcionDto> Categorias, IReadOnlyList<OpcionDto> Colores);
public record FiltroCatalogo(string? Buscar = null, int? Categoria = null, int? Color = null,
    decimal? Minimo = null, decimal? Maximo = null, string? Talla = null, string Orden = "relevancia");
public interface ICatalogoService
{
    Task<CatalogoDto> ConsultarAsync(FiltroCatalogo filtro);
    Task<ProductoDto?> ObtenerAsync(int id);
}
