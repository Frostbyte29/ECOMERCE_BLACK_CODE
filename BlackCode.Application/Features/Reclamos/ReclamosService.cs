using BlackCode.Application.Abstractions;
using BlackCode.Domain.Reclamos;

namespace BlackCode.Application.Features.Reclamos;

/// <summary>
/// SRP: registra reclamos o quejas en el libro de reclamaciones.
/// DIP: depende de IAppDbContext, no de AppDbContext.
/// </summary>
public sealed class ReclamosService(IAppDbContext db, TimeProvider reloj) : IReclamosService
{
    public async Task<int> RegistrarAsync(int? usuarioId, ReclamoDto dto)
    {
        var entidad = new Reclamo
        {
            UsuarioId = usuarioId,
            NombresCompletos = dto.NombresCompletos.Trim(),
            Documento = dto.Documento.Trim(),
            Domicilio = dto.Domicilio.Trim(),
            Correo = dto.Correo.Trim(),
            Telefono = dto.Telefono?.Trim(),
            NumeroPedido = dto.NumeroPedido?.Trim().ToUpperInvariant(),
            Tipo = dto.Tipo,
            DescripcionBien = dto.DescripcionBien.Trim(),
            Detalle = dto.Detalle.Trim(),
            PedidoCliente = dto.PedidoCliente.Trim(),
            CreadoUtc = reloj.GetUtcNow().UtcDateTime
        };
        db.Reclamos.Add(entidad);
        await db.SaveChangesAsync();
        return entidad.Id;
    }
}
