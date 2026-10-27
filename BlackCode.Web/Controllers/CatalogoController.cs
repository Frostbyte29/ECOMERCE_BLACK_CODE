using BlackCode.Application.Features.Catalogo;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Mvc;
namespace BlackCode.Web.Controllers;
public sealed class CatalogoController(ICatalogoService catalogo) : Controller {
    public async Task<IActionResult> Index(CatalogoViewModel model) {
        var dto = await catalogo.ConsultarAsync(new(model.Buscar,model.Categoria,model.Color,model.Minimo,model.Maximo,model.Talla,model.Orden));
        model.Productos = dto.Productos.Select(TiendaMapper.Producto).ToList();
        model.Categorias = dto.Categorias.Select(x => new OpcionViewModel(x.Id,x.Nombre)).ToList();
        model.Colores = dto.Colores.Select(x => new OpcionViewModel(x.Id,x.Nombre)).ToList();
        return View(model);
    }
    public async Task<IActionResult> Detalle(int id) {
        var p = await catalogo.ObtenerAsync(id); return p is null ? NotFound() : View(TiendaMapper.Producto(p));
    }
}
