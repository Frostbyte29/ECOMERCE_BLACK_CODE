using System.ComponentModel.DataAnnotations;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu correo."), EmailAddress(ErrorMessage = "Correo no válido.")]
    [Display(Name = "Correo")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu contraseña."), DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Display(Name = "Mantener sesión iniciada")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ingresa tu nombre."), StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu correo."), EmailAddress(ErrorMessage = "Correo no válido.")]
    [Display(Name = "Correo")]
    public string Email { get; set; } = "";

    [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Ingresa un teléfono válido o deja este campo vacío.")]
    [Display(Name = "Teléfono (opcional)")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Ingresa una contraseña."), StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres."), DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirma tu contraseña."), DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

// LOGIN MÍNIMO Y TEMPORAL para poder probar pedidos y pagos.
// El módulo de usuarios del equipo puede reemplazar este controlador manteniendo las rutas /Cuenta/Login y /Cuenta/Logout.
public class CuentaController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : Controller
{
    public IActionResult Login(string? returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(model.Email.Trim(), model.Password, isPersistent: model.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.IsLockedOut
                ? "Demasiados intentos. Espera unos minutos e inténtalo de nuevo."
                : "Correo o contraseña incorrectos.");
            return View(model);
        }
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    public IActionResult Registro(string? returnUrl) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim();
        var user = new ApplicationUser { UserName = email, Email = email, FullName = model.FullName.Trim(), PhoneNumber = model.PhoneNumber?.Trim() };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        await userManager.AddToRoleAsync(user, Roles.Cliente);
        await signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccesoDenegado() => View();
}
