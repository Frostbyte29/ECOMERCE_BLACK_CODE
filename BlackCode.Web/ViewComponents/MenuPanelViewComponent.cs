using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.ViewComponents;

/// <summary>
/// SRP: provee la lista estática de módulos del panel como datos, sin HTML.
/// OCP: agregar un módulo futuro = nueva entrada en la lista, sin tocar el layout.
/// </summary>
public sealed class MenuPanelViewComponent : ViewComponent
{
    private static readonly IReadOnlyList<ModuloPanel> Modulos =
    [
        new("Clientes", "Clientes", "Admin"),
    ];

    public IViewComponentResult Invoke() => View(Modulos);
}

public sealed record ModuloPanel(string Nombre, string Controlador, string Area);
