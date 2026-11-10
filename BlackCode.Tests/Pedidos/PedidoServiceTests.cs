using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Pedidos;
using BlackCode.Domain.Pedidos;
using BlackCode.Domain.Usuarios;
using BlackCode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Tests.Pedidos;
public sealed class PedidoServiceTests
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static PedidoService Servicio(AppDbContext db, int usuarioId) =>
        new(db, new FakeUsuario(usuarioId), null!, null!, null!, TimeProvider.System);
    [Fact]
    public async Task ObtenerUltimoEnvio_SinPedidos_RetornaNull()
    {
        await using var db = Db();
        var resultado = await Servicio(db, 1).ObtenerUltimoEnvioAsync();
        Assert.Null(resultado);
    }
    [Fact]
    public async Task ObtenerUltimoEnvio_ConDosPedidos_RetornaElMasReciente()
    {
        await using var db = Db();
        db.Pedidos.AddRange(
            new Pedido { Codigo="BC-T01", UsuarioId=1, CreadoUtc=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), EnvioNombreCompleto="Antiguo", EnvioTelefono="900000001", EnvioDireccion="Jr. Viejo 1", EnvioDistrito="Miraflores" },
            new Pedido { Codigo="BC-T02", UsuarioId=1, CreadoUtc=new DateTime(2026,6,1,0,0,0,DateTimeKind.Utc), EnvioNombreCompleto="Reciente", EnvioTelefono="900000002", EnvioDireccion="Jr. Nuevo 2", EnvioDistrito="San Isidro" }
        );
        await db.SaveChangesAsync();
        var resultado = await Servicio(db, 1).ObtenerUltimoEnvioAsync();
        Assert.NotNull(resultado);
        Assert.Equal("Reciente", resultado.Nombre);
        Assert.Equal("900000002", resultado.Telefono);
    }
    [Fact]
    public async Task ObtenerUltimoEnvio_SinPedidosConTelefono_RetornaDtoConSoloTelefono()
    {
        await using var db = Db();
        var u = new Usuario { Correo="cliente@test.com", NombreCompleto="Test", Telefono="987654321", TerminosAceptadosUtc=DateTime.UtcNow, CreadoUtc=DateTime.UtcNow };
        db.Usuarios.Add(u);
        await db.SaveChangesAsync();
        var resultado = await Servicio(db, u.Id).ObtenerUltimoEnvioAsync();
        Assert.NotNull(resultado);
        Assert.Equal("987654321", resultado.Telefono);
        Assert.Empty(resultado.Nombre);
        Assert.Empty(resultado.Direccion);
        Assert.Empty(resultado.Distrito);
        Assert.Null(resultado.Referencia);
    }
    private sealed class FakeUsuario(int id) : IUsuarioActual
    {
        public int? UsuarioId => id;
        public bool EstaAutenticado => true;
        public bool EsAdmin => false;
    }
}
