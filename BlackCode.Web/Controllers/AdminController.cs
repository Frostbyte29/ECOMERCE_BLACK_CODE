using BlackCode.Application.Features.Administracion;
using BlackCode.Application.Features.Pedidos;
using BlackCode.Domain.Common;
using BlackCode.Domain.Pedidos;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BlackCode.Web.Controllers;
[Authorize(Roles="Admin")]
public sealed class AdminController(IAdministracionService admin,IPedidoService pedidos,IReporteService reportes) : Controller {
    public async Task<IActionResult> Index() { var d=await admin.ConsultarAsync(); return View(new AdminViewModel(d.Productos,d.Pedidos,d.Clientes,d.CarritosInvitados,d.Ventas,d.Lista.Select(TiendaMapper.Edicion).ToList())); }
    [HttpGet] public async Task<IActionResult> Producto(int id=0) {
        var datos=await admin.ConsultarAsync();
        var dto=datos.Lista.SingleOrDefault(p => p.Id==id);
        if(id!=0 && dto is null) return NotFound();
        var model=dto is null ? new ProductoEdicionViewModel() : TiendaMapper.Edicion(dto); Opciones(model,datos); return View(model);
    }
    [HttpPost] public async Task<IActionResult> Producto(ProductoEdicionViewModel model) {
        if(ModelState.IsValid) {
            try { await admin.GuardarAsync(new(model.Id,model.Nombre,model.Slug,model.Descripcion,model.Precio,model.CategoriaId,model.ColorId,model.MaterialId,model.Publicado,model.Destacado,model.Imagen,model.Variantes.Select(v => new VarianteEdicionDto(v.Id,v.Talla,v.Stock,v.Activo)).ToList())); TempData["Exito"]="Producto guardado."; return RedirectToAction(nameof(Index)); }
            catch(DomainException ex) { ModelState.AddModelError("",ex.Message); }
        }
        Opciones(model,await admin.ConsultarAsync()); return View(model);
    }
    [HttpPost] public async Task<IActionResult> Eliminar(int id) {
        try { await admin.EliminarAsync(id); TempData["Exito"]="Producto retirado del catálogo. Si tenía pedidos, se conservó su historial."; }
        catch(DomainException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Pedidos() => View((await pedidos.ListarAsync(true)).Select(TiendaMapper.Pedido).ToList());
    public async Task<IActionResult> Reporte(string formato = "csv") {
        try { var archivo=await reportes.ProductosAsync(formato); return File(archivo.Contenido,archivo.ContentType,archivo.NombreArchivo); }
        catch(DomainException ex) { TempData["Error"]=ex.Message; return RedirectToAction(nameof(Index)); }
    }
    [HttpPost] public async Task<IActionResult> Estado(int id,string estado) {
        try { if(!Enum.TryParse<EstadoPedido>(estado,out var valor)) throw new DomainException("Estado inválido."); await pedidos.CambiarEstadoAsync(id,valor); TempData["Exito"]="Estado actualizado."; }
        catch(DomainException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Pedidos));
    }
    private static void Opciones(ProductoEdicionViewModel m,AdministracionDto d) { m.Categorias=d.Categorias.Select(o => new OpcionViewModel(o.Id,o.Nombre)).ToList(); m.Colores=d.Colores.Select(o => new OpcionViewModel(o.Id,o.Nombre)).ToList(); m.Materiales=d.Materiales.Select(o => new OpcionViewModel(o.Id,o.Nombre)).ToList(); }
}
