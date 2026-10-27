using BlackCode.Application.Abstractions;
using BlackCode.Domain.Common;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Application.Features.Administracion;
public record ProductoReporteDto(int Id, string Nombre, decimal Precio, int Stock, string Estado);
public record ArchivoReporteDto(byte[] Contenido, string ContentType, string NombreArchivo);
public interface IExportadorReporte
{
    string Formato { get; }
    string ContentType { get; }
    byte[] Exportar(IReadOnlyList<ProductoReporteDto> productos);
}
public interface IReporteService
{
    Task<ArchivoReporteDto> ProductosAsync(string formato);
}
public sealed class ReporteService(IAppDbContext db, IUsuarioActual usuario, IEnumerable<IExportadorReporte> exportadores) : IReporteService
{
    public async Task<ArchivoReporteDto> ProductosAsync(string formato)
    {
        if (!usuario.EsAdmin) throw new DomainException("Acceso restringido.");
        var exportador = exportadores.SingleOrDefault(e => e.Formato == formato)
            ?? throw new DomainException("Formato de reporte no disponible.");
        var filas = await db.Productos.AsNoTracking().OrderBy(p => p.Id)
            .Select(p => new ProductoReporteDto(p.Id, p.Nombre, p.Precio, p.Variantes.Sum(v => v.Stock), p.Estado.ToString())).ToListAsync();
        return new(exportador.Exportar(filas), exportador.ContentType, "productos-blackcode." + exportador.Formato);
    }
}
