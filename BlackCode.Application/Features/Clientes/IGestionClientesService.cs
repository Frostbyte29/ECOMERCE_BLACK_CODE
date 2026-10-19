using BlackCode.Application.Common;

namespace BlackCode.Application.Features.Clientes;

public interface IGestionClientesService
{
    Task<ResultadoPaginado<ClienteResumen>> ListarAsync(FiltroClientes filtro, CancellationToken ct = default);
    Task<ClienteResumen?> ObtenerAsync(int clienteId, CancellationToken ct = default);
    Task<ResultadoOperacion> CrearAsync(SolicitudCrearCliente solicitud, CancellationToken ct = default);
    Task<ResultadoOperacion> ActualizarClienteAsync(SolicitudActualizarCliente solicitud, CancellationToken ct = default);
}
