using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Catalogo;
using BlackCode.Application.Features.Favoritos;
using BlackCode.Application.Features.Resenas;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Mvc;
namespace BlackCode.Web.Controllers;
public sealed class CatalogoController(ICatalogoService catalogo, IFavoritosService favoritos, IResenaService resenas, IUsuarioActual usuario) : Controller {
    public async Task<IActionResult> Index(CatalogoViewModel model) {
        var dto = await catalogo.ConsultarAsync(new(model.Buscar,model.Categoria,model.Color,model.Material,model.Minimo,model.Maximo,model.Talla,model.Orden));
        HashSet<int> favIds = usuario.UsuarioId.HasValue && !usuario.EsAdmin
            ? await favoritos.ObtenerIdsAsync(usuario.UsuarioId.Value)
            : [];
        model.Productos = dto.Productos.Select(p => TiendaMapper.Producto(p, favIds.Contains(p.Id))).ToList();
        model.Categorias = dto.Categorias.Select(x => new OpcionViewModel(x.Id,x.Nombre)).ToList();
        model.Colores = dto.Colores.Select(x => new OpcionViewModel(x.Id,x.Nombre)).ToList();
        model.Materiales = dto.Materiales.Select(x => new OpcionViewModel(x.Id,x.Nombre)).ToList();
        return View(model);
    }
    public async Task<IActionResult> Colecciones() {
        var dto = await catalogo.ConsultarAsync(new());
        return View(dto.Categorias.Select(c => new OpcionViewModel(c.Id, c.Nombre)).ToList());
    }
    public async Task<IActionResult> Detalle(int id) {
        var p = await catalogo.ObtenerAsync(id);
        if (p is null) return NotFound();
        bool esFav = usuario.UsuarioId.HasValue && !usuario.EsAdmin && await favoritos.EsFavoritoAsync(usuario.UsuarioId.Value, p.Id);
        var resenasList = await resenas.ListarPorProductoAsync(p.Id);
        bool yaReseno = usuario.UsuarioId.HasValue && !usuario.EsAdmin && await resenas.YaResenoAsync(usuario.UsuarioId.Value, p.Id);
        ViewBag.Resenas = resenasList.Select(TiendaMapper.Resena).ToList();
        ViewBag.YaReseno = yaReseno;
        return View(TiendaMapper.Producto(p, esFav));
    }
}
