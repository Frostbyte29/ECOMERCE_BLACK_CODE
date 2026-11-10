using BlackCode.Domain.Catalogo;

namespace BlackCode.Domain.Compras;

public class Favorito
{
    public int UsuarioId { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public DateTime CreadoUtc { get; set; }
}
