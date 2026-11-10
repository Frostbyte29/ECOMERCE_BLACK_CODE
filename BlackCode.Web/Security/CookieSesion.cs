using System.Security.Claims;
using BlackCode.Application.Features.Autenticacion;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlackCode.Web.Security;

/// <summary>
/// SRP: convertir una SesionUsuario (Application) en la cookie de sesión es trabajo de Web.
/// La usan los dos logins (cliente y admin) y el registro, para no repetir código.
/// </summary>
public static class CookieSesion
{
    private static readonly TimeSpan DuracionPersistente = TimeSpan.FromDays(14);

    public static ClaimsPrincipal CrearPrincipal(SesionUsuario sesion)
    {
        var identidad = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, sesion.Id.ToString()),
                new Claim(ClaimTypes.Email, sesion.Correo),
                new Claim(ClaimTypes.Name, sesion.NombreCompleto),
                new Claim(ClaimTypes.Role, sesion.Rol.ToString())
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identidad);
    }

    /// <param name="persistente">"Mantener mi sesión activa": la cookie sobrevive al cerrar el navegador.</param>
    public static Task IniciarAsync(HttpContext http, SesionUsuario sesion, bool persistente, DateTimeOffset ahora)
        => http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            CrearPrincipal(sesion),
            new AuthenticationProperties
            {
                IsPersistent = persistente,
                ExpiresUtc = persistente ? ahora.Add(DuracionPersistente) : null
            });
}
