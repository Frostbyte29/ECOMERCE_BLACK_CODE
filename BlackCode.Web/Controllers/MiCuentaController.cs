using BlackCode.Web.Data;
using BlackCode.Web.Models;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

[Authorize]
public class MiCuentaController(BlackCodeContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, OrderService orders) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User)!;
        await orders.ExpirePendingOrdersAsync(userId: userId);
        var user = await userManager.GetUserAsync(User);
        var model = new AccountDashboardViewModel
        {
            FullName = user?.FullName ?? "",
            OrderCount = await context.Orders.CountAsync(o => o.UserId == userId),
            FavoriteCount = await context.Favorites.CountAsync(f => f.UserId == userId),
            AddressCount = await context.SavedAddresses.CountAsync(a => a.UserId == userId),
            RecentOrders = await context.Orders.AsNoTracking().Include(o => o.Items)
                .Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).Take(3).ToListAsync()
        };
        return View(model);
    }

    public async Task<IActionResult> MisDatos()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        return View(new ProfileViewModel { FullName = user.FullName, Email = user.Email ?? "", PhoneNumber = user.PhoneNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MisDatos(ProfileViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        model.Email = user.Email ?? "";
        if (!ModelState.IsValid) return View(model);
        user.FullName = model.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }
        await signInManager.RefreshSignInAsync(user);
        TempData["Message"] = "Tus datos se guardaron.";
        return RedirectToAction(nameof(MisDatos));
    }

    public IActionResult CambiarContrasena() => View(new ChangePasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarContrasena(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "No se pudo cambiar la contraseña. Revisa la contraseña actual y los requisitos de la nueva.");
            return View(model);
        }
        await signInManager.RefreshSignInAsync(user);
        TempData["Message"] = "Tu contraseña se cambió.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Direcciones()
    {
        var userId = userManager.GetUserId(User)!;
        var addresses = await context.SavedAddresses.AsNoTracking()
            .Where(a => a.UserId == userId).OrderByDescending(a => a.IsDefault).ThenBy(a => a.CreatedAt).ToListAsync();
        return View(addresses);
    }

    public IActionResult NuevaDireccion() => View("EditarDireccion", new AddressViewModel());

    public async Task<IActionResult> EditarDireccion(int id)
    {
        var userId = userManager.GetUserId(User)!;
        var address = await context.SavedAddresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address is null) return NotFound();
        return View(new AddressViewModel
        {
            Id = address.Id, Label = address.Label, RecipientName = address.RecipientName,
            Phone = address.Phone, Street = address.Street, District = address.District,
            City = address.City, Reference = address.Reference, IsDefault = address.IsDefault
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarDireccion(AddressViewModel model)
    {
        if (!ModelState.IsValid) return View("EditarDireccion", model);
        var userId = userManager.GetUserId(User)!;
        SavedAddress address;
        if (model.Id.HasValue)
        {
            var existing = await context.SavedAddresses.FirstOrDefaultAsync(a => a.Id == model.Id.Value && a.UserId == userId);
            if (existing is null) return NotFound();
            address = existing;
        }
        else
        {
            address = new SavedAddress { UserId = userId };
            context.SavedAddresses.Add(address);
        }
        var hasDefault = await context.SavedAddresses.AnyAsync(a => a.UserId == userId && a.IsDefault);
        var makeDefault = model.IsDefault || !hasDefault;
        if (makeDefault)
        {
            var otherDefaults = await context.SavedAddresses.Where(a => a.UserId == userId && a.IsDefault && a.Id != address.Id).ToListAsync();
            foreach (var other in otherDefaults) other.IsDefault = false;
        }
        address.Label = model.Label.Trim();
        address.RecipientName = model.RecipientName.Trim();
        address.Phone = model.Phone.Trim();
        address.Street = model.Street.Trim();
        address.District = model.District.Trim();
        address.City = model.City.Trim();
        address.Reference = string.IsNullOrWhiteSpace(model.Reference) ? null : model.Reference.Trim();
        address.IsDefault = makeDefault || address.IsDefault;
        await context.SaveChangesAsync();
        TempData["Message"] = "Dirección guardada.";
        return RedirectToAction(nameof(Direcciones));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DireccionPrincipal(int id)
    {
        var userId = userManager.GetUserId(User)!;
        var address = await context.SavedAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address is null) return NotFound();
        var others = await context.SavedAddresses.Where(a => a.UserId == userId && a.IsDefault && a.Id != id).ToListAsync();
        foreach (var other in others) other.IsDefault = false;
        address.IsDefault = true;
        await context.SaveChangesAsync();
        TempData["Message"] = "Dirección principal actualizada.";
        return RedirectToAction(nameof(Direcciones));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarDireccion(int id)
    {
        var userId = userManager.GetUserId(User)!;
        var address = await context.SavedAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address is null) return NotFound();
        context.SavedAddresses.Remove(address);
        if (address.IsDefault)
        {
            var replacement = await context.SavedAddresses.Where(a => a.UserId == userId && a.Id != id)
                .OrderBy(a => a.CreatedAt).FirstOrDefaultAsync();
            if (replacement is not null) replacement.IsDefault = true;
        }
        await context.SaveChangesAsync();
        TempData["Message"] = "Dirección eliminada.";
        return RedirectToAction(nameof(Direcciones));
    }
}
