using BlackCode.Web.Data;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

public class ProductoDetalleViewModel
{
    public Product? Product { get; init; }
    public IReadOnlyList<Product> RelatedProducts { get; init; } = [];
}

public class CatalogoViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public IReadOnlyList<string> Materials { get; init; } = [];
    public IReadOnlyList<string> Sizes { get; init; } = [];
    public IReadOnlyList<string> SuggestionNames { get; init; } = [];
    public string? Search { get; init; }
    public string? Category { get; init; }
    public string? Material { get; init; }
    public string? Size { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal PriceCeiling { get; init; }
    public string Sort { get; init; } = "relevancia";
}

public class ColeccionViewModel
{
    public string Name { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public int ProductCount { get; init; }
}

public class ProductosController(BlackCodeContext context) : Controller
{
    [HttpGet("Productos/Sugerencias")]
    public async Task<IActionResult> Sugerencias(string? q)
    {
        var query = q?.Trim() ?? "";
        var products = await context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        var matches = products.Where(p => query.Length == 0
                || p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || p.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
                || p.Material.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .Select(p => new { p.Id, p.Name, p.Category, p.ImageUrl, p.Price });
        var categories = products.Select(p => p.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(c => query.Length == 0 || c.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Order().Take(4);
        return Json(new { products = matches, categories });
    }

    [HttpGet("Productos")]
    public async Task<IActionResult> Index(string? q, string? categoria, string? material,
        string? talla, decimal? precioMin, decimal? precioMax, string ordenar = "relevancia")
    {
        var allProducts = await context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        q = q?.Trim();
        categoria = categoria?.Trim();
        material = material?.Trim();
        talla = talla?.Trim();
        precioMin = precioMin is >= 0 ? precioMin : null;
        precioMax = precioMax is >= 0 ? precioMax : null;
        if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
            (precioMin, precioMax) = (precioMax, precioMin);
        var highestPrice = allProducts.Count == 0 ? 0 : allProducts.Max(p => p.Price);
        var priceCeiling = Math.Max(50m,
            Math.Ceiling(Math.Max(highestPrice, Math.Max(precioMin ?? 0, precioMax ?? 0)) / 50m) * 50m);

        IEnumerable<Product> filtered = allProducts;
        if (!string.IsNullOrWhiteSpace(q))
            filtered = filtered.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || p.Description.Contains(q, StringComparison.OrdinalIgnoreCase)
                || p.Category.Contains(q, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(categoria))
            filtered = filtered.Where(p => p.Category.Equals(categoria, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(material))
            filtered = filtered.Where(p => p.Material.Equals(material, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(talla))
            filtered = filtered.Where(p => p.Sizes.Contains(talla, StringComparer.OrdinalIgnoreCase));
        if (precioMin is >= 0)
            filtered = filtered.Where(p => p.Price >= precioMin);
        if (precioMax is >= 0)
            filtered = filtered.Where(p => p.Price <= precioMax);

        filtered = ordenar switch
        {
            "precio-asc" => filtered.OrderBy(p => p.Price).ThenBy(p => p.Name),
            "precio-desc" => filtered.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
            "nombre" => filtered.OrderBy(p => p.Name),
            _ => filtered.OrderBy(p => p.Id)
        };

        return View(new CatalogoViewModel
        {
            Products = filtered.ToList(),
            Categories = allProducts.Select(p => p.Category).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToList(),
            Materials = allProducts.Select(p => p.Material).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToList(),
            Sizes = allProducts.SelectMany(p => p.Sizes).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToList(),
            SuggestionNames = allProducts.Select(p => p.Name).Order().ToList(),
            Search = q,
            Category = categoria,
            Material = material,
            Size = talla,
            MinPrice = precioMin,
            MaxPrice = precioMax,
            PriceCeiling = priceCeiling,
            Sort = ordenar
        });
    }

    [HttpGet("Colecciones")]
    public async Task<IActionResult> Colecciones()
    {
        var products = await context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        var collections = products.GroupBy(p => p.Category)
            .Select(group => new ColeccionViewModel
            {
                Name = group.Key,
                ImageUrl = group.First().ImageUrl,
                ProductCount = group.Count()
            })
            .OrderBy(c => c.Name)
            .ToList();
        return View(collections);
    }

    [HttpGet("Productos/Detalle/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View(new ProductoDetalleViewModel());
        }

        var related = await context.Products.AsNoTracking()
            .Where(p => p.Id != id)
            .OrderByDescending(p => p.Category == product.Category)
            .ThenBy(p => p.Id).Take(4).ToListAsync();

        return View(new ProductoDetalleViewModel { Product = product, RelatedProducts = related });
    }
}
