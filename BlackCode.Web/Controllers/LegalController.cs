using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

public class LegalController : Controller
{
    [HttpGet, Route("terminos")]
    public IActionResult Terminos() => View();

    [HttpGet, Route("privacidad")]
    public IActionResult Privacidad() => View();

    [HttpGet, Route("nosotros")]
    public IActionResult Nosotros() => View();

    [HttpGet, Route("faq")]
    public IActionResult Faq() => View();

    [HttpGet, Route("envios")]
    public IActionResult Envios() => View();

    [HttpGet, Route("guia-tallas")]
    public IActionResult GuiaTallas() => View();

    [HttpGet, Route("contacto")]
    public IActionResult Contacto() => View();
}
