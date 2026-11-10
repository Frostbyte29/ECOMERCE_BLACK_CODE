namespace BlackCode.Domain.Usuarios;

public class Usuario
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
    public bool Activo { get; set; } = true;

    // Reservado para un limitador de intentos de login; sin uso activo.
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHastaUtc { get; set; }

    public DateTime? UltimoAccesoUtc { get; set; }
    public DateTime TerminosAceptadosUtc { get; set; }
    public DateTime CreadoUtc { get; set; }

    public void RegistrarAcceso(DateTime ahoraUtc) => UltimoAccesoUtc = ahoraUtc;
}
