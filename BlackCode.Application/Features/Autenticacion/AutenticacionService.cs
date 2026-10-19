using BlackCode.Application.Abstractions;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Autenticacion;

/// <summary>
/// SRP: solo decide quién puede entrar y crea cuentas. No sabe de cookies (eso es Web),
/// ni de cómo se hashea (IContrasenaService), ni de MySQL (IAppDbContext). DIP en las tres.
/// </summary>
public sealed class AutenticacionService(
    IAppDbContext db,
    IContrasenaService contrasenas,
    TimeProvider reloj) : IAutenticacionService
{
    private const int LargoMinimoContrasena = 8;

    // Mismo mensaje para correo inexistente, clave incorrecta y cuenta bloqueada:
    // quien ataca no puede saber si el correo existe.
    private const string CredencialesInvalidas =
        "Correo o contraseña incorrectos, o la cuenta está bloqueada temporalmente. Intenta de nuevo en unos minutos.";

    private const string CorreoYaRegistrado = "Ya existe una cuenta con ese correo.";

    public async Task<ResultadoAutenticacion> RegistrarAsync(SolicitudRegistro solicitud, CancellationToken ct = default)
    {
        if (!solicitud.AceptoTerminos)
            return ResultadoAutenticacion.Fallo("Debes aceptar los términos y la política de privacidad.");
        if (solicitud.Contrasena.Length < LargoMinimoContrasena)
            return ResultadoAutenticacion.Fallo($"La contraseña debe tener al menos {LargoMinimoContrasena} caracteres.");

        var correo = NormalizarCorreo(solicitud.Correo);
        if (await db.Usuarios.AnyAsync(u => u.Correo == correo, ct))
            return ResultadoAutenticacion.Fallo(CorreoYaRegistrado);

        var ahora = Ahora();
        var usuario = new Usuario
        {
            Correo = correo,
            NombreCompleto = solicitud.NombreCompleto.Trim(),
            Telefono = solicitud.Telefono.Trim(),
            HashContrasena = contrasenas.Hashear(solicitud.Contrasena),
            Rol = RolUsuario.Cliente,          // el rol NUNCA viene del formulario
            TerminosAceptadosUtc = ahora,
            CreadoUtc = ahora
        };

        db.Usuarios.Add(usuario);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Dos registros simultáneos con el mismo correo: el índice único de la base decide.
            return ResultadoAutenticacion.Fallo(CorreoYaRegistrado);
        }

        return ResultadoAutenticacion.Ok(ASesion(usuario));
    }

    public async Task<ResultadoAutenticacion> IniciarSesionAsync(
        SolicitudInicioSesion solicitud, bool soloAdmin, CancellationToken ct = default)
    {
        var correo = NormalizarCorreo(solicitud.Correo);
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo, ct);
        if (usuario is null) return ResultadoAutenticacion.Fallo(CredencialesInvalidas);

        var ahora = Ahora();
        if (usuario.EstaBloqueado(ahora)) return ResultadoAutenticacion.Fallo(CredencialesInvalidas);

        var verificacion = contrasenas.Verificar(usuario.HashContrasena, solicitud.Contrasena);
        if (verificacion == ResultadoContrasena.Fallido)
        {
            usuario.RegistrarIntentoFallido(ahora);   // la regla del bloqueo vive en Domain
            await db.SaveChangesAsync(ct);
            return ResultadoAutenticacion.Fallo(CredencialesInvalidas);
        }

        // Solo quien ya acertó la clave se entera de que su cuenta está desactivada.
        if (!usuario.Activo)
            return ResultadoAutenticacion.Fallo("Tu cuenta está desactivada. Contáctanos para reactivarla.");

        // Un Cliente con la clave correcta en el login de admin: mismo mensaje genérico, sin sesión.
        if (soloAdmin && usuario.Rol != RolUsuario.Admin)
            return ResultadoAutenticacion.Fallo(CredencialesInvalidas);

        if (verificacion == ResultadoContrasena.ExitosoRehashRequerido)
            usuario.HashContrasena = contrasenas.Hashear(solicitud.Contrasena);

        usuario.ReiniciarIntentos();
        await db.SaveChangesAsync(ct);
        return ResultadoAutenticacion.Ok(ASesion(usuario));
    }

    public Task<bool> SesionSigueValidaAsync(int usuarioId, RolUsuario rol, CancellationToken ct = default)
        => db.Usuarios.AsNoTracking().AnyAsync(u => u.Id == usuarioId && u.Activo && u.Rol == rol, ct);

    private DateTime Ahora() => reloj.GetUtcNow().UtcDateTime;

    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

    private static SesionUsuario ASesion(Usuario u) => new(u.Id, u.Correo, u.NombreCompleto, u.Rol);
}
