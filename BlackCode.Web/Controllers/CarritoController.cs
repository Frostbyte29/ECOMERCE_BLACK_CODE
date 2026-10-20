using BlackCode.Web.Data;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

public class CarritoController(CartService cart, BlackCodeContext context) : Controller
{
    public async Task<IActionResult> Index() => View(await cart.GetSummaryAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int productId, string? size, int quantity = 1, bool buyNow = false)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null) return NotFound();

        // Si el producto tiene tallas, la talla elegida debe ser una de ellas.
        if (product.Sizes.Length > 0 && (size is null || !product.Sizes.Contains(size)))
            size = product.Sizes.Length == 1 ? product.Sizes[0] : null;
        if (product.Sizes.Length > 1 && size is null)
        {
            TempData["Error"] = "Selecciona una talla.";
            return RedirectToAction("Detalle", "Productos", new { id = productId });
        }

        cart.Add(productId, size, quantity);
        if (buyNow) return RedirectToAction("Index", "Checkout");

        TempData["Message"] = $"{product.Name} se agregó al carrito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Actualizar(int productId, string? size, int quantity)
    {
        cart.Update(productId, size, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Eliminar(int productId, string? size)
    {
        cart.Remove(productId, size);
        return RedirectToAction(nameof(Index));
    }
}
