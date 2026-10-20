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
        var addresses = await context.SavedAddresses.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenBy(a => a.CreatedAt).ToListAsync();
        var saved = addresses.FirstOrDefault();
        // Reutiliza la última dirección usada para ahorrar tiempo al cliente.
        var last = await context.Orders.AsNoTracking()
            .Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();

        return View(new CheckoutViewModel
        {
            Cart = summary,
            SavedAddresses = addresses,
            SelectedSavedAddressId = saved?.Id,
            ShippingName = saved?.RecipientName ?? last?.ShippingName ?? user?.FullName ?? "",
            ShippingPhone = saved?.Phone ?? last?.ShippingPhone ?? user?.PhoneNumber ?? "",
            ShippingAddress = saved?.Street ?? last?.ShippingAddress ?? "",
            ShippingDistrict = saved?.District ?? last?.ShippingDistrict ?? "",
            ShippingCity = saved?.City ?? last?.ShippingCity ?? "Lima",
            ShippingReference = saved?.Reference ?? last?.ShippingReference
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var summary = await cart.GetSummaryAsync();
        if (summary.IsEmpty) return RedirectToAction("Index", "Carrito");

        var userId = userManager.GetUserId(User)!;
        model.SavedAddresses = await context.SavedAddresses.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenBy(a => a.CreatedAt).ToListAsync();
        model.Cart = summary;

        if (model.SelectedSavedAddressId.HasValue)
        {
            var selected = model.SavedAddresses.FirstOrDefault(a => a.Id == model.SelectedSavedAddressId);
            if (selected is null)
            {
                ModelState.AddModelError("", "Selecciona una dirección de tu cuenta o escribe una nueva.");
                return View(model);
            }
            model.ShippingName = selected.RecipientName;
            model.ShippingPhone = selected.Phone;
            model.ShippingAddress = selected.Street;
            model.ShippingDistrict = selected.District;
            model.ShippingCity = selected.City;
            model.ShippingReference = selected.Reference;
            model.SaveNewAddress = false;
            model.NewAddressLabel = null;
            ModelState.Clear();
            TryValidateModel(model);
        }
        else if (model.SaveNewAddress && string.IsNullOrWhiteSpace(model.NewAddressLabel))
        {
            ModelState.AddModelError(nameof(model.NewAddressLabel), "Ponle un nombre a la dirección que quieres guardar.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        if (model.SelectedSavedAddressId is null && model.SaveNewAddress)
        {
            context.SavedAddresses.Add(new SavedAddress
            {
                UserId = userId,
                Label = model.NewAddressLabel!.Trim(),
                RecipientName = model.ShippingName.Trim(),
                Phone = model.ShippingPhone.Trim(),
                Street = model.ShippingAddress.Trim(),
                District = model.ShippingDistrict.Trim(),
                City = model.ShippingCity.Trim(),
                Reference = string.IsNullOrWhiteSpace(model.ShippingReference) ? null : model.ShippingReference.Trim(),
                IsDefault = model.SavedAddresses.Count == 0
            });
        }
        var order = await orders.CreateOrderAsync(userId, model, summary);
        await transaction.CommitAsync();
        cart.Clear();
        return RedirectToAction(nameof(Pagar), new { id = order.Id });
    }

    // Paso 2: pago (se puede reintentar si fue rechazado).
    public async Task<IActionResult> Pagar(int id)
    {
        await orders.ExpirePendingOrdersAsync(orderId: id, userId: userManager.GetUserId(User));
        var order = await FindOwnOrderAsync(id);
        if (order is null) return NotFound();
        if (order.Status == OrderStatus.Cancelado) return RedirectToAction("Detalle", "Pedidos", new { id });
        if (order.Status != OrderStatus.PendientePago) return RedirectToAction(nameof(Confirmacion), new { id });
        return View(new PaymentViewModel { Order = order });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(int id, PaymentViewModel model)
    {
        await orders.ExpirePendingOrdersAsync(orderId: id, userId: userManager.GetUserId(User));
        var order = await FindOwnOrderAsync(id);
        if (order is null) return NotFound();
        if (order.Status == OrderStatus.Cancelado)
        {
            TempData["Error"] = "Este pedido fue cancelado y ya no admite pagos.";
            return RedirectToAction("Detalle", "Pedidos", new { id });
        }
        if (order.Status != OrderStatus.PendientePago)
            return RedirectToAction(nameof(Confirmacion), new { id });

        var result = await orders.ProcessPaymentAsync(order, model);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            // No se devuelven los datos sensibles de la tarjeta al formulario
            // (hay que quitarlos también del ModelState, que es de donde los lee la vista).
            model.CardNumber = model.CardCvv = null;
            ModelState.Remove(nameof(PaymentViewModel.CardNumber));
            ModelState.Remove(nameof(PaymentViewModel.CardCvv));
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
        if (order.Status == OrderStatus.Cancelado) return RedirectToAction("Detalle", "Pedidos", new { id });
        if (order.Status == OrderStatus.PendientePago) return RedirectToAction(nameof(Pagar), new { id });
        return View(order);
    }

    private Task<Order?> FindOwnOrderAsync(int id)
    {
        var userId = userManager.GetUserId(User);
        return orders.QueryWithDetails().FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
    }
}
