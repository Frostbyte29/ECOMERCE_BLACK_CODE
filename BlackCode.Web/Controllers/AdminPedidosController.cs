using BlackCode.Web.Data;
using BlackCode.Web.Models;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

public class AdminPedidosViewModel
{
    public IReadOnlyList<Order> Orders { get; init; } = [];
    public OrderStatus? Status { get; init; }
    public string? Search { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public Dictionary<OrderStatus, int> CountByStatus { get; init; } = [];
    public decimal RevenueToday { get; init; }
    public decimal RevenueMonth { get; init; }
}

// Panel de gestión de pedidos para administradores.
[Authorize(Roles = Roles.Admin)]
[Route("Admin/Pedidos")]
public class AdminPedidosController(OrderService orders, BlackCodeContext context) : Controller
{
    private const int PageSize = 15;

    // Estados que cuentan como venta realizada.
    private static readonly OrderStatus[] SoldStatuses =
        [OrderStatus.Pagado, OrderStatus.EnPreparacion, OrderStatus.Enviado, OrderStatus.Entregado];

    [HttpGet("")]
    public async Task<IActionResult> Index(OrderStatus? status, string? search, DateTime? from, DateTime? to, int page = 1)
    {
        await orders.ExpirePendingOrdersAsync();
        var query = context.Orders.AsNoTracking().Include(o => o.User).Include(o => o.Items).AsQueryable();

        if (status is not null) query = query.Where(o => o.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(o => o.Code.Contains(s) || o.ShippingName.Contains(s) || o.User!.Email!.Contains(s));
        }
        if (from is not null)
        {
            var fromUtc = PeruTime.ToUtc(from.Value.Date);
            query = query.Where(o => o.CreatedAt >= fromUtc);
        }
        if (to is not null)
        {
            var toUtc = PeruTime.ToUtc(to.Value.Date.AddDays(1));
            query = query.Where(o => o.CreatedAt < toUtc);
        }

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var list = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        var counts = await context.Orders.GroupBy(o => o.Status)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count);

        var todayStart = PeruTime.ToUtc(PeruTime.Today);
        var monthStart = PeruTime.ToUtc(new DateTime(PeruTime.Today.Year, PeruTime.Today.Month, 1));
        var sold = context.Orders.Where(o => SoldStatuses.Contains(o.Status));

        return View(new AdminPedidosViewModel
        {
            Orders = list,
            Status = status,
            Search = search,
            From = from,
            To = to,
            Page = page,
            TotalPages = totalPages,
            TotalCount = total,
            CountByStatus = counts,
            RevenueToday = await sold.Where(o => o.CreatedAt >= todayStart).SumAsync(o => (decimal?)o.Total) ?? 0,
            RevenueMonth = await sold.Where(o => o.CreatedAt >= monthStart).SumAsync(o => (decimal?)o.Total) ?? 0
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        await orders.ExpirePendingOrdersAsync(orderId: id);
        var order = await orders.QueryWithDetails().AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost("{id:int}/Estado"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, OrderStatus newStatus, string? note, string? trackingNumber)
    {
        var order = await orders.QueryWithDetails().FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        var result = await orders.ChangeStatusAsync(order, newStatus, User.Identity?.Name ?? "Admin", note, trackingNumber);
        TempData[result.Success ? "Message" : "Error"] = result.Message;
        return RedirectToAction(nameof(Detalle), new { id });
    }
}
