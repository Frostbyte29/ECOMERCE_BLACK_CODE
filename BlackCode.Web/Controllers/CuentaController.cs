using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Autenticacion;
using BlackCode.Application.Features.Cuenta;
using BlackCode.Web.Security;
using BlackCode.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

/// <summary>
/// Login, registro, cierre de sesión y gestión de la cuenta del cliente.
/// Controlador delgado: convierte ViewModel → DTO y delega en los servicios de Application (DIP).
/// </summary>
public class CuentaController(
    IAutenticacionService autenticacion,
    ICuentaService cuenta,
    IUsuarioActual usuarioActual,
    TimeProvider reloj) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => User.Identity?.IsAuthenticated == true
            ? RedirectToAction(nameof(Index))
            : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await autenticacion.IniciarSesionAsync(
            new SolicitudInicioSesion(modelo.Correo, modelo.Contrasena), PortalAcceso.Tienda, ct);
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
            ? RedirectToAction(nameof(Index))
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

    [Authorize]
    [HttpGet]
    public IActionResult Index()
    {
        if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
        return View();
    }

    [Authorize]
    [HttpGet]
    public IActionResult CambiarContrasena() => View(new CambiarContrasenaViewModel());

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await cuenta.CambiarContrasenaAsync(
            usuarioActual.UsuarioId!.Value, modelo.ContrasenaActual, modelo.NuevaContrasena, ct);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Ok"] = "Contraseña actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult AccesoDenegado() => View();

    // Solo se redirige a URLs locales: evita el "open redirect" hacia sitios externos.
    private IActionResult RedirigirLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");
}
