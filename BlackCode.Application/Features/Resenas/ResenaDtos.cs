namespace BlackCode.Application.Features.Resenas;

public record ResenaDto(int Id, string Autor, int Calificacion, string? Comentario, DateTime Fecha);

public interface IResenaService
{
    Task<IReadOnlyList<ResenaDto>> ListarPorProductoAsync(int productoId);
    Task<bool> YaResenoAsync(int usuarioId, int productoId);
    Task CrearAsync(int usuarioId, int productoId, int calificacion, string? comentario);
}
