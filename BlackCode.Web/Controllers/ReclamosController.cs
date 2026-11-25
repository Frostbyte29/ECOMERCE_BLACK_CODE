using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Reclamos;
using BlackCode.Domain.Reclamos;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

// No requiere autenticación: cualquier consumidor puede registrar un reclamo.
public sealed class ReclamosController(IReclamosService reclamos, IUsuarioActual usuario) : Controller
{
    [HttpGet]
    public IActionResult Formulario() => View(new ReclamoFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Enviar(ReclamoFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View("Formulario", modelo);

        var dto = new ReclamoDto(
            modelo.NombresCompletos, modelo.Documento, modelo.Domicilio, modelo.Correo,
            modelo.Telefono, modelo.NumeroPedido, modelo.Tipo, modelo.DescripcionBien,
            modelo.Detalle, modelo.PedidoCliente);

        var id = await reclamos.RegistrarAsync(usuario.UsuarioId, dto);
        return View("Confirmacion", id);
    }
}
