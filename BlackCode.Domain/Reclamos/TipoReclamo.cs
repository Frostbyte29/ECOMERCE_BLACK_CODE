namespace BlackCode.Domain.Reclamos;

// OCP: si el Indecopi agrega categorías nuevas, se extiende el enum sin tocar la lógica de negocio.
public enum TipoReclamo
{
    Queja  = 1,   // disconformidad con la atención, no implica defecto del producto
    Reclamo = 2   // disconformidad con el producto o servicio contratado
}
