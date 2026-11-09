using BlackCode.Application.Abstractions;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
namespace BlackCode.Application.Features.Administracion;
public interface IInventarioService
{
    void Actualizar(Producto producto, IReadOnlyList<VarianteEdicionDto> variantes, bool nuevo);
}
public sealed class InventarioService(IUsuarioActual usuario) : IInventarioService
{
    public void Actualizar(Producto producto, IReadOnlyList<VarianteEdicionDto> variantes, bool nuevo)
    {
        if (!usuario.EsAdmin) throw new DomainException("Acceso restringido.");
        if (variantes.Count == 0 || variantes.Any(v => string.IsNullOrWhiteSpace(v.Talla)
            || v.Talla.Length > 20 || v.Stock < 0 || v.Stock > 100000)
            || variantes.Select(v => v.Talla.Trim().ToLowerInvariant()).Distinct().Count() != variantes.Count)
            throw new DomainException("Usa tallas únicas y existencias entre 0 y 100000.");
        if (!nuevo && (producto.Variantes.Count != variantes.Count
            || variantes.Any(v => producto.Variantes.All(actual => actual.Id != v.Id))))
            throw new DomainException("Las variantes cambiaron. Recarga la página.");
        foreach (var entrada in variantes)
        {
            var variante = nuevo ? new VarianteProducto { Sku = Guid.NewGuid().ToString("N") }
                : producto.Variantes.Single(v => v.Id == entrada.Id);
            variante.Talla = entrada.Talla.Trim();
            variante.Stock = entrada.Stock;
            variante.Activo = entrada.Activo;
            if (nuevo) producto.Variantes.Add(variante);
        }
    }
}
