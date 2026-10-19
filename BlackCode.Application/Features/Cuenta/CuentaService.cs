using BlackCode.Application.Abstractions;
using BlackCode.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Cuenta;

/// <summary>
/// SRP: gestionar la cuenta del usuario autenticado (≠ autenticar).
/// DIP: depende de IAppDbContext e IContrasenaService, no de implementaciones concretas.
/// </summary>
public sealed class CuentaService(
    IAppDbContext db,
    IContrasenaService contrasenas) : ICuentaService
{
    private const int LargoMinimoContrasena = 8;

    public async Task<ResultadoOperacion> CambiarContrasenaAsync(
        int usuarioId, string actual, string nueva, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null) return ResultadoOperacion.Fallo("Usuario no encontrado.");

        if (contrasenas.Verificar(usuario.HashContrasena, actual) == ResultadoContrasena.Fallido)
            return ResultadoOperacion.Fallo("Contraseña actual incorrecta.");

        if (nueva.Length < LargoMinimoContrasena)
            return ResultadoOperacion.Fallo($"La contraseña debe tener al menos {LargoMinimoContrasena} caracteres.");

        usuario.HashContrasena = contrasenas.Hashear(nueva);
        await db.SaveChangesAsync(ct);
        return ResultadoOperacion.Ok();
    }
}
