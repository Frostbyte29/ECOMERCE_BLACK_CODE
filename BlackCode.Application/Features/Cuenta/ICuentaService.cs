using BlackCode.Application.Common;

namespace BlackCode.Application.Features.Cuenta;

public record DatosClienteDto(string NombreCompleto, string? Telefono);

public interface ICuentaService
{
    Task<ResultadoOperacion> CambiarContrasenaAsync(int usuarioId, string actual, string nueva, CancellationToken ct = default);
    Task<DatosClienteDto?> ObtenerDatosAsync(int usuarioId, CancellationToken ct = default);
    Task<ResultadoOperacion> ActualizarDatosAsync(int usuarioId, string nombreCompleto, string? telefono, CancellationToken ct = default);
}
