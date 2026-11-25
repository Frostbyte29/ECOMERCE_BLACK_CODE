using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Autenticacion;
using BlackCode.Application.Features.Carrito;
using BlackCode.Application.Features.Cuenta;
using BlackCode.Web.Models;
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
    TimeProvider reloj,
    ICarritoService carrito,
    CarritoIdentidad identidad) : Controller
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

        var tokenAnonimo = identidad.Obtener().TokenAnonimo;
        await CookieSesion.IniciarAsync(HttpContext, resultado.Sesion!, modelo.MantenerSesion, reloj.GetUtcNow());
        if (tokenAnonimo is not null)
            await carrito.FusionarAsync(tokenAnonimo, resultado.Sesion!.Id);

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

        var tokenAnonimo = identidad.Obtener().TokenAnonimo;
        // Quien se registra queda con la sesión iniciada: la compra sigue sin pasar por otro login.
        await CookieSesion.IniciarAsync(HttpContext, resultado.Sesion!, persistente: false, reloj.GetUtcNow());
        if (tokenAnonimo is not null)
            await carrito.FusionarAsync(tokenAnonimo, resultado.Sesion!.Id);

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
    [Authorize(Policy = "NoEsAdministrador")]
    [HttpGet]
    public IActionResult Index()
    {
        if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
        return View();
    }

    [Authorize]
    [Authorize(Policy = "NoEsAdministrador")]
    [HttpGet]
    public IActionResult CambiarContrasena() => View(new CambiarContrasenaViewModel());

    [Authorize]
    [Authorize(Policy = "NoEsAdministrador")]
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

    [Authorize]
    [Authorize(Policy = "NoEsAdministrador")]
    [HttpGet]
    public async Task<IActionResult> MisDatos()
    {
        var datos = await cuenta.ObtenerDatosAsync(usuarioActual.UsuarioId!.Value);
        return View(new MisDatosViewModel { NombreCompleto = datos?.NombreCompleto ?? "", Telefono = datos?.Telefono });
    }

    [Authorize]
    [Authorize(Policy = "NoEsAdministrador")]
    [HttpPost]
    public async Task<IActionResult> MisDatos(MisDatosViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(modelo);
        var resultado = await cuenta.ActualizarDatosAsync(usuarioActual.UsuarioId!.Value, modelo.NombreCompleto, modelo.Telefono, ct);
        if (!resultado.Exitoso) { ModelState.AddModelError("", resultado.Error!); return View(modelo); }
        TempData["Ok"] = "Datos actualizados correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult AccesoDenegado() => View();

    // Solo se redirige a URLs locales: evita el "open redirect" hacia sitios externos.
    private IActionResult RedirigirLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");
}
