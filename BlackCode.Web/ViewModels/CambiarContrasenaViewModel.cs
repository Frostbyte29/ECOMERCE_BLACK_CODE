using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.ViewModels;

public class CambiarContrasenaViewModel
{
    [Required(ErrorMessage = "Ingresa tu contraseña actual.")]
    [Display(Name = "Contraseña actual")]
    public string ContrasenaActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa la nueva contraseña.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [Display(Name = "Nueva contraseña")]
    public string NuevaContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la nueva contraseña.")]
    [Compare(nameof(NuevaContrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
