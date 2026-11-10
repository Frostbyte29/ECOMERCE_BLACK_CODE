using System.Security.Claims;
using BlackCode.Application.Abstractions;
using BlackCode.Domain.Usuarios;

namespace BlackCode.Web.Security;

/// <summary>
/// Implementación web de IUsuarioActual: lee los datos de la cookie de sesión.
/// Los servicios de Application nunca tocan HttpContext directamente (DIP).
/// </summary>
public sealed class UsuarioActualHttp(IHttpContextAccessor accessor) : IUsuarioActual
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool EstaAutenticado => Principal?.Identity?.IsAuthenticated ?? false;

    public int? UsuarioId =>
        int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public bool EsAdmin => Principal?.IsInRole(nameof(RolUsuario.Admin)) ?? false;
}
