using BlackCode.Application.Features.Catalogo;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlackCode.Web.Controllers;
public sealed class HomeController(ICatalogoService catalogo) : Controller {
    public async Task<IActionResult> Index() {
        var datos=await catalogo.ConsultarAsync(new());
        return View(new CatalogoViewModel { Productos=datos.Productos.Select(TiendaMapper.Producto).ToList(),Categorias=datos.Categorias.Select(c => new OpcionViewModel(c.Id,c.Nombre)).ToList() });
    }
    [ResponseCache(Duration=0,Location=ResponseCacheLocation.None,NoStore=true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId=Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
