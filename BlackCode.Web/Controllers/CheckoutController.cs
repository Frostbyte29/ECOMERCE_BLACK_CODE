using BlackCode.Web.Data;
using BlackCode.Web.Models;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

[Authorize]
public class CheckoutController(
    CartService cart,
    OrderService orders,
    BlackCodeContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    // Paso 1: datos de envío.
    public async Task<IActionResult> Index()
    {
        var summary = await cart.GetSummaryAsync();
        if (summary.IsEmpty) return RedirectToAction("Index", "Carrito");

        var userId = userManager.GetUserId(User)!;
        var user = await userManager.GetUserAsync(User);
        // Reutiliza la última dirección usada para ahorrar tiempo al cliente.
        var last = await context.Orders.AsNoTracking()
            .Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();

        return View(new CheckoutViewModel
        {
            Cart = summary,
            ShippingName = last?.ShippingName ?? user?.FullName ?? "",
            ShippingPhone = last?.ShippingPhone ?? user?.PhoneNumber ?? "",
            ShippingAddress = last?.ShippingAddress ?? "",
            ShippingDistrict = last?.ShippingDistrict ?? "",
            ShippingCity = last?.ShippingCity ?? "Lima",
            ShippingReference = last?.ShippingReference
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var summary = await cart.GetSummaryAsync();
        if (summary.IsEmpty) return RedirectToAction("Index", "Carrito");

        if (!ModelState.IsValid)
        {
            model.Cart = summary;
            return View(model);
        }

        var order = await orders.CreateOrderAsync(userManager.GetUserId(User)!, model, summary);
        cart.Clear();
        return RedirectToAction(nameof(Pagar), new { id = order.Id });
    }

    // Paso 2: pago (se puede reintentar si fue rechazado).
    public async Task<IActionResult> Pagar(int id)
    {
        var order = await FindOwnOrderAsync(id);
        if (order is null) return NotFound();
        if (order.Status != OrderStatus.PendientePago) return RedirectToAction(nameof(Confirmacion), new { id });
        return View(new PaymentViewModel { Order = order });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(int id, PaymentViewModel model)
    {
        var order = await FindOwnOrderAsync(id);
        if (order is null) return NotFound();

        var result = await orders.ProcessPaymentAsync(order, model);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            // No se devuelven los datos sensibles de la tarjeta al formulario.
            model.CardNumber = model.CardCvv = null;
            model.Order = order;
            return View(model);
        }

        return RedirectToAction(nameof(Confirmacion), new { id });
    }

    // Paso 3: confirmación.
    public async Task<IActionResult> Confirmacion(int id)
    {
        var order = await FindOwnOrderAsync(id);
        if (order is null) return NotFound();
        if (order.Status == OrderStatus.PendientePago) return RedirectToAction(nameof(Pagar), new { id });
        return View(order);
    }

    private Task<Order?> FindOwnOrderAsync(int id)
    {
        var userId = userManager.GetUserId(User);
        return orders.QueryWithDetails().FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
    }
}
