namespace BlackCode.Domain.Usuarios;

// SRP: encapsula una dirección de envío reutilizable del cliente.
public class DireccionGuardada
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Distrito { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public bool EsPredeterminada { get; set; }
    public DateTime CreadoUtc { get; set; }
}
