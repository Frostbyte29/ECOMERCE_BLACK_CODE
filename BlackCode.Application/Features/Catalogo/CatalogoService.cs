using BlackCode.Application.Abstractions;
using BlackCode.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Application.Features.Catalogo;
public sealed class CatalogoService(IAppDbContext db, IEnumerable<IFiltroProducto> filtros) : ICatalogoService
{
    public async Task<CatalogoDto> ConsultarAsync(FiltroCatalogo filtro)
    {
        var query = db.Productos.AsNoTracking().Where(p => p.Estado == EstadoProducto.Publicado);
        foreach (var criterio in filtros) query = criterio.Aplicar(query, filtro);
        query = filtro.Orden switch {
            "precio-asc" => query.OrderBy(p => p.Precio),
            "precio-desc" => query.OrderByDescending(p => p.Precio),
            "nombre" => query.OrderBy(p => p.Nombre),
            _ => query.OrderByDescending(p => p.Destacado).ThenBy(p => p.Id)
        };
        var productos = await query.Include(p => p.Categoria).Include(p => p.Color).Include(p => p.Imagenes).Include(p => p.Variantes).ToListAsync();
        return new(productos.Select(Mapear).ToList(),
            await db.Categorias.AsNoTracking().Where(c => c.Activo).OrderBy(c => c.Orden).Select(c => new OpcionDto(c.Id,c.Nombre)).ToListAsync(),
            await db.Colores.AsNoTracking().Select(c => new OpcionDto(c.Id,c.Nombre)).ToListAsync());
    }
    public async Task<ProductoDto?> ObtenerAsync(int id) {
        var p = await db.Productos.AsNoTracking().Include(p => p.Categoria).Include(p => p.Color).Include(p => p.Imagenes).Include(p => p.Variantes)
            .SingleOrDefaultAsync(p => p.Id == id && p.Estado == EstadoProducto.Publicado);
        return p is null ? null : Mapear(p);
    }
    private static ProductoDto Mapear(Producto p) => new(p.Id,p.Nombre,p.Slug,p.Descripcion,p.Precio,p.Categoria.Nombre,p.Color.Nombre,
        p.Imagenes.OrderBy(i => i.Orden).FirstOrDefault()?.Url ?? "/img/productos/anillo-profano.svg", p.Destacado,
        p.Variantes.OrderBy(v => v.Id).Select(v => new VarianteDto(v.Id,v.Talla,v.Stock,v.Activo)).ToList());
}
