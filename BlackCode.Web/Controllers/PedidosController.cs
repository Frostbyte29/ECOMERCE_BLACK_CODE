using BlackCode.Application.Features.Pedidos;
using BlackCode.Application.Features.Carrito;
using BlackCode.Application.Features.Direcciones;
using BlackCode.Application.Features.Seguimiento;
using BlackCode.Application.Abstractions;
using BlackCode.Domain.Common;
using BlackCode.Web.Models;
using BlackCode.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BlackCode.Web.Controllers;
[Authorize]
[Authorize(Policy = "NoEsAdministrador")]
public sealed class PedidosController(IPedidoService pedidos,ICarritoService carrito,CarritoIdentidad identidad,IPagoService pagos,INotificacionPedidoService notificaciones,IWebHostEnvironment env,IDireccionService direcciones,ISeguimientoService seguimiento,IUsuarioActual usuarioActual) : Controller {
    public async Task<IActionResult> Index() => View((await pedidos.ListarAsync()).Select(TiendaMapper.Pedido).ToList());
    public async Task<IActionResult> Detalle(int id) { var dto=await pedidos.ObtenerAsync(id); return dto is null ? NotFound() : View(TiendaMapper.Pedido(dto)); }
    [HttpGet] public async Task<IActionResult> Checkout() {
        var c=TiendaMapper.Carrito(await carrito.ObtenerAsync(identidad.Obtener()));
        if(c.Lineas.Count==0) return RedirectToAction("Index","Carrito");
        var ultimo=await pedidos.ObtenerUltimoEnvioAsync();
        var dirs = (await direcciones.ListarAsync(usuarioActual.UsuarioId!.Value)).Select(TiendaMapper.Direccion).ToList();
        return View(new CheckoutViewModel { Nombre=!string.IsNullOrEmpty(ultimo?.Nombre)?ultimo.Nombre:User.Identity?.Name??"",Telefono=ultimo?.Telefono??"",Direccion=ultimo?.Direccion??"",Distrito=ultimo?.Distrito??"",Referencia=ultimo?.Referencia,Carrito=c,Metodos=Metodos(),EsDesarrollo=env.IsDevelopment(),MostrarRechazo=env.IsDevelopment()&&Request.Query["pruebas"]=="1",DireccionesGuardadas=dirs });
    }
    [HttpPost] public async Task<IActionResult> Checkout(CheckoutViewModel model) {
        if(ModelState.IsValid) {
            try {
                var refManual = model.MetodoPago == "tarjeta" && !string.IsNullOrEmpty(model.TarjetaMarca)
                    ? $"{model.TarjetaMarca} *{model.TarjetaUltimos}"
                    : model.NumeroOperacion;
                var id=await pedidos.ConfirmarAsync(new(model.Nombre,model.Telefono,model.Direccion,model.Distrito,model.Referencia),model.SimularRechazo && env.IsDevelopment(),model.MetodoPago,refManual);
                var pedido=await pedidos.ObtenerAsync(id);
                var pagoInmediato = pedido!.PagoEstado == BlackCode.Domain.Pedidos.EstadoPago.Aprobado;
                TempData["Exito"]=notificaciones.Confirmacion(pedido.Codigo, pagoInmediato);
                return RedirectToAction(nameof(Detalle),new { id });
            }
            catch(DomainException ex) { ModelState.AddModelError("",ex.Message); }
        }
        model.Carrito=TiendaMapper.Carrito(await carrito.ObtenerAsync(identidad.Obtener()));
        model.Metodos=Metodos();
        model.EsDesarrollo=env.IsDevelopment();
        model.MostrarRechazo=env.IsDevelopment()&&Request.Query["pruebas"]=="1";
        model.DireccionesGuardadas=(await direcciones.ListarAsync(usuarioActual.UsuarioId!.Value)).Select(TiendaMapper.Direccion).ToList();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(int id) {
        try { await pedidos.CancelarPorClienteAsync(id); TempData["Ok"]="Tu pedido fue cancelado."; }
        catch(DomainException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Detalle),new{id});
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Seguimiento(string? codigo) {
        if(string.IsNullOrWhiteSpace(codigo)) return View((SeguimientoViewModel?)null);
        var dto=await seguimiento.ObtenerPorCodigoAsync(codigo.Trim());
        return View(dto is null ? null : TiendaMapper.Seguimiento(dto));
    }
    private static readonly IReadOnlyDictionary<string, string> ImagenesQr = new Dictionary<string, string>
    {
        ["yape"] = "img/pagos/yape-qr.png",
        ["plin"] = "img/pagos/plin-qr.png"
    };
    private IReadOnlyList<MetodoPagoViewModel> Metodos() => pagos.Metodos().Select(m =>
    {
        var imagen = ImagenesQr.GetValueOrDefault(m.Codigo);
        var disponible = imagen is not null && env.WebRootFileProvider.GetFileInfo(imagen).Exists;
        return new MetodoPagoViewModel(m.Codigo, m.Nombre, imagen is null ? null : "~/" + imagen, disponible);
    }).ToList();
}
