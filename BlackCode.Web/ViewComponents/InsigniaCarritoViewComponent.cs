using BlackCode.Application.Features.Carrito;
using BlackCode.Web.Security;
using Microsoft.AspNetCore.Mvc;
namespace BlackCode.Web.ViewComponents;
/// <summary>
/// SRP: consultar cuántas unidades hay en el carrito del visitante actual
/// es una responsabilidad de presentación separada del resto del header.
/// DIP: depende de ICarritoService, no de CarritoService.
/// </summary>
public sealed class InsigniaCarritoViewComponent(ICarritoService carrito, CarritoIdentidad identidad) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var total = await carrito.ContarUnidadesAsync(identidad.Obtener());
        return View(total);
    }
}
