namespace BlackCode.Domain.Catalogo;

public enum EstadoProducto
{
    Borrador = 1,   // el admin lo está armando, no se ve en la tienda
    Publicado = 2,  // visible en la tienda
    Archivado = 3   // ya no se vende; los pedidos viejos lo siguen referenciando
}
