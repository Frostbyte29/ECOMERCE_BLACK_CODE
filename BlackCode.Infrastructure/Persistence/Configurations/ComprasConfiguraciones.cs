using BlackCode.Domain.Compras;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlackCode.Infrastructure.Persistence.Configurations;

public sealed class FavoritoConfiguration : IEntityTypeConfiguration<Favorito>
{
    public void Configure(EntityTypeBuilder<Favorito> b)
    {
        b.ToTable("favoritos");
        b.HasKey(f => new { f.UsuarioId, f.ProductoId });   // un producto se marca favorito una sola vez
        b.Property(f => f.UsuarioId).HasColumnName("usuario_id");
        b.Property(f => f.ProductoId).HasColumnName("producto_id");
        b.Property(f => f.CreadoUtc).HasColumnName("creado_utc");

        b.HasOne<Usuario>().WithMany()
            .HasForeignKey(f => f.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(f => f.Producto).WithMany()
            .HasForeignKey(f => f.ProductoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CarritoConfiguration : IEntityTypeConfiguration<Carrito>
{
    public void Configure(EntityTypeBuilder<Carrito> b)
    {
        b.ToTable("carritos");
        b.Property(c => c.Id).HasColumnName("id");
        b.Property(c => c.UsuarioId).HasColumnName("usuario_id");
        b.Property(c => c.TokenAnonimo).HasColumnName("token_anonimo").HasMaxLength(64);
        b.Property(c => c.ActualizadoUtc).HasColumnName("actualizado_utc");
        b.HasIndex(c => c.UsuarioId).IsUnique();          // MySQL permite varios NULL en un índice único
        b.HasIndex(c => c.TokenAnonimo).IsUnique();

        b.HasOne<Usuario>().WithMany()
            .HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ItemCarritoConfiguration : IEntityTypeConfiguration<ItemCarrito>
{
    public void Configure(EntityTypeBuilder<ItemCarrito> b)
    {
        b.ToTable("items_carrito");
        b.Property(i => i.Id).HasColumnName("id");
        b.Property(i => i.CarritoId).HasColumnName("carrito_id");
        b.Property(i => i.VarianteId).HasColumnName("variante_id");
        b.Property(i => i.Cantidad).HasColumnName("cantidad");
        b.HasIndex(i => new { i.CarritoId, i.VarianteId }).IsUnique();   // misma talla = misma línea, se suma cantidad

        b.HasOne(i => i.Carrito).WithMany(c => c.Items)
            .HasForeignKey(i => i.CarritoId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(i => i.Variante).WithMany()
            .HasForeignKey(i => i.VarianteId).OnDelete(DeleteBehavior.Cascade);
    }
}
