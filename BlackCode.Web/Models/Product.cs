using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackCode.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 999999)]
    public decimal Price { get; set; }

    [Required, StringLength(50)]
    public string Category { get; set; } = "Anillos";

    [Required, StringLength(1200)]
    public string Description { get; set; } = "";

    [Required, StringLength(100)]
    public string Material { get; set; } = "Acero inoxidable 316L";

    [Required, StringLength(100)]
    public string Finish { get; set; } = "Plateado envejecido";

    [Required, StringLength(500)]
    public string ImageUrl { get; set; } = "";

    // Guardamos tallas como texto separado por comas para simplificar la primera versión.
    [Required, StringLength(100)]
    public string SizesCsv { get; set; } = "7,8,9,10,11";

    [NotMapped]
    public string[] Sizes => SizesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
