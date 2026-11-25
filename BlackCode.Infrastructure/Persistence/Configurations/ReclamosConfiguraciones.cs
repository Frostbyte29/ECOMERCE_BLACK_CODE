using BlackCode.Domain.Reclamos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlackCode.Infrastructure.Persistence.Configurations;

public sealed class ReclamoConfiguration : IEntityTypeConfiguration<Reclamo>
{
    public void Configure(EntityTypeBuilder<Reclamo> b)
    {
        b.ToTable("reclamos");
        b.Property(r => r.Id).HasColumnName("id");
        b.Property(r => r.UsuarioId).HasColumnName("usuario_id");
        b.HasIndex(r => r.UsuarioId);
        b.Property(r => r.NombresCompletos).HasColumnName("nombres_completos").HasMaxLength(120).IsRequired();
        b.Property(r => r.Documento).HasColumnName("documento").HasMaxLength(20).IsRequired();
        b.Property(r => r.Domicilio).HasColumnName("domicilio").HasMaxLength(200).IsRequired();
        b.Property(r => r.Correo).HasColumnName("correo").HasMaxLength(150).IsRequired();
        b.Property(r => r.Telefono).HasColumnName("telefono").HasMaxLength(20);
        b.Property(r => r.NumeroPedido).HasColumnName("numero_pedido").HasMaxLength(20);
        b.Property(r => r.Tipo).HasColumnName("tipo");
        b.Property(r => r.DescripcionBien).HasColumnName("descripcion_bien").HasMaxLength(300).IsRequired();
        b.Property(r => r.Detalle).HasColumnName("detalle").HasMaxLength(1000).IsRequired();
        b.Property(r => r.PedidoCliente).HasColumnName("pedido_cliente").HasMaxLength(1000).IsRequired();
        b.Property(r => r.CreadoUtc).HasColumnName("creado_utc");
    }
}
