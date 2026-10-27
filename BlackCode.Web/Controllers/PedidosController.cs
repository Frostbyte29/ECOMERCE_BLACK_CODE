using BlackCode.Application.Features.Pedidos;
using BlackCode.Application.Features.Carrito;
using BlackCode.Domain.Common;
using BlackCode.Web.Models;
using BlackCode.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BlackCode.Web.Controllers;
[Authorize]
public sealed class PedidosController(IPedidoService pedidos,ICarritoService carrito,CarritoIdentidad identidad,IPagoService pagos,INotificacionPedidoService notificaciones) : Controller {
    public async Task<IActionResult> Index() => View((await pedidos.ListarAsync()).Select(TiendaMapper.Pedido).ToList());
    public async Task<IActionResult> Detalle(int id) { var dto=await pedidos.ObtenerAsync(id); return dto is null ? NotFound() : View(TiendaMapper.Pedido(dto)); }
    [HttpGet] public async Task<IActionResult> Checkout() {
        var c=TiendaMapper.Carrito(await carrito.ObtenerAsync(identidad.Obtener()));
        if(c.Lineas.Count==0) return RedirectToAction("Index","Carrito");
        return View(new CheckoutViewModel { Nombre=User.Identity?.Name ?? "",Carrito=c,Metodos=Metodos() });
    }
    [HttpPost] public async Task<IActionResult> Checkout(CheckoutViewModel model) {
        if(ModelState.IsValid) {
            try { var id=await pedidos.ConfirmarAsync(new(model.Nombre,model.Telefono,model.Direccion,model.Distrito,model.Referencia),model.SimularRechazo,model.MetodoPago); var pedido=await pedidos.ObtenerAsync(id); TempData["Exito"]=notificaciones.Confirmacion(pedido!.Codigo); return RedirectToAction(nameof(Detalle),new { id }); }
            catch(DomainException ex) { ModelState.AddModelError("",ex.Message); }
        }
        model.Carrito=TiendaMapper.Carrito(await carrito.ObtenerAsync(identidad.Obtener()));
        model.Metodos=Metodos();
        return View(model);
    }
    private IReadOnlyList<MetodoPagoViewModel> Metodos() => pagos.Metodos().Select(m=>new MetodoPagoViewModel(m.Codigo,m.Nombre)).ToList();
}
