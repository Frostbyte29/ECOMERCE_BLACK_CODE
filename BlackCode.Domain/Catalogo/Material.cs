namespace BlackCode.Domain.Catalogo;

// Tabla de catálogo: así el filtro no se rompe con "acero" vs "Acero Inoxidable".
public class Material
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
