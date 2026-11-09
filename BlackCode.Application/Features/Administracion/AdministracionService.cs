using BlackCode.Application.Abstractions;
using BlackCode.Application.Features.Catalogo;
using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Common;
using Microsoft.EntityFrameworkCore;
namespace BlackCode.Application.Features.Administracion;
public record VarianteEdicionDto(int Id, string Talla, int Stock, bool Activo);
public record ProductoEdicionDto(int Id,string Nombre,string Slug,string Descripcion,decimal Precio,int CategoriaId,int ColorId,int MaterialId,bool Publicado,bool Destacado,string Imagen,IReadOnlyList<VarianteEdicionDto> Variantes);
public record AdministracionDto(int Productos,int Pedidos,int Clientes,int CarritosInvitados,decimal Ventas,IReadOnlyList<ProductoEdicionDto> Lista,IReadOnlyList<OpcionDto> Categorias,IReadOnlyList<OpcionDto> Colores,IReadOnlyList<OpcionDto> Materiales);
public interface IAdministracionService {
    Task<AdministracionDto> ConsultarAsync();
    Task GuardarAsync(ProductoEdicionDto producto);
    Task EliminarAsync(int id);
}
public sealed class AdministracionService(IAppDbContext db,IUsuarioActual usuario,TimeProvider reloj, IInventarioService inventario) : IAdministracionService {
    private void Verificar() { if (!usuario.EsAdmin) throw new DomainException("Acceso restringido."); }
    public async Task<AdministracionDto> ConsultarAsync() {
        Verificar();
        var lista = await db.Productos.AsNoTracking().Include(p => p.Variantes).Include(p => p.Imagenes).OrderByDescending(p => p.Id).ToListAsync();
        return new(lista.Count,await db.Pedidos.CountAsync(),await db.Usuarios.CountAsync(u => u.Rol == BlackCode.Domain.Usuarios.RolUsuario.Cliente),
            await db.Carritos.CountAsync(c => c.UsuarioId == null),await db.Pedidos.Where(p => p.Estado != BlackCode.Domain.Pedidos.EstadoPedido.Cancelado).SumAsync(p => (decimal?)p.Total) ?? 0,
            lista.Select(p => new ProductoEdicionDto(p.Id,p.Nombre,p.Slug,p.Descripcion,p.Precio,p.CategoriaId,p.ColorId,p.MaterialId,p.Estado == EstadoProducto.Publicado,p.Destacado,
                p.Imagenes.OrderBy(i => i.Orden).FirstOrDefault()?.Url ?? "/img/productos/anillo-profano.svg",
                p.Variantes.Select(v => new VarianteEdicionDto(v.Id,v.Talla,v.Stock,v.Activo)).ToList())).ToList(),
            await db.Categorias.Select(c => new OpcionDto(c.Id,c.Nombre)).ToListAsync(),await db.Colores.Select(c => new OpcionDto(c.Id,c.Nombre)).ToListAsync(),await db.Materiales.Select(c => new OpcionDto(c.Id,c.Nombre)).ToListAsync());
    }
    public async Task GuardarAsync(ProductoEdicionDto dto) {
        Verificar();
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Length > 120 || string.IsNullOrWhiteSpace(dto.Slug) || dto.Slug.Length > 140 || !System.Text.RegularExpressions.Regex.IsMatch(dto.Slug,"^[a-z0-9]+(?:-[a-z0-9]+)*$") ||
            string.IsNullOrWhiteSpace(dto.Descripcion) || dto.Descripcion.Length > 2000 || dto.Precio <= 0 || dto.Precio > 99999999m)
            throw new DomainException("Revisa los datos: nombre, descripción, precio positivo e identificador válido.");
        if (!dto.Imagen.StartsWith("/img/productos/") || dto.Imagen.Contains("..") || dto.Imagen.Length > 300) throw new DomainException("Selecciona una imagen del catálogo local.");
        if (await db.Productos.AnyAsync(p => p.Slug == dto.Slug && p.Id != dto.Id)) throw new DomainException("Ese identificador ya pertenece a otro producto.");
        if (!await db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId) || !await db.Colores.AnyAsync(c => c.Id == dto.ColorId) || !await db.Materiales.AnyAsync(m => m.Id == dto.MaterialId)) throw new DomainException("Categoría, material o color inválido.");
        var p = dto.Id == 0 ? new Producto { CreadoUtc = reloj.GetUtcNow().UtcDateTime } :
            await db.Productos.Include(p => p.Variantes).Include(p => p.Imagenes).SingleOrDefaultAsync(p => p.Id == dto.Id) ?? throw new DomainException("Producto no encontrado.");
        p.Nombre = dto.Nombre.Trim(); p.Slug = dto.Slug; p.Descripcion = dto.Descripcion.Trim(); p.Precio = Math.Round(dto.Precio,2);
        p.CategoriaId = dto.CategoriaId; p.ColorId = dto.ColorId; p.MaterialId = dto.MaterialId;
        p.Estado = dto.Publicado ? EstadoProducto.Publicado : EstadoProducto.Borrador; p.Destacado = dto.Destacado;
        inventario.Actualizar(p, dto.Variantes, dto.Id == 0);
        var imagen = p.Imagenes.OrderBy(i => i.Orden).FirstOrDefault();
        if (imagen is null) p.Imagenes.Add(new ImagenProducto { Url = dto.Imagen, Orden = 1 }); else imagen.Url = dto.Imagen;
        if (dto.Id == 0) db.Productos.Add(p);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { throw new DomainException("No se pudo guardar. Comprueba que el slug y las tallas no estén repetidos."); }
    }
    public async Task EliminarAsync(int id) {
        Verificar();
        var p = await db.Productos.Include(p => p.Variantes).SingleOrDefaultAsync(p => p.Id == id) ?? throw new DomainException("Producto no encontrado.");
        if (await db.ItemsPedido.AnyAsync(i => p.Variantes.Select(v => v.Id).Contains(i.VarianteId))) {
            p.Estado = EstadoProducto.Borrador; // Conserva referencias históricas.
        } else db.Productos.Remove(p);
        await db.SaveChangesAsync();
    }
}
