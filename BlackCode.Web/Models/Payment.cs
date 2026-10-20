using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackCode.Web.Models;

public enum PaymentMethod
{
    [Display(Name = "Tarjeta de crédito/débito")] Tarjeta,
    [Display(Name = "Yape / Plin")] YapePlin,
    [Display(Name = "Pago contra entrega")] ContraEntrega
}

public enum PaymentStatus
{
    [Display(Name = "Pendiente")] Pendiente,
    [Display(Name = "Aprobado (demo)")] Aprobado,
    [Display(Name = "Rechazado (demo)")] Rechazado,
    [Display(Name = "Reembolso simulado")] Reembolsado
}

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pendiente;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    // Identificador que devolvería la pasarela. En la simulación se genera localmente.
    [StringLength(60)]
    public string? TransactionId { get; set; }

    // Nunca se guarda el número completo de la tarjeta, solo los últimos 4 dígitos.
    [StringLength(4)]
    public string? CardLast4 { get; set; }

    [StringLength(200)]
    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
