using BlackCode.Domain.Reclamos;

namespace BlackCode.Application.Features.Reclamos;

public record ReclamoDto(
    string NombresCompletos,
    string Documento,
    string Domicilio,
    string Correo,
    string? Telefono,
    string? NumeroPedido,
    TipoReclamo Tipo,
    string DescripcionBien,
    string Detalle,
    string PedidoCliente);

public interface IReclamosService
{
    Task<int> RegistrarAsync(int? usuarioId, ReclamoDto dto);
}
