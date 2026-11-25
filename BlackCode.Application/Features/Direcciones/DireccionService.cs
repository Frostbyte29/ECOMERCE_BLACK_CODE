using BlackCode.Application.Abstractions;
using BlackCode.Domain.Common;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Application.Features.Direcciones;

/// <summary>
/// SRP: gestiona únicamente las direcciones de envío guardadas del cliente.
/// DIP: depende de IAppDbContext, no de la implementación concreta.
/// </summary>
public sealed class DireccionService(IAppDbContext db, TimeProvider reloj) : IDireccionService
{
    public async Task<IReadOnlyList<DireccionDto>> ListarAsync(int usuarioId)
        => await db.DireccionesGuardadas
            .AsNoTracking()
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.EsPredeterminada)
            .ThenByDescending(d => d.CreadoUtc)
            .Select(d => new DireccionDto(d.Id, d.NombreCompleto, d.Telefono, d.Direccion, d.Distrito, d.Referencia, d.EsPredeterminada))
            .ToListAsync();

    public async Task<DireccionDto?> ObtenerAsync(int id, int usuarioId)
    {
        var d = await db.DireccionesGuardadas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId);
        return d is null ? null : new DireccionDto(d.Id, d.NombreCompleto, d.Telefono, d.Direccion, d.Distrito, d.Referencia, d.EsPredeterminada);
    }

    public async Task<int> CrearAsync(int usuarioId, CrearDireccionDto dto)
    {
        if (dto.EsPredeterminada)
            await QuitarPredeterminadaAsync(usuarioId);

        var entidad = new DireccionGuardada
        {
            UsuarioId = usuarioId,
            NombreCompleto = dto.NombreCompleto.Trim(),
            Telefono = dto.Telefono.Trim(),
            Direccion = dto.Direccion.Trim(),
            Distrito = dto.Distrito.Trim(),
            Referencia = dto.Referencia?.Trim(),
            EsPredeterminada = dto.EsPredeterminada,
            CreadoUtc = reloj.GetUtcNow().UtcDateTime
        };
        db.DireccionesGuardadas.Add(entidad);
        await db.SaveChangesAsync();
        return entidad.Id;
    }

    public async Task ActualizarAsync(int id, int usuarioId, CrearDireccionDto dto)
    {
        var entidad = await db.DireccionesGuardadas
            .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId)
            ?? throw new DomainException("Dirección no encontrada.");

        if (dto.EsPredeterminada && !entidad.EsPredeterminada)
            await QuitarPredeterminadaAsync(usuarioId);

        entidad.NombreCompleto = dto.NombreCompleto.Trim();
        entidad.Telefono = dto.Telefono.Trim();
        entidad.Direccion = dto.Direccion.Trim();
        entidad.Distrito = dto.Distrito.Trim();
        entidad.Referencia = dto.Referencia?.Trim();
        entidad.EsPredeterminada = dto.EsPredeterminada;
        await db.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id, int usuarioId)
    {
        var entidad = await db.DireccionesGuardadas
            .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId)
            ?? throw new DomainException("Dirección no encontrada.");
        db.DireccionesGuardadas.Remove(entidad);
        await db.SaveChangesAsync();
    }

    private async Task QuitarPredeterminadaAsync(int usuarioId)
        => await db.DireccionesGuardadas
            .Where(d => d.UsuarioId == usuarioId && d.EsPredeterminada)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.EsPredeterminada, false));
}
