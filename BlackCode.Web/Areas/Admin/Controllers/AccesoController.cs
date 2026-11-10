using BlackCode.Application.Features.Autenticacion;
using BlackCode.Domain.Usuarios;
using BlackCode.Web.Security;
using BlackCode.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Areas.Admin.Controllers;

/// <summary>
/// Login de administrador en /Admin/Login. Usa el MISMO IAutenticacionService y la MISMA cookie que el cliente;
/// lo único distinto es PortalAcceso.Panel: un Cliente con la clave correcta NO entra.
/// No lleva [Authorize]: es la puerta de entrada del panel.
/// </summary>
[Area("Admin")]
[Route("Admin/Login")]
public class AccesoController(IAutenticacionService autenticacion, TimeProvider reloj) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => User.IsInRole(nameof(RolUsuario.Admin))
            ? RedirigirLocal(returnUrl)
            : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await autenticacion.IniciarSesionAsync(
            new SolicitudInicioSesion(modelo.Correo, modelo.Contrasena), PortalAcceso.Panel, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        await CookieSesion.IniciarAsync(HttpContext, resultado.Sesion!, modelo.MantenerSesion, reloj.GetUtcNow());
        return RedirigirLocal(modelo.ReturnUrl);
    }

    private IActionResult RedirigirLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl!)
            : RedirectToAction("Index", "Clientes", new { area = "Admin" });
}
