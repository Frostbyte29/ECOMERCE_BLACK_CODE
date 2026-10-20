using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.Models;

public class AdminProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Escribe el nombre."), StringLength(100)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Indica el precio.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Escribe la categoría."), StringLength(50)]
    public string Category { get; set; } = "";

    [Required(ErrorMessage = "Escribe la descripción."), StringLength(1200)]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Escribe el material."), StringLength(100)]
    public string Material { get; set; } = "";

    [Required(ErrorMessage = "Escribe el acabado."), StringLength(100)]
    public string Finish { get; set; } = "";

    [Required(ErrorMessage = "Indica las tallas."), StringLength(100)]
    public string SizesCsv { get; set; } = "";

    [Required(ErrorMessage = "Elige una imagen."), StringLength(500)]
    public string ImageUrl { get; set; } = "";
}

public record AdminCustomerRow(string Name, string Email, int Orders, int Favorites, int Addresses);

public class AdminReportsViewModel
{
    public int Products { get; init; }
    public int Customers { get; init; }
    public int Orders { get; init; }
    public int ProductsWithoutStock { get; init; }
}
