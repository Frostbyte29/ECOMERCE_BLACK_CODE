using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Carrito;
namespace BlackCode.Web.Security;

public sealed class CarritoIdentidad(IHttpContextAccessor accessor, IUsuarioActual usuario)
{
    public const string Cookie = "bc_proto_carrito";
    public string? Token => accessor.HttpContext?.Request.Cookies[Cookie];
    public PropietarioCarrito Obtener()
    {
        if (usuario.UsuarioId is int id) return new(id, null);
        var ctx = accessor.HttpContext ?? throw new InvalidOperationException();
        var token = Token;
        if (token is null || token.Length != 64 || !token.All(Uri.IsHexDigit))
        {
            token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            ctx.Response.Cookies.Append(Cookie, token, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, IsEssential = true, MaxAge = TimeSpan.FromDays(30) });
        }
        return new(null, token);
    }
}
