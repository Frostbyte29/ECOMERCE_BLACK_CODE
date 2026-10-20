using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.Models;

public class SavedAddress
{
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    [Required, StringLength(40)]
    public string Label { get; set; } = "Casa";
    [Required, StringLength(120)]
    public string RecipientName { get; set; } = "";
    [Required, StringLength(20)]
    public string Phone { get; set; } = "";
    [Required, StringLength(200)]
    public string Street { get; set; } = "";
    [Required, StringLength(80)]
    public string District { get; set; } = "";
    [Required, StringLength(80)]
    public string City { get; set; } = "";
    [StringLength(200)]
    public string? Reference { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
