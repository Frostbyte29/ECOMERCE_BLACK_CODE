using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackCode.Web.Models;

public enum OrderStatus
{
    [Display(Name = "Pendiente de pago")] PendientePago,
    [Display(Name = "Pagado")] Pagado,
    [Display(Name = "En preparación")] EnPreparacion,
    [Display(Name = "Enviado")] Enviado,
    [Display(Name = "Entregado")] Entregado,
    [Display(Name = "Cancelado")] Cancelado
}

public class Order
{
    public int Id { get; set; }

    // Código visible para el cliente, por ejemplo BC-20261019-4F7A.
    [Required, StringLength(30)]
    public string Code { get; set; } = "";

    [StringLength(32)]
    public string? CheckoutToken { get; set; }

    [Required]
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.PendientePago;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ShippingCost { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    // Datos de envío copiados al momento de la compra.
    [Required, StringLength(120)]
    public string ShippingName { get; set; } = "";

    [Required, StringLength(20)]
    public string ShippingPhone { get; set; } = "";

    [Required, StringLength(200)]
    public string ShippingAddress { get; set; } = "";

    [Required, StringLength(80)]
    public string ShippingDistrict { get; set; } = "";

    [Required, StringLength(80)]
    public string ShippingCity { get; set; } = "";

    [StringLength(200)]
    public string? ShippingReference { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(80)]
    public string? TrackingNumber { get; set; }

    public List<OrderItem> Items { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
    public List<OrderStatusHistory> History { get; set; } = [];

    [NotMapped]
    public int ItemCount => Items.Sum(i => i.Quantity);
}

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    // Se guarda una copia del nombre, imagen y precio por si el producto cambia después.
    [Required, StringLength(100)]
    public string ProductName { get; set; } = "";

    [Required, StringLength(500)]
    public string ImageUrl { get; set; } = "";

    [StringLength(20)]
    public string? Size { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    [NotMapped]
    public decimal LineTotal => UnitPrice * Quantity;
}

public class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [StringLength(256)]
    public string? ChangedBy { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }
}
