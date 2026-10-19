using BlackCode.Domain.Usuarios;

namespace BlackCode.Application.Features.Autenticacion;

public sealed record SolicitudRegistro(
    string NombreCompleto,
    string Correo,
    string Telefono,
    string Contrasena,
    bool AceptoTerminos);

public sealed record SolicitudInicioSesion(string Correo, string Contrasena);

/// <summary>Lo mínimo que la capa Web necesita para armar la cookie de sesión.</summary>
public sealed record SesionUsuario(int Id, string Correo, string NombreCompleto, RolUsuario Rol);

public sealed record ResultadoAutenticacion(bool Exitoso, string? Error, SesionUsuario? Sesion)
{
    public static ResultadoAutenticacion Ok(SesionUsuario sesion) => new(true, null, sesion);
    public static ResultadoAutenticacion Fallo(string error) => new(false, error, null);
}

/// <summary>
/// Identifica desde qué portal se inicia sesión.
/// OCP: agregar un portal nuevo solo requiere una entrada aquí y en RolPorPortal,
/// sin modificar IniciarSesionAsync.
/// </summary>
public enum PortalAcceso
{
    Tienda,
    Panel,
}
