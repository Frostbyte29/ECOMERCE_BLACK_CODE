using BlackCode.Domain.Usuarios;
using Xunit;

namespace BlackCode.Tests.Domain;

public class UsuarioBloqueoTests
{
    private static readonly DateTime Ahora = new(2026, 10, 20, 15, 0, 0, DateTimeKind.Utc);

    private static Usuario FallarVeces(int veces)
    {
        var usuario = new Usuario();
        for (var i = 0; i < veces; i++) usuario.RegistrarIntentoFallido(Ahora);
        return usuario;
    }

    [Fact]
    public void Cuatro_fallos_no_bloquean()
    {
        var usuario = FallarVeces(4);

        Assert.False(usuario.EstaBloqueado(Ahora));
        Assert.Equal(4, usuario.IntentosFallidos);
    }

    [Fact]
    public void El_quinto_fallo_bloquea_por_15_minutos()
    {
        var usuario = FallarVeces(5);

        Assert.True(usuario.EstaBloqueado(Ahora));
        Assert.Equal(Ahora.AddMinutes(15), usuario.BloqueadoHastaUtc);
    }

    [Fact]
    public void El_bloqueo_termina_a_los_15_minutos()
    {
        var usuario = FallarVeces(5);

        Assert.True(usuario.EstaBloqueado(Ahora.AddMinutes(14)));
        Assert.False(usuario.EstaBloqueado(Ahora.AddMinutes(15)));
    }

    [Fact]
    public void Tras_vencer_el_bloqueo_el_conteo_empieza_de_cero()
    {
        var usuario = FallarVeces(5);

        usuario.RegistrarIntentoFallido(Ahora.AddMinutes(20));

        Assert.Equal(1, usuario.IntentosFallidos);
        Assert.False(usuario.EstaBloqueado(Ahora.AddMinutes(20)));
    }

    [Fact]
    public void Un_login_exitoso_o_el_desbloqueo_reinician_todo()
    {
        var usuario = FallarVeces(5);

        usuario.ReiniciarIntentos();

        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHastaUtc);
        Assert.False(usuario.EstaBloqueado(Ahora));
    }
}
