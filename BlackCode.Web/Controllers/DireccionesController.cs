using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Direcciones;
using BlackCode.Domain.Common;
using BlackCode.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Controllers;

[Authorize]
[Authorize(Policy = "NoEsAdministrador")]
public sealed class DireccionesController(IDireccionService direcciones, IUsuarioActual usuario) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var lista = await direcciones.ListarAsync(usuario.UsuarioId!.Value);
        return View(lista.Select(TiendaMapper.Direccion).ToList());
    }

    [HttpGet]
    public IActionResult Crear() => View(new DireccionFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Crear(DireccionFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        await direcciones.CrearAsync(usuario.UsuarioId!.Value, Mapear(modelo));
        TempData["Ok"] = "Dirección guardada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var dto = await direcciones.ObtenerAsync(id, usuario.UsuarioId!.Value);
        if (dto is null) return NotFound();
        return View(new DireccionFormViewModel
        {
            Id = dto.Id, NombreCompleto = dto.NombreCompleto, Telefono = dto.Telefono,
            Direccion = dto.Direccion, Distrito = dto.Distrito, Referencia = dto.Referencia,
            EsPredeterminada = dto.EsPredeterminada
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(DireccionFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        try { await direcciones.ActualizarAsync(modelo.Id, usuario.UsuarioId!.Value, Mapear(modelo)); }
        catch (DomainException ex) { ModelState.AddModelError("", ex.Message); return View(modelo); }
        TempData["Ok"] = "Dirección actualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        try { await direcciones.EliminarAsync(id, usuario.UsuarioId!.Value); }
        catch (DomainException) { /* ya no existe, ignorar */ }
        TempData["Ok"] = "Dirección eliminada.";
        return RedirectToAction(nameof(Index));
    }

    private static CrearDireccionDto Mapear(DireccionFormViewModel m) => new()
    {
        NombreCompleto = m.NombreCompleto, Telefono = m.Telefono, Direccion = m.Direccion,
        Distrito = m.Distrito, Referencia = m.Referencia, EsPredeterminada = m.EsPredeterminada
    };
}
