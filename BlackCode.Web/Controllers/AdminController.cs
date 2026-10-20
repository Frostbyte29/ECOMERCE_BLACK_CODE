using BlackCode.Web.Data;
using BlackCode.Web.Models;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("Admin")]
public class AdminController(BlackCodeContext context, UserManager<ApplicationUser> userManager) : Controller
{
    private static readonly OrderStatus[] SoldStatuses =
        [OrderStatus.Pagado, OrderStatus.EnPreparacion, OrderStatus.Enviado, OrderStatus.Entregado];

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var firstMonth = new DateTime(PeruTime.Today.Year, PeruTime.Today.Month, 1).AddMonths(-11);
        var sales = await context.Orders.AsNoTracking()
            .Where(o => SoldStatuses.Contains(o.Status) && o.CreatedAt >= PeruTime.ToUtc(firstMonth))
            .Select(o => new { o.CreatedAt, o.Total }).ToListAsync();
        var monthly = Enumerable.Range(0, 12).Select(index =>
        {
            var start = firstMonth.AddMonths(index);
            var end = start.AddMonths(1);
            var amount = sales.Where(o => PeruTime.FromUtc(o.CreatedAt) >= start && PeruTime.FromUtc(o.CreatedAt) < end).Sum(o => o.Total);
            return new MonthlySalesViewModel(start.ToString("MMM yy", new System.Globalization.CultureInfo("es-PE")), amount);
        }).ToList();
        var inventories = await context.ProductInventories.AsNoTracking().ToListAsync();
        return View(new AdminDashboardViewModel
        {
            RevenueMonth = monthly.Last().Amount,
            OrderCount = await context.Orders.CountAsync(),
            UserCount = await context.Users.CountAsync(),
            ProductCount = await context.Products.CountAsync(),
            UndefinedStockCount = await context.Products.CountAsync() - inventories.Count(i => i.Quantity.HasValue),
            LowStockCount = inventories.Count(i => i.Quantity.HasValue && i.MinimumQuantity.HasValue && i.Quantity <= i.MinimumQuantity),
            RecentOrders = await context.Orders.AsNoTracking().Include(o => o.Items).OrderByDescending(o => o.CreatedAt).Take(5).ToListAsync(),
            MonthlySales = monthly
        });
    }

    [HttpGet("Inventario")]
    public async Task<IActionResult> Inventario(string? search)
    {
        var query = context.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(p => p.Name.Contains(value) || p.Category.Contains(value));
        }
        var products = await query.OrderBy(p => p.Name).ToListAsync();
        var ids = products.Select(p => p.Id).ToList();
        var inventories = await context.ProductInventories.AsNoTracking().Where(i => ids.Contains(i.ProductId)).ToDictionaryAsync(i => i.ProductId);
        var movements = await context.StockMovements.AsNoTracking().Include(m => m.Inventory).ThenInclude(i => i!.Product)
            .OrderByDescending(m => m.ChangedAt).Take(8).ToListAsync();
        return View(new AdminInventoryViewModel { Products = products, InventoryByProduct = inventories, RecentMovements = movements, Search = search });
    }

    [HttpGet("Inventario/{id:int}")]
    public async Task<IActionResult> EditarStock(int id)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        var inventory = await context.ProductInventories.AsNoTracking().FirstOrDefaultAsync(i => i.ProductId == id);
        return View(new StockEditViewModel
        {
            ProductId = id, ProductName = product.Name, PreviousQuantity = inventory?.Quantity,
            Quantity = inventory?.Quantity, MinimumQuantity = inventory?.MinimumQuantity
        });
    }

    [HttpPost("Inventario/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarStock(int id, StockEditViewModel model)
    {
        if (id != model.ProductId) return BadRequest();
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        model.ProductName = product.Name;
        model.PreviousQuantity = await context.ProductInventories.AsNoTracking()
            .Where(i => i.ProductId == id).Select(i => i.Quantity).FirstOrDefaultAsync();
        if (!ModelState.IsValid) return View(model);

        var inventory = await context.ProductInventories.FirstOrDefaultAsync(i => i.ProductId == id);
        if (inventory is null)
        {
            inventory = new ProductInventory { ProductId = id };
            context.ProductInventories.Add(inventory);
        }
        var previous = inventory.Quantity;
        inventory.Quantity = model.Quantity;
        inventory.MinimumQuantity = model.MinimumQuantity;
        inventory.UpdatedAt = DateTime.UtcNow;
        if (previous != model.Quantity)
        {
            context.StockMovements.Add(new StockMovement
            {
                ProductId = id, PreviousQuantity = previous, NewQuantity = model.Quantity!.Value,
                Reason = model.Reason.Trim(), ChangedByUserId = userManager.GetUserId(User)!
            });
        }
        await context.SaveChangesAsync();
        TempData["Message"] = "Stock guardado. Las compras siguen abiertas mientras se termina de configurar el control de inventario.";
        return RedirectToAction(nameof(Inventario));
    }
}
