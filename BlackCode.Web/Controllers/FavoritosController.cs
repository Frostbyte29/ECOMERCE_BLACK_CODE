using BlackCode.Web.Data;
using BlackCode.Web.Models;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

[Authorize]
public class FavoritosController(BlackCodeContext context, UserManager<ApplicationUser> userManager, FavoriteService favorites) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User)!;
        var products = await context.Favorites.AsNoTracking()
            .Where(f => f.UserId == userId && f.Product != null)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.Product!)
            .ToListAsync();
        return View(products);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cambiar(int productId)
    {
        var userId = userManager.GetUserId(User)!;
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null) return NotFound();

        var existing = await context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
        var added = existing is null;
        if (added) context.Favorites.Add(new Favorite { UserId = userId, ProductId = productId });
        else context.Favorites.Remove(existing!);
        await context.SaveChangesAsync();
        favorites.Forget();
        var count = await context.Favorites.CountAsync(f => f.UserId == userId);
        return Json(new { added, count, product = new { product.Id, product.Name, product.ImageUrl, product.Price } });
    }
}
