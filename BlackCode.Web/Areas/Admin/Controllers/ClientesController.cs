using BlackCode.Application.Features.Clientes;
using BlackCode.Domain.Usuarios;
using BlackCode.Web.Common;
using BlackCode.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin → Clientes: lista con filtros y ordenamiento, activar o desactivar.
/// Controlador delgado: las reglas viven en IGestionClientesService (DIP).
/// </summary>
[Area("Admin")]
[Authorize(Roles = nameof(RolUsuario.Admin))]
public class ClientesController(
    IGestionClientesService clientes,
    TimeProvider reloj) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? q, bool? activo, string? orden, string? dir,
        int pagina = 1, CancellationToken ct = default)
    {
        var resultado = await clientes.ListarAsync(
            new FiltroClientes(q, activo, pagina, Orden: orden, Dir: dir), ct);

        return View(new ClientesListaViewModel
        {
            Items = resultado.Items.Select(c => new ClienteFilaViewModel
            {
                Id = c.Id,
                NombreCompleto = c.NombreCompleto,
                Correo = c.Correo,
                Activo = c.Activo,
                UltimoAccesoRelativo = FechaRelativa.Formatear(c.UltimoAccesoUtc, reloj.GetUtcNow()),
            }).ToList(),
            Busqueda = q,
            Activo = activo,
            Pagina = resultado.Pagina,
            TotalPaginas = resultado.TotalPaginas,
            Total = resultado.Total,
            Orden = orden,
            Dir = dir,
        });
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activo, string? returnUrl, CancellationToken ct)
    {
        var resultado = await clientes.CambiarEstadoAsync(id, activo, ct);
        Notificar(resultado.Exitoso, resultado.Error, activo ? "Cliente activado." : "Cliente desactivado.");
        return VolverALista(returnUrl);
    }

    private void Notificar(bool exitoso, string? error, string mensajeOk)
    {
        if (exitoso) TempData["Ok"] = mensajeOk;
        else TempData["Error"] = error;
    }

    // POST-Redirect-GET: vuelve a la lista con los mismos filtros; si recargan no se repite la acción.
    private IActionResult VolverALista(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
}
