using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.Models;

public record AdminCustomerRow(string Id, string Name, string Email, string? Phone, bool IsActive,
    int Orders, int Favorites, int Addresses);

public class AdminCustomersViewModel
{
    public IReadOnlyList<AdminCustomerRow> Customers { get; init; } = [];
    public string? Search { get; init; }
    public bool? Active { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }
    public int InactiveCount { get; init; }
}

public class AdminCustomerFormViewModel
{
    public string? Id { get; set; }
    [Required(ErrorMessage = "Escribe el correo."), EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Escribe el nombre."), StringLength(120)]
    public string Name { get; set; } = "";

    [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Ingresa un teléfono válido o deja el campo vacío.")]
    public string? Phone { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;
}
