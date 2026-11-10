using BlackCode.Application.Features.Clientes;
using BlackCode.Domain.Usuarios;
using BlackCode.Web.Common;
using BlackCode.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackCode.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin → Clientes: lista, nuevo, editar.
/// Controlador delgado: todas las reglas de negocio viven en IGestionClientesService (DIP).
/// </summary>
[Area("Admin")]
[Authorize(Roles = nameof(RolUsuario.Admin))]
public class ClientesController(
    IGestionClientesService clientes,
    TimeProvider reloj) : Controller
{
    // ── Lista ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(
        string? q, bool? activo, string? orden, string? dir,
        int pagina = 1, CancellationToken ct = default)
    {
        return View(await ConstruirListaAsync(q, activo, orden, dir, pagina, ct));
    }

    // ── Nuevo cliente ──────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Nuevo(CancellationToken ct)
    {
        return View(new NuevoClienteViewModel
        {
            Lista = await ConstruirListaAsync(null, null, null, null, 1, ct),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Nuevo(NuevoClienteViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            modelo.Lista = await ConstruirListaAsync(null, null, null, null, 1, ct);
            return View(modelo);
        }

        var resultado = await clientes.CrearAsync(
            new SolicitudCrearCliente(modelo.NombreCompleto, modelo.Correo, modelo.Telefono, modelo.Contrasena), ct);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            modelo.Lista = await ConstruirListaAsync(null, null, null, null, 1, ct);
            return View(modelo);
        }

        TempData["Ok"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── Editar cliente ─────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var cliente = await clientes.ObtenerAsync(id, ct);
        if (cliente is null) return NotFound();

        return View(new EditarClienteViewModel
        {
            Id                   = cliente.Id,
            Correo               = cliente.Correo,
            NombreCompleto       = cliente.NombreCompleto,
            Telefono             = cliente.Telefono,
            Activo               = cliente.Activo,
            UltimoAccesoRelativo = FechaRelativa.Formatear(cliente.UltimoAccesoUtc, reloj.GetUtcNow()),
            ClienteDesde         = cliente.CreadoUtc.ToLocalTime().ToString("dd/MM/yyyy"),
            Lista                = await ConstruirListaAsync(null, null, null, null, 1, ct),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(int id, EditarClienteViewModel modelo, CancellationToken ct)
    {
        if (id != modelo.Id) return BadRequest();

        if (!ModelState.IsValid) return await VolverAEditar(id, modelo, ct);

        var resultado = await clientes.ActualizarClienteAsync(
            new SolicitudActualizarCliente(
                modelo.Id, modelo.NombreCompleto, modelo.Telefono, modelo.Activo, modelo.NuevaContrasena), ct);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return await VolverAEditar(id, modelo, ct);
        }

        TempData["Ok"] = "Cambios guardados.";
        return RedirectToAction(nameof(Index));
    }

    // Recarga desde la base los campos de solo display que no viajan en el POST.
    private async Task<IActionResult> VolverAEditar(int id, EditarClienteViewModel modelo, CancellationToken ct)
    {
        var cliente = await clientes.ObtenerAsync(id, ct);
        if (cliente is null) return NotFound();
        modelo.Correo               = cliente.Correo;
        modelo.UltimoAccesoRelativo = FechaRelativa.Formatear(cliente.UltimoAccesoUtc, reloj.GetUtcNow());
        modelo.ClienteDesde         = cliente.CreadoUtc.ToLocalTime().ToString("dd/MM/yyyy");
        modelo.Lista                = await ConstruirListaAsync(null, null, null, null, 1, ct);
        return View(modelo);
    }

    // SRP: construye el ClientesListaViewModel reutilizable en Index, Nuevo y Editar.
    private async Task<ClientesListaViewModel> ConstruirListaAsync(
        string? q, bool? activo, string? orden, string? dir, int pagina, CancellationToken ct)
    {
        var resultado = await clientes.ListarAsync(new FiltroClientes(q, activo, pagina, Orden: orden, Dir: dir), ct);
        var resumen   = await clientes.ObtenerResumenAsync(ct);

        return new ClientesListaViewModel
        {
            Items = resultado.Items.Select(c => new ClienteFilaViewModel
            {
                Id                   = c.Id,
                NombreCompleto       = c.NombreCompleto,
                Correo               = c.Correo,
                Telefono             = c.Telefono,
                Activo               = c.Activo,
                UltimoAccesoRelativo = FechaRelativa.Formatear(c.UltimoAccesoUtc, reloj.GetUtcNow()),
            }).ToList(),
            Busqueda      = q,
            Activo        = activo,
            Pagina        = resultado.Pagina,
            TotalPaginas  = resultado.TotalPaginas,
            Total         = resultado.Total,
            TotalClientes = resumen.Total,
            Activos       = resumen.Activos,
            Inactivos     = resumen.Inactivos,
            TamanoPagina  = resultado.TamanoPagina,
            Orden         = orden,
            Dir           = dir,
        };
    }
}
