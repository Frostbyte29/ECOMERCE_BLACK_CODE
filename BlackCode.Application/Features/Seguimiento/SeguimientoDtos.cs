namespace BlackCode.Application.Features.Seguimiento;

public record EventoSeguimientoDto(string Estado, DateTime Fecha);
public record SeguimientoDto(
    string Codigo,
    string EstadoActual,
    IReadOnlyList<EventoSeguimientoDto> Historial,
    string? Transportista,
    string? CodigoRastreo);

public interface ISeguimientoService
{
    Task<SeguimientoDto?> ObtenerPorCodigoAsync(string codigo);
}
