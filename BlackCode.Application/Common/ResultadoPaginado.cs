namespace BlackCode.Application.Common;

/// <summary>Una página de resultados de cualquier listado (usuarios, productos, pedidos).</summary>
public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Items, int Total, int Pagina, int TamanoPagina)
{
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Total / (double)TamanoPagina));
}
