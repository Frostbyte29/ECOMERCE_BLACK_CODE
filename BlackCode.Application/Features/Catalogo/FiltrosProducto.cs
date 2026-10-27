using BlackCode.Domain.Catalogo;
namespace BlackCode.Application.Features.Catalogo;
// Cada criterio transforma la consulta; CatalogoService no conoce sus implementaciones.
public interface IFiltroProducto
{
    IQueryable<Producto> Aplicar(IQueryable<Producto> productos, FiltroCatalogo entrada);
}
public sealed class FiltroTexto : IFiltroProducto
{
    public IQueryable<Producto> Aplicar(IQueryable<Producto> productos, FiltroCatalogo entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada.Buscar)) return productos;
        var buscar = entrada.Buscar.Trim();
        return productos.Where(p => p.Nombre.Contains(buscar) || p.Descripcion.Contains(buscar));
    }
}
public sealed class FiltroCategoria : IFiltroProducto
{
    public IQueryable<Producto> Aplicar(IQueryable<Producto> productos, FiltroCatalogo entrada)
        => entrada.Categoria.HasValue ? productos.Where(p => p.CategoriaId == entrada.Categoria) : productos;
}
public sealed class FiltroColor : IFiltroProducto
{
    public IQueryable<Producto> Aplicar(IQueryable<Producto> productos, FiltroCatalogo entrada)
        => entrada.Color.HasValue ? productos.Where(p => p.ColorId == entrada.Color) : productos;
}
public sealed class FiltroPrecio : IFiltroProducto
{
    public IQueryable<Producto> Aplicar(IQueryable<Producto> productos, FiltroCatalogo entrada)
    {
        if (entrada.Minimo.HasValue) productos = productos.Where(p => p.Precio >= entrada.Minimo);
        if (entrada.Maximo.HasValue) productos = productos.Where(p => p.Precio <= entrada.Maximo);
        return productos;
    }
}
public sealed class FiltroTalla : IFiltroProducto
{
    public IQueryable<Producto> Aplicar(IQueryable<Producto> productos, FiltroCatalogo entrada)
        => string.IsNullOrWhiteSpace(entrada.Talla) ? productos :
            productos.Where(p => p.Variantes.Any(v => v.Talla == entrada.Talla && v.Activo && v.Stock > 0));
}
