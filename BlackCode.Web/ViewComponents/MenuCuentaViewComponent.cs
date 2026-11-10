using System.Security.Claims;
using BlackCode.Domain.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.ViewComponents;

/// <summary>
/// SRP: renderizar el estado de sesión del usuario en el header de la tienda.
/// Lee directamente los claims del principal; no inyecta servicios de datos.
/// </summary>
public class MenuCuentaViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
