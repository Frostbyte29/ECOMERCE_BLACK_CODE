using BlackCode.Web.Data;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("Admin/Clientes")]
public class AdminClientesController(BlackCodeContext context, UserManager<ApplicationUser> userManager) : Controller
{
    private const int PageSize = 15;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, bool? active, int page = 1)
    {
        var roleId = await context.Roles.Where(r => r.Name == Roles.Cliente).Select(r => r.Id).SingleAsync();
        var query = context.Users.AsNoTracking()
            .Where(u => context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId));

        var totalCount = await query.CountAsync();
        var activeCount = await query.CountAsync(u => u.IsActive);
        var inactiveCount = totalCount - activeCount;
        if (active.HasValue) query = query.Where(u => u.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.FullName.Contains(term) || (u.Email != null && u.Email.Contains(term)));
        }

        var count = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);
        var customers = await query.OrderBy(u => u.FullName).ThenBy(u => u.Id)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(u => new AdminCustomerRow(u.Id, u.FullName, u.Email ?? "", u.PhoneNumber, u.IsActive,
                context.Orders.Count(o => o.UserId == u.Id),
                context.Favorites.Count(f => f.UserId == u.Id),
                context.SavedAddresses.Count(a => a.UserId == u.Id)))
            .ToListAsync();

        return View(new AdminCustomersViewModel
        {
            Customers = customers, Search = search, Active = active, Page = page,
            TotalPages = totalPages, TotalCount = count, ActiveCount = activeCount, InactiveCount = inactiveCount
        });
    }

    [HttpGet("Nuevo")]
    public IActionResult Nuevo() => View(new AdminCustomerFormViewModel());

    [HttpPost("Nuevo"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Nuevo(AdminCustomerFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Escribe una contraseña para el cliente.");
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email, Email = email, FullName = model.Name.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            IsActive = true
        };
        var created = await userManager.CreateAsync(user, model.Password!);
        if (!created.Succeeded)
        {
            AddIdentityErrors(created);
            return View(model);
        }

        var roleAdded = await userManager.AddToRoleAsync(user, Roles.Cliente);
        if (!roleAdded.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddIdentityErrors(roleAdded);
            return View(model);
        }

        TempData["Message"] = "Cliente creado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}/Editar")]
    public async Task<IActionResult> Editar(string id)
    {
        var customer = await FindCustomerAsync(id);
        if (customer is null) return NotFound();
        return View(new AdminCustomerFormViewModel
        {
            Id = customer.Id, Email = customer.Email ?? "", Name = customer.FullName,
            Phone = customer.PhoneNumber, IsActive = customer.IsActive
        });
    }

    [HttpPost("{id}/Editar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(string id, AdminCustomerFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        var customer = await FindCustomerAsync(id);
        if (customer is null) return NotFound();

        model.Email = customer.Email ?? "";
        ModelState.Remove(nameof(model.Email));
        ModelState.Remove(nameof(model.Password));
        if (!ModelState.IsValid) return View(model);

        customer.FullName = model.Name.Trim();
        customer.PhoneNumber = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        if (customer.IsActive != model.IsActive)
        {
            customer.IsActive = model.IsActive;
            customer.SecurityStamp = Guid.NewGuid().ToString("N");
        }

        var updated = await userManager.UpdateAsync(customer);
        if (!updated.Succeeded)
        {
            AddIdentityErrors(updated);
            return View(model);
        }

        TempData["Message"] = model.IsActive ? "Cliente actualizado." : "Cliente desactivado. Sus sesiones se cerrarán al continuar navegando.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ApplicationUser?> FindCustomerAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        return user is not null && await userManager.IsInRoleAsync(user, Roles.Cliente) ? user : null;
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
    }
}
