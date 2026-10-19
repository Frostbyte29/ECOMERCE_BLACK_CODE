namespace BlackCode.Application.Features.Clientes;

public sealed record FiltroClientes(
    string? Busqueda,
    bool? Activo,
    int Pagina = 1,
    int TamanoPagina = 10,
    string? Orden = null,
    string? Dir = null);

/// <summary>
/// Proyección de solo lectura para la lista de clientes del panel.
/// No incluye Rol porque el servicio filtra exclusivamente por RolUsuario.Cliente.
/// </summary>
public sealed record ClienteResumen(
    int Id,
    string NombreCompleto,
    string Correo,
    string? Telefono,
    bool Activo,
    DateTime? UltimoAccesoUtc,
    DateTime CreadoUtc);

public sealed record SolicitudCrearCliente(
    string NombreCompleto,
    string Correo,
    string? Telefono,
    string Contrasena);

public sealed record SolicitudActualizarCliente(
    int Id,
    string NombreCompleto,
    string? Telefono,
    bool Activo,
    string? NuevaContrasena);

public sealed record ResumenClientes(int Total, int Activos, int Inactivos);
