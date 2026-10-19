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

public class ProductosController(BlackCodeContext context) : Controller
{
    [HttpGet("Productos/Detalle/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return View(new ProductoDetalleViewModel());

        var related = await context.Products.AsNoTracking()
            .Where(p => p.Id != id).OrderBy(p => p.Id).Take(4).ToListAsync();

        return View(new ProductoDetalleViewModel { Product = product, RelatedProducts = related });
    }
}
