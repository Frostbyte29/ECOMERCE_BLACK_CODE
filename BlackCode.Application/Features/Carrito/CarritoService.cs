using BlackCode.Application.Abstractions;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
using BlackCode.Domain.Compras;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Application.Features.Carrito;
public sealed class CarritoService(IAppDbContext db, ICalculoCarritoService calculo, TimeProvider reloj) : ICarritoService
{
    private IQueryable<BlackCode.Domain.Compras.Carrito> Consulta(PropietarioCarrito p) {
        if (p.UsuarioId is int id) return db.Carritos.Where(c => c.UsuarioId == id);
        if (string.IsNullOrWhiteSpace(p.TokenAnonimo)) throw new DomainException("No se pudo identificar tu carrito.");
        return db.Carritos.Where(c => c.UsuarioId == null && c.TokenAnonimo == p.TokenAnonimo);
    }
    public async Task<CarritoDto> ObtenerAsync(PropietarioCarrito p) {
        var carrito = await Consulta(p).AsNoTracking().Include(c => c.Items).ThenInclude(i => i.Variante).ThenInclude(v => v.Producto).ThenInclude(p => p.Imagenes).SingleOrDefaultAsync();
        var lineas = carrito?.Items.Select(i => new LineaCarritoDto(i.VarianteId,i.Variante.ProductoId,i.Variante.Producto.Nombre,i.Variante.Talla,
            i.Variante.Producto.Imagenes.OrderBy(x => x.Orden).FirstOrDefault()?.Url ?? "/img/productos/anillo-profano.svg",
            i.Variante.Producto.Precio,i.Cantidad,i.Variante.Stock,i.Variante.Activo && i.Variante.Producto.Estado == EstadoProducto.Publicado)).ToList() ?? [];
        return new(lineas,await calculo.CalcularAsync(lineas));
    }
    public async Task AgregarAsync(PropietarioCarrito p, int varianteId, int cantidad) {
        if (cantidad < 1 || cantidad > 99) throw new DomainException("Elige una cantidad entre 1 y 99.");
        var variante = await db.VariantesProducto.Include(v => v.Producto).SingleOrDefaultAsync(v => v.Id == varianteId);
        if (variante is null || !variante.Activo || variante.Producto.Estado != EstadoProducto.Publicado) throw new DomainException("El producto ya no está disponible.");
        var carrito = await Consulta(p).Include(c => c.Items).SingleOrDefaultAsync();
        if (carrito is null) { carrito = new() { UsuarioId = p.UsuarioId, TokenAnonimo = p.UsuarioId.HasValue ? null : p.TokenAnonimo }; db.Carritos.Add(carrito); }
        var item = carrito.Items.SingleOrDefault(i => i.VarianteId == varianteId);
        var total = (item?.Cantidad ?? 0) + cantidad;
        if (total > variante.Stock || total > 99) throw new DomainException("La cantidad supera el stock disponible o el máximo de 99.");
        if (item is null) carrito.Items.Add(new ItemCarrito { VarianteId = varianteId, Cantidad = cantidad }); else item.Cantidad = total;
        carrito.ActualizadoUtc = reloj.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
    }
    public async Task ActualizarAsync(PropietarioCarrito p, int varianteId, int cantidad) {
        if (cantidad < 0 || cantidad > 99) throw new DomainException("Cantidad inválida.");
        var carrito = await Consulta(p).Include(c => c.Items).ThenInclude(i => i.Variante).ThenInclude(v => v.Producto).SingleOrDefaultAsync();
        var item = carrito?.Items.SingleOrDefault(i => i.VarianteId == varianteId);
        if (carrito is null || item is null) throw new DomainException("El artículo ya no está en tu carrito.");
        if (cantidad == 0) db.ItemsCarrito.Remove(item);
        else {
            if (!item.Variante.Activo || item.Variante.Producto.Estado != EstadoProducto.Publicado || cantidad > item.Variante.Stock) throw new DomainException("No hay stock suficiente para esa cantidad.");
            item.Cantidad = cantidad;
        }
        carrito.ActualizadoUtc = reloj.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
    }
    public async Task FusionarAsync(string tokenAnonimo, int usuarioId) {
        var anonimo = await db.Carritos.Include(c => c.Items).SingleOrDefaultAsync(c => c.UsuarioId == null && c.TokenAnonimo == tokenAnonimo);
        if (anonimo is null) return;
        var usuario = await db.Carritos.Include(c => c.Items).SingleOrDefaultAsync(c => c.UsuarioId == usuarioId);
        if (usuario is null) { anonimo.UsuarioId = usuarioId; anonimo.TokenAnonimo = null; anonimo.ActualizadoUtc = reloj.GetUtcNow().UtcDateTime; }
        else {
            foreach (var item in anonimo.Items) {
                var existente = usuario.Items.SingleOrDefault(i => i.VarianteId == item.VarianteId);
                if (existente is null) usuario.Items.Add(new ItemCarrito { VarianteId = item.VarianteId, Cantidad = item.Cantidad });
                else existente.Cantidad = Math.Min(99,existente.Cantidad + item.Cantidad);
            }
            usuario.ActualizadoUtc = reloj.GetUtcNow().UtcDateTime;
            db.Carritos.Remove(anonimo);
        }
        await db.SaveChangesAsync();
    }
}
