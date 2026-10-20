using BlackCode.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Controllers;

public class HomeController(BlackCodeContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        return View(products);
    }

    [HttpGet("Nosotros")]
    public IActionResult Nosotros() => View();
}
