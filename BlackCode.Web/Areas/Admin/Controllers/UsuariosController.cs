using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Usuarios;
using BlackCode.Domain.Usuarios;
using BlackCode.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin → Usuarios: listar con filtros, activar o desactivar y desbloquear.
/// Controlador delgado: las reglas (no desactivarse a sí mismo, desbloqueo) viven en IGestionUsuariosService.
/// </summary>
[Area("Admin")]
[Authorize(Roles = nameof(RolUsuario.Admin))]
public class UsuariosController(IGestionUsuariosService usuarios, IUsuarioActual usuarioActual) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q, RolUsuario? rol, bool? activo, int pagina = 1, CancellationToken ct = default)
    {
        var resultado = await usuarios.ListarAsync(new FiltroUsuarios(q, rol, activo, pagina), ct);

        return View(new UsuariosListaViewModel
        {
            Items = resultado.Items.Select(u => new UsuarioFilaViewModel
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                Correo = u.Correo,
                Telefono = string.IsNullOrWhiteSpace(u.Telefono) ? "—" : u.Telefono,
                Rol = u.Rol,
                Activo = u.Activo,
                EstaBloqueado = u.EstaBloqueado,
                EsUsuarioActual = u.Id == usuarioActual.UsuarioId,
                FechaRegistro = u.CreadoUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            }).ToList(),
            Busqueda = q,
            Rol = rol,
            Activo = activo,
            Pagina = resultado.Pagina,
            TotalPaginas = resultado.TotalPaginas,
            Total = resultado.Total
        });
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activo, string? returnUrl, CancellationToken ct)
    {
        var resultado = await usuarios.CambiarEstadoAsync(id, activo, ct);
        Notificar(resultado.Exitoso, resultado.Error, activo ? "Usuario activado." : "Usuario desactivado.");
        return VolverALista(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Desbloquear(int id, string? returnUrl, CancellationToken ct)
    {
        var resultado = await usuarios.DesbloquearAsync(id, ct);
        Notificar(resultado.Exitoso, resultado.Error, "Usuario desbloqueado.");
        return VolverALista(returnUrl);
    }

    private void Notificar(bool exitoso, string? error, string mensajeOk)
    {
        if (exitoso) TempData["Ok"] = mensajeOk;
        else TempData["Error"] = error;
    }

    // POST-Redirect-GET: vuelve a la lista con los mismos filtros; si refrescan, no se repite la acción.
    private IActionResult VolverALista(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
}
