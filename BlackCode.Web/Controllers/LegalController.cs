using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

public class LegalController : Controller
{
    [HttpGet, Route("terminos")]
    public IActionResult Terminos() => View();

    [HttpGet, Route("privacidad")]
    public IActionResult Privacidad() => View();
}
