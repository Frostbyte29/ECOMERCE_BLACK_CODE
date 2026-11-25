using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Favoritos;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

[Authorize]
[Authorize(Policy = "NoEsAdministrador")]
public sealed class FavoritosController(IFavoritosService favoritos, IUsuarioActual usuario) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var lista = await favoritos.ListarAsync(usuario.UsuarioId!.Value);
        return View(lista.Select(TiendaMapper.Favorito).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int productoId, string? returnUrl)
    {
        await favoritos.ToggleAsync(usuario.UsuarioId!.Value, productoId);
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }
}
