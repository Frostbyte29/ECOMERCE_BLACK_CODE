using BlackCode.Application.Features.Carrito;
using BlackCode.Domain.Common;
using BlackCode.Web.Models;
using BlackCode.Web.Security;
using Microsoft.AspNetCore.Mvc;
namespace BlackCode.Web.Controllers;
public sealed class CarritoController(ICarritoService carrito,CarritoIdentidad identidad) : Controller {
    public async Task<IActionResult> Index() => View(TiendaMapper.Carrito(await carrito.ObtenerAsync(identidad.Obtener())));
    [HttpPost] public async Task<IActionResult> Agregar(int varianteId,int cantidad=1) {
        try { await carrito.AgregarAsync(identidad.Obtener(),varianteId,cantidad); TempData["Exito"]="Producto añadido a tu carrito."; }
        catch (DomainException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Actualizar(int varianteId,int cantidad) {
        try { await carrito.ActualizarAsync(identidad.Obtener(),varianteId,cantidad); TempData["Exito"]="Carrito actualizado."; }
        catch (DomainException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
