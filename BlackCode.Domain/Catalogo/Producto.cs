namespace BlackCode.Domain.Catalogo;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    // El precio vive en el producto; el stock vive en cada variante (talla).
    public decimal Precio { get; set; }

    public string? Acabado { get; set; }        // acabado, ej. "Plateado envejecido"
    public bool Destacado { get; set; }          // para ordenar por "Más relevante"
    public EstadoProducto Estado { get; set; } = EstadoProducto.Borrador;
    public DateTime CreadoUtc { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public int MaterialId { get; set; }
    public Material Material { get; set; } = null!;

    public int ColorId { get; set; }
    public Color Color { get; set; } = null!;

    public List<VarianteProducto> Variantes { get; set; } = [];
    public List<ImagenProducto> Imagenes { get; set; } = [];
}
