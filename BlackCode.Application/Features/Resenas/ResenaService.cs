using BlackCode.Application.Abstractions;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Resenas;

/// <summary>
/// SRP: gestiona únicamente las reseñas de productos.
/// DIP: depende de IAppDbContext, no de AppDbContext.
/// </summary>
public sealed class ResenaService(IAppDbContext db, TimeProvider reloj) : IResenaService
{
    public async Task<IReadOnlyList<ResenaDto>> ListarPorProductoAsync(int productoId)
    {
        // JOIN en SQL: una sola consulta, sin N+1. El formato del nombre se aplica en memoria.
        var datos = await db.Resenas
            .AsNoTracking()
            .Where(r => r.ProductoId == productoId)
            .OrderByDescending(r => r.CreadoUtc)
            .Join(db.Usuarios,
                r => r.UsuarioId,
                u => u.Id,
                (r, u) => new { r.Id, u.NombreCompleto, r.Calificacion, r.Comentario, r.CreadoUtc })
            .ToListAsync();

        return datos.Select(x => new ResenaDto(
            x.Id,
            FormatoAutor(x.NombreCompleto),
            x.Calificacion,
            x.Comentario,
            x.CreadoUtc)).ToList();
    }

    // "María P." — primer nombre + inicial del primer apellido; "Cliente" si el nombre viene vacío.
    private static string FormatoAutor(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "Cliente";
        var partes = nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length > 1 ? $"{partes[0]} {partes[1][0]}." : partes[0];
    }

    public Task<bool> YaResenoAsync(int usuarioId, int productoId)
        => db.Resenas.AnyAsync(r => r.UsuarioId == usuarioId && r.ProductoId == productoId);

    public async Task CrearAsync(int usuarioId, int productoId, int calificacion, string? comentario)
    {
        if (calificacion < 1 || calificacion > 5)
            throw new DomainException("La calificación debe estar entre 1 y 5.");

        if (await YaResenoAsync(usuarioId, productoId))
            throw new DomainException("Ya dejaste una reseña para este producto.");

        db.Resenas.Add(new Resena
        {
            ProductoId = productoId,
            UsuarioId = usuarioId,
            Calificacion = calificacion,
            Comentario = comentario?.Trim(),
            CreadoUtc = reloj.GetUtcNow().UtcDateTime
        });
        await db.SaveChangesAsync();
    }
}
