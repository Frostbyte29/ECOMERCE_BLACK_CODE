using BlackCode.Application.Features.Autenticacion;
using BlackCode.Web.Security;
using BlackCode.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

/// <summary>
/// Login, registro y cierre de sesión del cliente.
/// Controlador delgado: convierte ViewModel → DTO, delega en IAutenticacionService (DIP) y arma la cookie.
/// </summary>
public class CuentaController(IAutenticacionService autenticacion, TimeProvider reloj) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => User.Identity?.IsAuthenticated == true
            ? RedirigirLocal(returnUrl)
            : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await autenticacion.IniciarSesionAsync(
            new SolicitudInicioSesion(modelo.Correo, modelo.Contrasena), soloAdmin: false, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        await CookieSesion.IniciarAsync(HttpContext, resultado.Sesion!, modelo.MantenerSesion, reloj.GetUtcNow());

        // Pendiente (issue #7): fusionar aquí el carrito del invitado (cookie bc_cart) con el del usuario.
        return RedirigirLocal(modelo.ReturnUrl);
    }

    [HttpGet]
    public IActionResult Registro(string? returnUrl = null)
        => User.Identity?.IsAuthenticated == true
            ? RedirigirLocal(returnUrl)
            : View(new RegistroViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await autenticacion.RegistrarAsync(
            new SolicitudRegistro(modelo.NombreCompleto, modelo.Correo, modelo.Telefono, modelo.Contrasena, modelo.AceptoTerminos),
            ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        // Quien se registra queda con la sesión iniciada: la compra sigue sin pasar por otro login.
        await CookieSesion.IniciarAsync(HttpContext, resultado.Sesion!, persistente: false, reloj.GetUtcNow());

        // Pendiente (issue #7): igual que en Login, fusionar el carrito del invitado.
        return RedirigirLocal(modelo.ReturnUrl);
    }

    // POST + antiforgery (global): un <img src="/Cuenta/Logout"> en otra página no puede cerrar la sesión.
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccesoDenegado() => View();

    // Solo se redirige a URLs locales: evita el "open redirect" hacia sitios externos.
    private IActionResult RedirigirLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");
}
