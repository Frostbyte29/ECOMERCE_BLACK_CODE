using BlackCode.Domain.Usuarios;

namespace BlackCode.Application.Features.Autenticacion;

public interface IAutenticacionService
{
    Task<ResultadoAutenticacion> RegistrarAsync(SolicitudRegistro solicitud, CancellationToken ct = default);

    /// <param name="soloAdmin">true desde /Admin/Login: un Cliente con la clave correcta NO entra.</param>
    Task<ResultadoAutenticacion> IniciarSesionAsync(SolicitudInicioSesion solicitud, bool soloAdmin, CancellationToken ct = default);

    /// <summary>
    /// La cookie lo consulta en cada petición: si el usuario fue desactivado
    /// (o le cambiaron el rol), la sesión que ya tenía deja de valer.
    /// </summary>
    Task<bool> SesionSigueValidaAsync(int usuarioId, RolUsuario rol, CancellationToken ct = default);
}
