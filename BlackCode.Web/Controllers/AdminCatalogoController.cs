using System.Globalization;
using System.Text;
using BlackCode.Web.Data;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("Admin")]
public class AdminCatalogoController(BlackCodeContext context, IWebHostEnvironment environment) : Controller
{
    [HttpGet("Productos")]
    public async Task<IActionResult> Productos(string? search)
    {
        var query = context.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Category.Contains(term));
        }
        ViewBag.Search = search;
        return View(await query.OrderBy(p => p.Name).ToListAsync());
    }

    [HttpGet("Productos/Nuevo")]
    public IActionResult Nuevo() => View("EditarProducto", new AdminProductFormViewModel());

    [HttpPost("Productos/Nuevo"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Nuevo(AdminProductFormViewModel model)
    {
        ValidateProduct(model);
        if (!ModelState.IsValid) return View("EditarProducto", model);
        var product = new Product();
        CopyFields(model, product);
        context.Products.Add(product);
        await context.SaveChangesAsync();
        TempData["Message"] = "Producto creado.";
        return RedirectToAction(nameof(Productos));
    }

    [HttpGet("Productos/{id:int}/Editar")]
    public async Task<IActionResult> Editar(int id)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound() : View("EditarProducto", new AdminProductFormViewModel
        {
            Id = product.Id, Name = product.Name, Price = product.Price, Category = product.Category,
            Description = product.Description, Material = product.Material, Finish = product.Finish,
            SizesCsv = product.SizesCsv, ImageUrl = product.ImageUrl
        });
    }

    [HttpPost("Productos/{id:int}/Editar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, AdminProductFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        ValidateProduct(model);
        if (!ModelState.IsValid) return View("EditarProducto", model);
        CopyFields(model, product);
        await context.SaveChangesAsync();
        TempData["Message"] = "Producto actualizado.";
        return RedirectToAction(nameof(Productos));
    }

    [HttpGet("Clientes")]
    public async Task<IActionResult> Clientes()
    {
        var users = await context.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync();
        var orderCounts = await context.Orders.AsNoTracking().GroupBy(o => o.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.UserId, x => x.Count);
        var favoriteCounts = await context.Favorites.AsNoTracking().GroupBy(f => f.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.UserId, x => x.Count);
        var addressCounts = await context.SavedAddresses.AsNoTracking().GroupBy(a => a.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.UserId, x => x.Count);
        var rows = users.Select(u => new AdminCustomerRow(u.FullName, u.Email ?? "", orderCounts.GetValueOrDefault(u.Id),
            favoriteCounts.GetValueOrDefault(u.Id), addressCounts.GetValueOrDefault(u.Id))).ToList();
        return View(rows);
    }

    [HttpGet("Reportes")]
    public async Task<IActionResult> Reportes()
    {
        var definedStock = await context.ProductInventories.CountAsync(i => i.Quantity.HasValue);
        var products = await context.Products.CountAsync();
        return View(new AdminReportsViewModel
        {
            Products = products, Customers = await context.Users.CountAsync(), Orders = await context.Orders.CountAsync(),
            ProductsWithoutStock = products - definedStock
        });
    }

    [HttpGet("Reportes/Productos.csv")]
    public async Task<IActionResult> ProductosCsv()
    {
        var products = await context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        var stock = await context.ProductInventories.AsNoTracking().ToDictionaryAsync(i => i.ProductId);
        var csv = new StringBuilder("\uFEFFId,Nombre,Categoría,Precio,Material,Acabado,Tallas,Imagen,Cantidad,Mínimo\r\n");
        foreach (var p in products)
        {
            stock.TryGetValue(p.Id, out var inventory);
            var fields = new[] { p.Id.ToString(CultureInfo.InvariantCulture), p.Name, p.Category,
                p.Price.ToString("0.00", CultureInfo.InvariantCulture), p.Material, p.Finish, p.SizesCsv, p.ImageUrl,
                inventory?.Quantity?.ToString(CultureInfo.InvariantCulture) ?? "Sin definir",
                inventory?.MinimumQuantity?.ToString(CultureInfo.InvariantCulture) ?? "Sin definir" };
            csv.AppendJoin(',', fields.Select(CsvCell)).Append("\r\n");
        }
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", "black-code-productos.csv");
    }

    [HttpGet("Reportes/Productos.json")]
    public async Task<IActionResult> ProductosJson()
    {
        var products = await context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        var stock = await context.ProductInventories.AsNoTracking().ToDictionaryAsync(i => i.ProductId);
        var rows = products.Select(p => new
        {
            p.Id, p.Name, p.Category, p.Price, p.Description, p.Material, p.Finish, p.SizesCsv, p.ImageUrl,
            Quantity = stock.GetValueOrDefault(p.Id)?.Quantity,
            MinimumQuantity = stock.GetValueOrDefault(p.Id)?.MinimumQuantity
        });
        return File(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(rows)),
            "application/json; charset=utf-8", "black-code-productos.json");
    }

    private void ValidateProduct(AdminProductFormViewModel model)
    {
        model.Name = model.Name?.Trim() ?? "";
        model.Category = model.Category?.Trim() ?? "";
        model.Description = model.Description?.Trim() ?? "";
        model.Material = model.Material?.Trim() ?? "";
        model.Finish = model.Finish?.Trim() ?? "";
        model.SizesCsv = model.SizesCsv?.Trim() ?? "";
        model.ImageUrl = model.ImageUrl?.Trim() ?? "";
        if (model.Name.Length == 0) ModelState.AddModelError(nameof(model.Name), "Escribe el nombre.");
        if (model.Category.Length == 0) ModelState.AddModelError(nameof(model.Category), "Escribe la categoría.");
        if (model.Description.Length == 0) ModelState.AddModelError(nameof(model.Description), "Escribe la descripción.");
        if (model.Material.Length == 0) ModelState.AddModelError(nameof(model.Material), "Escribe el material.");
        if (model.Finish.Length == 0) ModelState.AddModelError(nameof(model.Finish), "Escribe el acabado.");
        if (model.Price <= 0 || model.Price > 999999)
            ModelState.AddModelError(nameof(model.Price), "El precio debe ser mayor que cero y no superar 999999.");
        if (model.SizesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length == 0)
            ModelState.AddModelError(nameof(model.SizesCsv), "Indica al menos una talla.");
        if (!model.ImageUrl.StartsWith("/images/products/", StringComparison.OrdinalIgnoreCase)
            || model.ImageUrl.Contains("..", StringComparison.Ordinal)
            || model.ImageUrl.Contains('\\')
            || !new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" }.Contains(Path.GetExtension(model.ImageUrl).ToLowerInvariant())
            || !System.IO.File.Exists(Path.Combine(environment.WebRootPath, model.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))))
            ModelState.AddModelError(nameof(model.ImageUrl), "Usa una imagen existente de /images/products/.");
    }

    private static void CopyFields(AdminProductFormViewModel source, Product target)
    {
        target.Name = source.Name;
        target.Price = source.Price;
        target.Category = source.Category;
        target.Description = source.Description;
        target.Material = source.Material;
        target.Finish = source.Finish;
        target.SizesCsv = source.SizesCsv;
        target.ImageUrl = source.ImageUrl;
    }

    private static string CsvCell(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
}
