using BlackCode.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlackCode.Infrastructure.Persistence.Configurations;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> b)
    {
        b.ToTable("categorias");
        b.Property(c => c.Id).HasColumnName("id");
        b.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
        b.Property(c => c.Slug).HasColumnName("slug").HasMaxLength(80).IsRequired();
        b.HasIndex(c => c.Slug).IsUnique();
        b.Property(c => c.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        b.Property(c => c.UrlImagen).HasColumnName("url_imagen").HasMaxLength(300);
        b.Property(c => c.Orden).HasColumnName("orden");
        b.Property(c => c.Activo).HasColumnName("activo");
    }
}

public sealed class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> b)
    {
        b.ToTable("materiales");
        b.Property(m => m.Id).HasColumnName("id");
        b.Property(m => m.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
        b.HasIndex(m => m.Nombre).IsUnique();
        b.Property(m => m.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
    }
}

public sealed class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> b)
    {
        b.ToTable("colores");
        b.Property(c => c.Id).HasColumnName("id");
        b.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(40).IsRequired();
        b.HasIndex(c => c.Nombre).IsUnique();
        b.Property(c => c.CodigoHex).HasColumnName("codigo_hex").HasMaxLength(7).IsRequired();
    }
}

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> b)
    {
        b.ToTable("productos");
        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
        b.Property(p => p.Slug).HasColumnName("slug").HasMaxLength(140).IsRequired();
        b.HasIndex(p => p.Slug).IsUnique();
        b.Property(p => p.Descripcion).HasColumnName("descripcion").HasMaxLength(2000).IsRequired();
        b.Property(p => p.Acabado).HasColumnName("acabado").HasMaxLength(80);
        b.Property(p => p.Precio).HasColumnName("precio").HasPrecision(10, 2);   // dinero: NUNCA float/double
        b.Property(p => p.Destacado).HasColumnName("destacado");
        b.Property(p => p.Estado).HasColumnName("estado");
        b.HasIndex(p => p.Estado);
        b.Property(p => p.CreadoUtc).HasColumnName("creado_utc");
        b.Property(p => p.CategoriaId).HasColumnName("categoria_id");
        b.Property(p => p.MaterialId).HasColumnName("material_id");
        b.Property(p => p.ColorId).HasColumnName("color_id");

        // Restrict: no se puede borrar una categoría/material/color que tenga productos
        b.HasOne(p => p.Categoria).WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.Material).WithMany()
            .HasForeignKey(p => p.MaterialId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.Color).WithMany()
            .HasForeignKey(p => p.ColorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class VarianteProductoConfiguration : IEntityTypeConfiguration<VarianteProducto>
{
    public void Configure(EntityTypeBuilder<VarianteProducto> b)
    {
        b.ToTable("variantes_producto");   // nombre usado en MySqlStockGateway: si lo cambias, cámbialo allá
        b.Property(v => v.Id).HasColumnName("id");
        b.Property(v => v.ProductoId).HasColumnName("producto_id");
        b.Property(v => v.Talla).HasColumnName("talla").HasMaxLength(20).IsRequired();
        b.Property(v => v.Sku).HasColumnName("sku").HasMaxLength(40).IsRequired();
        b.HasIndex(v => v.Sku).IsUnique();
        b.HasIndex(v => new { v.ProductoId, v.Talla }).IsUnique();   // no dos "talla 9" del mismo anillo
        b.Property(v => v.Stock).HasColumnName("stock");
        b.Property(v => v.Activo).HasColumnName("activo");

        b.HasOne(v => v.Producto).WithMany(p => p.Variantes)
            .HasForeignKey(v => v.ProductoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ImagenProductoConfiguration : IEntityTypeConfiguration<ImagenProducto>
{
    public void Configure(EntityTypeBuilder<ImagenProducto> b)
    {
        b.ToTable("imagenes_producto");
        b.Property(i => i.Id).HasColumnName("id");
        b.Property(i => i.ProductoId).HasColumnName("producto_id");
        b.Property(i => i.Url).HasColumnName("url").HasMaxLength(300).IsRequired();
        b.Property(i => i.Orden).HasColumnName("orden");

        b.HasOne(i => i.Producto).WithMany(p => p.Imagenes)
            .HasForeignKey(i => i.ProductoId).OnDelete(DeleteBehavior.Cascade);
    }
}
