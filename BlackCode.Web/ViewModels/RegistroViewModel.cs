using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.ViewModels;

public class RegistroViewModel
{
    [Required(ErrorMessage = "Ingresa tu nombre completo.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 120 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu correo.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [StringLength(254)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    // Celular peruano: 9 dígitos que empiezan en 9.
    [Required(ErrorMessage = "Ingresa tu celular.")]
    [RegularExpression(@"^9\d{8}$", ErrorMessage = "Ingresa un celular de 9 dígitos que empiece en 9.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa una contraseña.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña.")]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    // Se valida en el servidor: el navegador no valida un Range sobre bool.
    [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar los términos y la política de privacidad.")]
    [Display(Name = "Acepto los Términos y Condiciones y la Política de Privacidad")]
    public bool AceptoTerminos { get; set; }

    public string? ReturnUrl { get; set; }
}
