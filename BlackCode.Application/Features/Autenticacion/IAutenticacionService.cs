using BlackCode.Domain.Usuarios;

namespace BlackCode.Application.Features.Autenticacion;

public interface IAutenticacionService
{
    Task<ResultadoAutenticacion> RegistrarAsync(SolicitudRegistro solicitud, CancellationToken ct = default);

    /// <param name="portal">
    /// Portal desde el que se autentica. Determina el rol permitido:
    /// Tienda → Cliente, Panel → Admin. Quien entra por el portal equivocado
    /// recibe el mensaje genérico sin revelar si la cuenta existe o la clave es correcta.
    /// </param>
    Task<ResultadoAutenticacion> IniciarSesionAsync(SolicitudInicioSesion solicitud, PortalAcceso portal, CancellationToken ct = default);

    /// <summary>
    /// La cookie lo consulta en cada petición: si el usuario fue desactivado
    /// (o le cambiaron el rol), la sesión que ya tenía deja de valer.
    /// </summary>
    Task<bool> SesionSigueValidaAsync(int usuarioId, RolUsuario rol, CancellationToken ct = default);
}
