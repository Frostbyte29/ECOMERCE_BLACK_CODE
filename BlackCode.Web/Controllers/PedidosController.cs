using BlackCode.Web.Data;
using BlackCode.Web.Models;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

// Historial de pedidos del cliente que inició sesión.
[Authorize]
public class PedidosController(
    OrderService orders,
    BlackCodeContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);
        var list = await context.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var order = await FindOwnOrderAsync(id);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var order = await FindOwnOrderAsync(id);
        if (order is null) return NotFound();

        var result = await orders.CancelByCustomerAsync(order);
        TempData[result.Success ? "Message" : "Error"] = result.Message;
        return RedirectToAction(nameof(Detalle), new { id });
    }

    private Task<Order?> FindOwnOrderAsync(int id)
    {
        var userId = userManager.GetUserId(User);
        return orders.QueryWithDetails().FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
    }
}
