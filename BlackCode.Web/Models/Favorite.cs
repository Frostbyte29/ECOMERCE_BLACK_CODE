using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.Models;

public class Favorite
{
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
