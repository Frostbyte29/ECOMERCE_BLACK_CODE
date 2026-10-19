using BlackCode.Application.Features.Autenticacion;
using BlackCode.Domain.Usuarios;
using BlackCode.Web.Security;
using BlackCode.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Areas.Admin.Controllers;

/// <summary>
/// Login de administrador en /Admin/Login. Usa el MISMO IAutenticacionService y la MISMA cookie que el cliente;
/// lo único distinto es soloAdmin: true y que la sesión nunca es persistente.
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
            new SolicitudInicioSesion(modelo.Correo, modelo.Contrasena), soloAdmin: true, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        await CookieSesion.IniciarAsync(HttpContext, resultado.Sesion!, persistente: false, reloj.GetUtcNow());
        return RedirigirLocal(modelo.ReturnUrl);
    }

    // Destino por defecto del panel: Usuarios, hasta que exista el dashboard.
    private IActionResult RedirigirLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl!)
            : RedirectToAction("Index", "Usuarios", new { area = "Admin" });
}
