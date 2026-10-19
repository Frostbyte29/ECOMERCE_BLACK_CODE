using BlackCode.Application.Common;

namespace BlackCode.Application.Features.Clientes;

public interface IGestionClientesService
{
    Task<ResultadoPaginado<ClienteResumen>> ListarAsync(FiltroClientes filtro, CancellationToken ct = default);
    Task<ResultadoOperacion> CambiarEstadoAsync(int clienteId, bool activo, CancellationToken ct = default);
}
