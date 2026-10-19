using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlackCode.Web.ViewModels;

public class EditarClienteViewModel : IValidatableObject
{
    public int Id { get; init; }

    [BindNever]
    [ValidateNever]
    public ClientesListaViewModel? Lista { get; set; }

    // Solo display; ninguna de estas propiedades viaja en el POST.
    public string Correo               { get; set; } = string.Empty;
    public string UltimoAccesoRelativo { get; set; } = string.Empty;
    public string ClienteDesde         { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 120 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [RegularExpression(@"^9\d{8}$", ErrorMessage = "Ingresa un celular de 9 dígitos que empiece en 9.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    [StringLength(100)]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string? NuevaContrasena { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmarContrasena { get; set; }

    /// <summary>
    /// Validación cruzada: si se ingresa alguna contraseña, ambas son obligatorias,
    /// deben tener mínimo 8 caracteres y coincidir. Vacías = sin cambio.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var tieneNueva    = !string.IsNullOrEmpty(NuevaContrasena);
        var tieneConfirm  = !string.IsNullOrEmpty(ConfirmarContrasena);

        if (!tieneNueva && !tieneConfirm) yield break;

        if (!tieneNueva)
        {
            yield return new ValidationResult("Ingresa la nueva contraseña.", [nameof(NuevaContrasena)]);
        }
        else if (NuevaContrasena!.Length < 8)
        {
            yield return new ValidationResult(
                "La contraseña debe tener al menos 8 caracteres.", [nameof(NuevaContrasena)]);
        }

        if (!tieneConfirm)
        {
            yield return new ValidationResult("Confirma la nueva contraseña.", [nameof(ConfirmarContrasena)]);
        }
        else if (tieneNueva && NuevaContrasena != ConfirmarContrasena)
        {
            yield return new ValidationResult(
                "Las contraseñas no coinciden.", [nameof(ConfirmarContrasena)]);
        }
    }
}
