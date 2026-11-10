namespace BlackCode.Domain.Compras;

// Un carrito es de un usuario (UsuarioId) o de un invitado (TokenAnonimo, guardado en una cookie).
// Al iniciar sesión, el carrito del invitado se fusiona con el del usuario.
public class Carrito
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string? TokenAnonimo { get; set; }
    public DateTime ActualizadoUtc { get; set; }

    public List<ItemCarrito> Items { get; set; } = [];
}
