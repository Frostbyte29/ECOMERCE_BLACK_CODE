using BlackCode.Domain.Usuarios;

namespace BlackCode.Application.Features.Usuarios;

public sealed record FiltroUsuarios(
    string? Busqueda,
    RolUsuario? Rol,
    bool? Activo,
    int Pagina = 1,
    int TamanoPagina = 10);

public sealed record UsuarioResumen(
    int Id,
    string NombreCompleto,
    string Correo,
    string? Telefono,
    RolUsuario Rol,
    bool Activo,
    bool EstaBloqueado,
    DateTime CreadoUtc);
