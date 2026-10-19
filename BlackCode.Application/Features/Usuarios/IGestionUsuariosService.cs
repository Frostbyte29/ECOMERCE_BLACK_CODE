using BlackCode.Application.Common;

namespace BlackCode.Application.Features.Usuarios;

public interface IGestionUsuariosService
{
    Task<ResultadoPaginado<UsuarioResumen>> ListarAsync(FiltroUsuarios filtro, CancellationToken ct = default);
    Task<ResultadoOperacion> CambiarEstadoAsync(int usuarioId, bool activo, CancellationToken ct = default);
    Task<ResultadoOperacion> DesbloquearAsync(int usuarioId, CancellationToken ct = default);
}
