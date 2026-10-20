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

    [Required(ErrorMessage = "Ingresa una contraseña."), StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres."), DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

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

        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    public IActionResult Registro(string? returnUrl) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName.Trim() };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        await userManager.AddToRoleAsync(user, Roles.Cliente);
        await signInManager.SignInAsync(user, isPersistent: true);
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
