namespace BlackCode.Domain.Reclamos;

// SRP: registra una queja o reclamo formal conforme al libro de reclamaciones peruano.
public class Reclamo
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }         // null si el consumidor no tiene cuenta
    public string NombresCompletos { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;    // DNI o RUC
    public string Domicilio { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? NumeroPedido { get; set; }   // código BC-XXXXXXXX si aplica
    public TipoReclamo Tipo { get; set; }
    public string DescripcionBien { get; set; } = string.Empty;  // producto o servicio
    public string Detalle { get; set; } = string.Empty;
    public string PedidoCliente { get; set; } = string.Empty;    // lo que el cliente solicita
    public DateTime CreadoUtc { get; set; }
}
