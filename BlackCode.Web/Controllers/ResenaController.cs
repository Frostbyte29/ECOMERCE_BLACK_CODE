using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Resenas;
using BlackCode.Domain.Common;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

[Authorize]
[Authorize(Policy = "NoEsAdministrador")]
public sealed class ResenaController(IResenaService resenas, IUsuarioActual usuario) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Crear(ResenaFormViewModel modelo, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            TempData["ResenaError"] = "Verifica la calificación y el comentario.";
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Catalogo");
        }
        try
        {
            await resenas.CrearAsync(usuario.UsuarioId!.Value, modelo.ProductoId, modelo.Calificacion, modelo.Comentario);
            TempData["ResenaOk"] = "Tu reseña fue publicada. ¡Gracias!";
        }
        catch (DomainException ex) { TempData["ResenaError"] = ex.Message; }
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Catalogo");
    }
}
