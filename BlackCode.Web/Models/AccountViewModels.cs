using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.Models;

public class AccountDashboardViewModel
{
    public string FullName { get; set; } = "";
    public int OrderCount { get; set; }
    public int FavoriteCount { get; set; }
    public int AddressCount { get; set; }
    public List<Order> RecentOrders { get; set; } = [];
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Ingresa tu nombre."), StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = "";

    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = "";

    [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Ingresa un teléfono válido o deja este campo vacío.")]
    [Display(Name = "Teléfono (opcional)")]
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Ingresa tu contraseña actual."), DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "Ingresa una contraseña nueva."), StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
    [Display(Name = "Contraseña nueva")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Confirma la contraseña nueva."), DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña nueva")]
    public string ConfirmPassword { get; set; } = "";
}

public class AddressViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Ponle un nombre a esta dirección."), StringLength(40)]
    [Display(Name = "Nombre de la dirección")]
    public string Label { get; set; } = "Casa";

    [Required(ErrorMessage = "Ingresa el nombre de quien recibe."), StringLength(120)]
    [Display(Name = "Nombre de quien recibe")]
    public string RecipientName { get; set; } = "";

    [Required(ErrorMessage = "Ingresa un teléfono."), RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Ingresa un teléfono válido.")]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la dirección."), StringLength(200)]
    [Display(Name = "Dirección")]
    public string Street { get; set; } = "";

    [Required(ErrorMessage = "Ingresa el distrito."), StringLength(80)]
    [Display(Name = "Distrito")]
    public string District { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la ciudad."), StringLength(80)]
    [Display(Name = "Ciudad / Departamento")]
    public string City { get; set; } = "";

    [StringLength(200)]
    [Display(Name = "Referencia (opcional)")]
    public string? Reference { get; set; }

    [Display(Name = "Usar como dirección principal")]
    public bool IsDefault { get; set; }
}
