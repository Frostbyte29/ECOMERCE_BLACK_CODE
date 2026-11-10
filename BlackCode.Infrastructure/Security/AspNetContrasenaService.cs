using BlackCode.Application.Abstractions;
using BlackCode.Domain.Usuarios;
using Microsoft.AspNetCore.Identity;

namespace BlackCode.Infrastructure.Security;

/// <summary>
/// Usa el PasswordHasher de ASP.NET Core (PBKDF2 con sal aleatoria y formato versionado).
/// Requiere en el .csproj de Infrastructure: &lt;FrameworkReference Include="Microsoft.AspNetCore.App" /&gt;
/// </summary>
public sealed class AspNetContrasenaService : IContrasenaService
{
    // El hasher pide un "usuario" pero la implementación por defecto no lo usa.
    private static readonly Usuario AnyUser = new();
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Hashear(string contrasena) => _hasher.HashPassword(AnyUser, contrasena);

    public ResultadoContrasena Verificar(string hashContrasena, string contrasenaProvista) =>
        _hasher.VerifyHashedPassword(AnyUser, hashContrasena, contrasenaProvista) switch
        {
            PasswordVerificationResult.Success             => ResultadoContrasena.Exitoso,
            PasswordVerificationResult.SuccessRehashNeeded => ResultadoContrasena.ExitosoRehashRequerido,
            _                                              => ResultadoContrasena.Fallido
        };
}
