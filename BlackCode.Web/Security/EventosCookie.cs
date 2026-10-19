using System.Security.Claims;
using BlackCode.Application.Features.Autenticacion;
using BlackCode.Domain.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlackCode.Web.Security;

/// <summary>
/// Dos comportamientos de la cookie de sesión que Program.cs conecta con una línea cada uno.
/// </summary>
public static class EventosCookie
{
    /// <summary>Quien no tiene sesión y entra a /Admin/... va al login de admin; el resto, al de cliente.</summary>
    public static Task RedirigirAlLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        var request = context.Request;
        var rutaLogin = request.Path.StartsWithSegments("/Admin") ? "/Admin/Login" : "/Cuenta/Login";
        var returnUrl = $"{request.PathBase}{request.Path}{request.QueryString}";

        context.Response.Redirect($"{request.PathBase}{rutaLogin}?returnUrl={Uri.EscapeDataString(returnUrl)}");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Si un admin desactiva a un usuario (o le cambia el rol), su cookie deja de valer en la
    /// siguiente petición. Sin esto, "desactivar" no sacaría a nadie hasta que venza la cookie.
    /// </summary>
    public static async Task ValidarSesion(CookieValidatePrincipalContext context)
    {
        // Imágenes, CSS y JS no necesitan consultar la base.
        if (Path.HasExtension(context.Request.Path.Value)) return;

        var principal = context.Principal;
        var idValido = int.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId);
        var rolValido = Enum.TryParse<RolUsuario>(principal?.FindFirstValue(ClaimTypes.Role), out var rol);

        if (idValido && rolValido)
        {
            var autenticacion = context.HttpContext.RequestServices.GetRequiredService<IAutenticacionService>();
            if (await autenticacion.SesionSigueValidaAsync(usuarioId, rol, context.HttpContext.RequestAborted)) return;
        }

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
