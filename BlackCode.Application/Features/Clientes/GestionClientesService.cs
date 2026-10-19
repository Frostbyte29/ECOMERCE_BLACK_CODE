using BlackCode.Application.Abstractions;
using BlackCode.Application.Common;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Clientes;

/// <summary>
/// SRP: gestionar clientes desde el panel es otra responsabilidad que autenticarlos.
/// Todas las operaciones filtran por RolUsuario.Cliente; el admin nunca aparece.
/// DIP: depende de abstracciones (IAppDbContext, IUsuarioActual, IContrasenaService, TimeProvider).
/// </summary>
public sealed class GestionClientesService(
    IAppDbContext db,
    IUsuarioActual usuarioActual,
    IContrasenaService contrasenas,
    TimeProvider reloj) : IGestionClientesService
{
    private const int TamanoPaginaMaximo = 50;

    // OCP: agregar criterio de orden = nueva entrada en el diccionario, sin modificar el método.
    private static readonly Dictionary<string, Func<IQueryable<Usuario>, bool, IQueryable<Usuario>>> Ordenamientos
        = new(StringComparer.OrdinalIgnoreCase)
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

    public async Task<ClienteResumen?> ObtenerAsync(int clienteId, CancellationToken ct = default)
    {
        var u = await db.Usuarios.AsNoTracking()
            .Where(u => u.Id == clienteId && u.Rol == RolUsuario.Cliente)
            .Select(u => new { u.Id, u.NombreCompleto, u.Correo, u.Telefono, u.Activo, u.UltimoAccesoUtc, u.CreadoUtc })
            .FirstOrDefaultAsync(ct);

        if (u is null) return null;
        return new ClienteResumen(u.Id, u.NombreCompleto, u.Correo, u.Telefono, u.Activo, u.UltimoAccesoUtc, u.CreadoUtc);
    }

    public async Task<ResultadoOperacion> CrearAsync(SolicitudCrearCliente solicitud, CancellationToken ct = default)
    {
        if (!usuarioActual.EsAdmin) return ResultadoOperacion.Fallo("No tienes permiso para esta acción.");

        var correo = Normalizacion.Correo(solicitud.Correo);
        if (await db.Usuarios.AnyAsync(u => u.Correo == correo, ct))
            return ResultadoOperacion.Fallo("Ya existe una cuenta con ese correo.");

        var ahora = reloj.GetUtcNow().UtcDateTime;
        var cliente = new Usuario
        {
            NombreCompleto = solicitud.NombreCompleto.Trim(),
            Correo        = correo,
            Telefono      = string.IsNullOrWhiteSpace(solicitud.Telefono) ? null : solicitud.Telefono.Trim(),
            HashContrasena = contrasenas.Hashear(solicitud.Contrasena),
            Rol            = RolUsuario.Cliente,
            Activo         = true,
            CreadoUtc      = ahora,
            TerminosAceptadosUtc = ahora,
        };

        db.Usuarios.Add(cliente);
        await db.SaveChangesAsync(ct);
        return ResultadoOperacion.Ok();
    }

    public async Task<ResultadoOperacion> ActualizarClienteAsync(SolicitudActualizarCliente solicitud, CancellationToken ct = default)
    {
        if (!usuarioActual.EsAdmin) return ResultadoOperacion.Fallo("No tienes permiso para esta acción.");

        var cliente = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == solicitud.Id && u.Rol == RolUsuario.Cliente, ct);
        if (cliente is null) return ResultadoOperacion.Fallo("Cliente no encontrado.");

        ActualizarDatos(cliente, solicitud);
        ActualizarEstado(cliente, solicitud.Activo);
        if (!string.IsNullOrEmpty(solicitud.NuevaContrasena))
            ActualizarContrasena(cliente, solicitud.NuevaContrasena);

        await db.SaveChangesAsync(ct);
        return ResultadoOperacion.Ok();
    }

    private static void ActualizarDatos(Usuario cliente, SolicitudActualizarCliente solicitud)
    {
        cliente.NombreCompleto = solicitud.NombreCompleto.Trim();
        cliente.Telefono       = string.IsNullOrWhiteSpace(solicitud.Telefono) ? null : solicitud.Telefono.Trim();
    }

    private static void ActualizarEstado(Usuario cliente, bool activo) => cliente.Activo = activo;

    private void ActualizarContrasena(Usuario cliente, string nuevaContrasena)
        => cliente.HashContrasena = contrasenas.Hashear(nuevaContrasena);

    // SRP: contar clientes por estado es una responsabilidad distinta a listarlos con filtros y paginación.
    public async Task<ResumenClientes> ObtenerResumenAsync(CancellationToken ct = default)
    {
        var grupos = await db.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == RolUsuario.Cliente)
            .GroupBy(u => u.Activo)
            .Select(g => new { Activo = g.Key, Cantidad = g.Count() })
            .ToListAsync(ct);

        var activos   = grupos.FirstOrDefault(g => g.Activo)?.Cantidad  ?? 0;
        var inactivos = grupos.FirstOrDefault(g => !g.Activo)?.Cantidad ?? 0;
        return new ResumenClientes(activos + inactivos, activos, inactivos);
    }
}
