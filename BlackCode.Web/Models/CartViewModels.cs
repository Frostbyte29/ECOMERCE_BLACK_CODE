using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlackCode.Web.Models;

// Lo que se guarda en la sesión: solo referencias, nunca precios.
public class CartItem
{
    public int ProductId { get; set; }
    public string? Size { get; set; }
    public int Quantity { get; set; }
}

public class CartLine
{
    public required Product Product { get; init; }
    public string? Size { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal => Product.Price * Quantity;
}

public class CartSummary
{
    public IReadOnlyList<CartLine> Lines { get; init; } = [];
    public decimal Subtotal => Lines.Sum(l => l.LineTotal);
    public decimal ShippingCost { get; init; }
    public decimal Total => Subtotal + ShippingCost;
    public int ItemCount => Lines.Sum(l => l.Quantity);
    public bool IsEmpty => Lines.Count == 0;
}

public class CheckoutViewModel
{
    [BindNever, ValidateNever]
    public CartSummary Cart { get; set; } = new();

    [BindNever, ValidateNever]
    public List<SavedAddress> SavedAddresses { get; set; } = [];

    public int? SelectedSavedAddressId { get; set; }
    public bool SaveNewAddress { get; set; }

    [StringLength(40, ErrorMessage = "Usa un nombre de máximo 40 caracteres.")]
    public string? NewAddressLabel { get; set; }

    [Required(ErrorMessage = "Ingresa el nombre de quien recibe.")]
    [StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string ShippingName { get; set; } = "";

    [Required(ErrorMessage = "Ingresa un teléfono de contacto.")]
    [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Ingresa un teléfono válido (solo números).")]
    [Display(Name = "Teléfono")]
    public string ShippingPhone { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la dirección.")]
    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string ShippingAddress { get; set; } = "";

    [Required(ErrorMessage = "Ingresa el distrito.")]
    [StringLength(80)]
    [Display(Name = "Distrito")]
    public string ShippingDistrict { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la ciudad.")]
    [StringLength(80)]
    [Display(Name = "Ciudad / Departamento")]
    public string ShippingCity { get; set; } = "Lima";

    [StringLength(200)]
    [Display(Name = "Referencia")]
    public string? ShippingReference { get; set; }

    [StringLength(500)]
    [Display(Name = "Notas del pedido")]
    public string? Notes { get; set; }
}

public class PaymentViewModel
{
    [BindNever, ValidateNever]
    public Order? Order { get; set; }

    [Required]
    public PaymentMethod Method { get; set; } = PaymentMethod.Tarjeta;

    // Campos de tarjeta (simulados). No se guardan salvo los últimos 4 dígitos.
    [Display(Name = "Número de tarjeta")]
    public string? CardNumber { get; set; }

    [Display(Name = "Nombre en la tarjeta")]
    public string? CardHolder { get; set; }

    [Display(Name = "Vencimiento (MM/AA)")]
    public string? CardExpiry { get; set; }

    [Display(Name = "CVV")]
    public string? CardCvv { get; set; }

    // Yape / Plin (simulado): número de operación que aparece en la app.
    [Display(Name = "Número de operación")]
    public string? OperationCode { get; set; }
}
