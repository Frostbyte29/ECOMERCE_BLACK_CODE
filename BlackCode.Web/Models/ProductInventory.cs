using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.Models;

public class ProductInventory
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int? Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<StockMovement> Movements { get; set; } = [];
}

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public ProductInventory? Inventory { get; set; }
    public int? PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    [Required, StringLength(300)]
    public string Reason { get; set; } = "";
    [Required]
    public string ChangedByUserId { get; set; } = "";
    public ApplicationUser? ChangedByUser { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
