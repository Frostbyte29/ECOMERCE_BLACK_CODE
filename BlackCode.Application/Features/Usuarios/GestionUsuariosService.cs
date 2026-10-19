using BlackCode.Application.Abstractions;
using BlackCode.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Usuarios;

/// <summary>
/// SRP: administrar usuarios desde el panel es otra responsabilidad que autenticarlos,
/// por eso es otro servicio (cambia por otras razones y lo usa otro controlador).
/// ISP: de IUsuarioActual solo necesita saber quién actúa y si es admin.
/// </summary>
public sealed class GestionUsuariosService(
    IAppDbContext db,
    IUsuarioActual usuarioActual,
    TimeProvider reloj) : IGestionUsuariosService
{
    private const int TamanoPaginaMaximo = 50;

    public async Task<ResultadoPaginado<UsuarioResumen>> ListarAsync(FiltroUsuarios filtro, CancellationToken ct = default)
    {
        var consulta = db.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var termino = filtro.Busqueda.Trim();
            consulta = consulta.Where(u => u.NombreCompleto.Contains(termino) || u.Correo.Contains(termino));
        }
        if (filtro.Rol is { } rol) consulta = consulta.Where(u => u.Rol == rol);
        if (filtro.Activo is { } activo) consulta = consulta.Where(u => u.Activo == activo);

        var tamano = Math.Clamp(filtro.TamanoPagina, 1, TamanoPaginaMaximo);
        var pagina = Math.Max(1, filtro.Pagina);
        var total = await consulta.CountAsync(ct);

        var filas = await consulta
            .OrderByDescending(u => u.CreadoUtc).ThenBy(u => u.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(u => new { u.Id, u.NombreCompleto, u.Correo, u.Telefono, u.Rol, u.Activo, u.BloqueadoHastaUtc, u.CreadoUtc })
            .ToListAsync(ct);

        // "Está bloqueado" depende de la hora actual: se calcula en memoria con el reloj inyectado.
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var items = filas
            .Select(u => new UsuarioResumen(
                u.Id, u.NombreCompleto, u.Correo, u.Telefono, u.Rol, u.Activo,
                u.BloqueadoHastaUtc is { } hasta && hasta > ahora,
                u.CreadoUtc))
            .ToList();

        return new ResultadoPaginado<UsuarioResumen>(items, total, pagina, tamano);
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(int usuarioId, bool activo, CancellationToken ct = default)
    {
        if (!usuarioActual.EsAdmin) return ResultadoOperacion.Fallo("No tienes permiso para esta acción.");

        // Basta con impedir la auto-desactivación: quien actúa es un admin activo,
        // así que siempre queda al menos uno.
        if (!activo && usuarioActual.UsuarioId == usuarioId)
            return ResultadoOperacion.Fallo("No puedes desactivar tu propia cuenta.");

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null) return ResultadoOperacion.Fallo("El usuario no existe.");

        usuario.Activo = activo;
        await db.SaveChangesAsync(ct);
        return ResultadoOperacion.Ok();
    }

    public async Task<ResultadoOperacion> DesbloquearAsync(int usuarioId, CancellationToken ct = default)
    {
        if (!usuarioActual.EsAdmin) return ResultadoOperacion.Fallo("No tienes permiso para esta acción.");

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null) return ResultadoOperacion.Fallo("El usuario no existe.");

        usuario.ReiniciarIntentos();
        await db.SaveChangesAsync(ct);
        return ResultadoOperacion.Ok();
    }
}
