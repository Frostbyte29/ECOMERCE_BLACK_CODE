using BlackCode.Application.Abstractions;
using BlackCode.Application.Common;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Clientes;

/// <summary>
/// SRP: gestionar clientes desde el panel es otra responsabilidad que autenticarlos.
/// Todas las operaciones filtran por RolUsuario.Cliente; el admin nunca aparece.
/// DIP: depende de IAppDbContext e IUsuarioActual, nunca de clases concretas.
/// </summary>
public sealed class GestionClientesService(
    IAppDbContext db,
    IUsuarioActual usuarioActual) : IGestionClientesService
{
    private const int TamanoPaginaMaximo = 50;

    // OCP: agregar criterio de orden = nueva entrada en el diccionario, sin tocar el resto del método.
    private static readonly Dictionary<string, Func<IQueryable<Domain.Usuarios.Usuario>, bool, IQueryable<Domain.Usuarios.Usuario>>> Ordenamientos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["nombre"] = (q, asc) => asc
            ? q.OrderBy(u => u.NombreCompleto).ThenBy(u => u.Id)
            : q.OrderByDescending(u => u.NombreCompleto).ThenBy(u => u.Id),
        ["estado"] = (q, asc) => asc
            ? q.OrderBy(u => u.Activo).ThenBy(u => u.Id)
            : q.OrderByDescending(u => u.Activo).ThenBy(u => u.Id),
        ["acceso"] = (q, asc) => asc
            ? q.OrderBy(u => u.UltimoAccesoUtc).ThenBy(u => u.Id)
            : q.OrderByDescending(u => u.UltimoAccesoUtc).ThenBy(u => u.Id),
    };

    public async Task<ResultadoPaginado<ClienteResumen>> ListarAsync(FiltroClientes filtro, CancellationToken ct = default)
    {
        var consulta = db.Usuarios.AsNoTracking()
            .Where(u => u.Rol == RolUsuario.Cliente);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var termino = filtro.Busqueda.Trim();
            consulta = consulta.Where(u => u.NombreCompleto.Contains(termino) || u.Correo.Contains(termino));
        }
        if (filtro.Activo is { } activo) consulta = consulta.Where(u => u.Activo == activo);

        if (!string.IsNullOrWhiteSpace(filtro.Orden) && Ordenamientos.TryGetValue(filtro.Orden, out var ordenar))
            consulta = ordenar(consulta, !string.Equals(filtro.Dir, "desc", StringComparison.OrdinalIgnoreCase));
        else
            consulta = consulta.OrderByDescending(u => u.CreadoUtc).ThenBy(u => u.Id);

        var tamano = Math.Clamp(filtro.TamanoPagina, 1, TamanoPaginaMaximo);
        var pagina = Math.Max(1, filtro.Pagina);
        var total = await consulta.CountAsync(ct);

        var filas = await consulta
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(u => new { u.Id, u.NombreCompleto, u.Correo, u.Telefono, u.Activo, u.UltimoAccesoUtc, u.CreadoUtc })
            .ToListAsync(ct);

        var items = filas
            .Select(u => new ClienteResumen(u.Id, u.NombreCompleto, u.Correo, u.Telefono, u.Activo, u.UltimoAccesoUtc, u.CreadoUtc))
            .ToList();

        return new ResultadoPaginado<ClienteResumen>(items, total, pagina, tamano);
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(int clienteId, bool activo, CancellationToken ct = default)
    {
        if (!usuarioActual.EsAdmin) return ResultadoOperacion.Fallo("No tienes permiso para esta acción.");

        var cliente = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == clienteId && u.Rol == RolUsuario.Cliente, ct);
        if (cliente is null) return ResultadoOperacion.Fallo("El cliente no existe.");

        cliente.Activo = activo;
        await db.SaveChangesAsync(ct);
        return ResultadoOperacion.Ok();
    }
}
