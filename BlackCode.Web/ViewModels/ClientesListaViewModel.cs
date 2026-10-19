namespace BlackCode.Web.ViewModels;

public class ClienteFilaViewModel
{
    public int Id { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public string UltimoAccesoRelativo { get; init; } = string.Empty;
}

public class ClientesListaViewModel
{
    public IReadOnlyList<ClienteFilaViewModel> Items { get; init; } = [];
    public string? Busqueda { get; init; }
    public bool? Activo { get; init; }
    public int Pagina { get; init; }
    public int TotalPaginas { get; init; }
    public int Total { get; init; }
    public string? Orden { get; init; }
    public string? Dir { get; init; }
}
