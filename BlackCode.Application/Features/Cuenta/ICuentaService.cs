using BlackCode.Application.Common;

namespace BlackCode.Application.Features.Cuenta;

public interface ICuentaService
{
    Task<ResultadoOperacion> CambiarContrasenaAsync(int usuarioId, string actual, string nueva, CancellationToken ct = default);
}
