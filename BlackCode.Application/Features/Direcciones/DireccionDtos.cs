using System.ComponentModel.DataAnnotations;

namespace BlackCode.Application.Features.Direcciones;

public record DireccionDto(int Id, string NombreCompleto, string Telefono, string Direccion, string Distrito, string? Referencia, bool EsPredeterminada);

public class CrearDireccionDto
{
    [Required, StringLength(120)] public string NombreCompleto { get; set; } = "";
    [Required, StringLength(20), RegularExpression(@"^\d{9}$")] public string Telefono { get; set; } = "";
    [Required, StringLength(200)] public string Direccion { get; set; } = "";
    [Required, StringLength(80)] public string Distrito { get; set; } = "";
    [StringLength(200)] public string? Referencia { get; set; }
    public bool EsPredeterminada { get; set; }
}

public interface IDireccionService
{
    Task<IReadOnlyList<DireccionDto>> ListarAsync(int usuarioId);
    Task<DireccionDto?> ObtenerAsync(int id, int usuarioId);
    Task<int> CrearAsync(int usuarioId, CrearDireccionDto dto);
    Task ActualizarAsync(int id, int usuarioId, CrearDireccionDto dto);
    Task EliminarAsync(int id, int usuarioId);
}
