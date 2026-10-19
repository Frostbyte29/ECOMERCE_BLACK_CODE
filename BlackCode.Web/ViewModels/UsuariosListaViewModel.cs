using BlackCode.Domain.Usuarios;

namespace BlackCode.Web.ViewModels;

public class UsuarioFilaViewModel
{
    public int Id { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string Telefono { get; init; } = "—";
    public RolUsuario Rol { get; init; }
    public bool Activo { get; init; }
    public bool EstaBloqueado { get; init; }
    public bool EsUsuarioActual { get; init; }
    public string FechaRegistro { get; init; } = string.Empty;
}

public class UsuariosListaViewModel
{
    public IReadOnlyList<UsuarioFilaViewModel> Items { get; init; } = [];
    public string? Busqueda { get; init; }
    public RolUsuario? Rol { get; init; }
    public bool? Activo { get; init; }
    public int Pagina { get; init; }
    public int TotalPaginas { get; init; }
    public int Total { get; init; }
}
