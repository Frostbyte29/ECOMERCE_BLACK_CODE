using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.Models;

public class AdminDashboardViewModel
{
    public decimal RevenueMonth { get; set; }
    public int OrderCount { get; set; }
    public int UserCount { get; set; }
    public int ProductCount { get; set; }
    public List<Order> RecentOrders { get; set; } = [];
    public List<MonthlySalesViewModel> MonthlySales { get; set; } = [];
    public int UndefinedStockCount { get; set; }
    public int LowStockCount { get; set; }
}

public record MonthlySalesViewModel(string Label, decimal Amount);

public class AdminInventoryViewModel
{
    public List<Product> Products { get; set; } = [];
    public Dictionary<int, ProductInventory> InventoryByProduct { get; set; } = [];
    public List<StockMovement> RecentMovements { get; set; } = [];
    public string? Search { get; set; }
}

public class StockEditViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int? PreviousQuantity { get; set; }

    [Required(ErrorMessage = "Indica la cantidad disponible."), Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa.")]
    [Display(Name = "Cantidad disponible")]
    public int? Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El mínimo no puede ser negativo.")]
    [Display(Name = "Avisar cuando queden (opcional)")]
    public int? MinimumQuantity { get; set; }

    [Required(ErrorMessage = "Explica el ajuste."), StringLength(300)]
    [Display(Name = "Motivo del ajuste")]
    public string Reason { get; set; } = "";
}
