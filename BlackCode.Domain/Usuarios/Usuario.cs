namespace BlackCode.Domain.Usuarios;

public class Usuario
{
    // Regla del negocio: a los 5 intentos fallidos la cuenta se bloquea 15 minutos.
    public const int MaximoIntentosFallidos = 5;
    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
    public bool Activo { get; set; } = true;

    // Bloqueo por intentos fallidos de login
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHastaUtc { get; set; }

    public DateTime TerminosAceptadosUtc { get; set; }
    public DateTime CreadoUtc { get; set; }

    public bool EstaBloqueado(DateTime ahoraUtc) => BloqueadoHastaUtc is { } hasta && hasta > ahoraUtc;

    /// <summary>
    /// Suma un intento fallido; al llegar a MaximoIntentosFallidos bloquea por DuracionBloqueo.
    /// Si un bloqueo anterior ya venció, el conteo empieza de cero.
    /// </summary>
    public void RegistrarIntentoFallido(DateTime ahoraUtc)
    {
        if (BloqueadoHastaUtc is { } hasta && hasta <= ahoraUtc)
            ReiniciarIntentos();

        IntentosFallidos++;
        if (IntentosFallidos >= MaximoIntentosFallidos)
            BloqueadoHastaUtc = ahoraUtc + DuracionBloqueo;
    }

    /// <summary>Login exitoso o desbloqueo manual desde el panel.</summary>
    public void ReiniciarIntentos()
    {
        IntentosFallidos = 0;
        BloqueadoHastaUtc = null;
    }
}
