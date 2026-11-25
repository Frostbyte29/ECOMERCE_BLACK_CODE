using BlackCode.Application.Features.Resenas;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
using BlackCode.Domain.Usuarios;
using BlackCode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Tests.Resenas;

public sealed class ResenaServiceTests
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Crear_guarda_resena_con_datos_correctos()
    {
        await using var db = Db();
        await new ResenaService(db, TimeProvider.System).CrearAsync(1, 10, 5, "Excelente");
        var resena = await db.Resenas.SingleAsync();
        Assert.Equal(5, resena.Calificacion);
        Assert.Equal("Excelente", resena.Comentario);
        Assert.Equal(1, resena.UsuarioId);
        Assert.Equal(10, resena.ProductoId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Crear_lanza_excepcion_con_calificacion_fuera_de_rango(int calificacion)
    {
        await using var db = Db();
        await Assert.ThrowsAsync<DomainException>(
            () => new ResenaService(db, TimeProvider.System).CrearAsync(1, 10, calificacion, null));
    }

    [Fact]
    public async Task Crear_lanza_excepcion_si_usuario_ya_reseno_ese_producto()
    {
        await using var db = Db();
        db.Resenas.Add(new Resena { UsuarioId = 1, ProductoId = 10, Calificacion = 4, CreadoUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<DomainException>(
            () => new ResenaService(db, TimeProvider.System).CrearAsync(1, 10, 3, null));
    }

    [Fact]
    public async Task YaReseno_devuelve_false_para_otro_producto()
    {
        await using var db = Db();
        db.Resenas.Add(new Resena { UsuarioId = 1, ProductoId = 10, Calificacion = 5, CreadoUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var svc = new ResenaService(db, TimeProvider.System);
        Assert.True(await svc.YaResenoAsync(1, 10));
        Assert.False(await svc.YaResenoAsync(1, 99));
    }

    [Theory]
    [InlineData("María Perez Lopez", "María P.")]
    [InlineData("Juan Gomez", "Juan G.")]
    [InlineData("Carlos", "Carlos")]
    [InlineData("", "Cliente")]
    public async Task ListarPorProducto_formatea_autor_correctamente(string nombreCompleto, string esperado)
    {
        await using var db = Db();
        db.Usuarios.Add(new Usuario { Id = 1, NombreCompleto = nombreCompleto, Correo = "u@test.local" });
        db.Resenas.Add(new Resena { UsuarioId = 1, ProductoId = 10, Calificacion = 5, CreadoUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var lista = await new ResenaService(db, TimeProvider.System).ListarPorProductoAsync(10);
        Assert.Single(lista);
        Assert.Equal(esperado, lista[0].Autor);
    }
}
