using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.ViewModels;

/// <summary>Formulario de inicio de sesión. Lo usan /Cuenta/Login y /Admin/Login.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu correo.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Display(Name = "Mantener mi sesión activa")]
    public bool MantenerSesion { get; set; }

    public string? ReturnUrl { get; set; }
}
