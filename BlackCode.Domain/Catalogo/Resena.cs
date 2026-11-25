namespace BlackCode.Domain.Catalogo;

// SRP: representa la opinión verificada de un cliente sobre un producto que compró.
public class Resena
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int UsuarioId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Calificacion { get; set; }   // 1–5 estrellas
    public string? Comentario { get; set; }
    public DateTime CreadoUtc { get; set; }
}
