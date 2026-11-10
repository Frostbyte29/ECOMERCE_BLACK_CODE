namespace BlackCode.Domain.Catalogo;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? UrlImagen { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;

    public List<Producto> Productos { get; set; } = [];
}
