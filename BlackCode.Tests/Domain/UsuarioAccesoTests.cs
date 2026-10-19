using BlackCode.Domain.Usuarios;
using Xunit;

namespace BlackCode.Tests.Domain;

public class UsuarioAccesoTests
{
    private static readonly DateTime Ahora = new(2026, 10, 20, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void UltimoAccesoUtc_empieza_sin_valor()
    {
        var usuario = new Usuario();

        Assert.Null(usuario.UltimoAccesoUtc);
    }

    [Fact]
    public void RegistrarAcceso_asigna_UltimoAccesoUtc()
    {
        var usuario = new Usuario();

        usuario.RegistrarAcceso(Ahora);

        Assert.Equal(Ahora, usuario.UltimoAccesoUtc);
    }

    [Fact]
    public void RegistrarAcceso_sobreescribe_un_acceso_previo()
    {
        var usuario = new Usuario();
        usuario.RegistrarAcceso(Ahora);

        var segundo = Ahora.AddHours(2);
        usuario.RegistrarAcceso(segundo);

        Assert.Equal(segundo, usuario.UltimoAccesoUtc);
    }
}
