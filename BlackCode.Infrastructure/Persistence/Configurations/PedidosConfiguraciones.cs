using BlackCode.Domain.Catalogo;
using BlackCode.Domain.Pedidos;
using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlackCode.Infrastructure.Persistence.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> b)
    {
        b.ToTable("pedidos");
        b.Property(o => o.Id).HasColumnName("id");
        b.Property(o => o.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        b.HasIndex(o => o.Codigo).IsUnique();
        b.Property(o => o.UsuarioId).HasColumnName("usuario_id");
        b.HasIndex(o => o.UsuarioId);
        b.Property(o => o.Estado).HasColumnName("estado");

        b.Property(o => o.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2);
        b.Property(o => o.CostoEnvio).HasColumnName("costo_envio").HasPrecision(10, 2);
        b.Property(o => o.Total).HasColumnName("total").HasPrecision(10, 2);

        b.Property(o => o.EnvioNombreCompleto).HasColumnName("envio_nombre_completo").HasMaxLength(120).IsRequired();
        b.Property(o => o.EnvioTelefono).HasColumnName("envio_telefono").HasMaxLength(20).IsRequired();
        b.Property(o => o.EnvioDireccion).HasColumnName("envio_direccion").HasMaxLength(200).IsRequired();
        b.Property(o => o.EnvioDistrito).HasColumnName("envio_distrito").HasMaxLength(80).IsRequired();
        b.Property(o => o.EnvioReferencia).HasColumnName("envio_referencia").HasMaxLength(200);
        b.Property(o => o.CreadoUtc).HasColumnName("creado_utc");

        // Restrict: no se borra un usuario que tiene pedidos (los pedidos son historia)
        b.HasOne<Usuario>().WithMany()
            .HasForeignKey(o => o.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> b)
    {
        b.ToTable("items_pedido");
        b.Property(i => i.Id).HasColumnName("id");
        b.Property(i => i.PedidoId).HasColumnName("pedido_id");
        b.Property(i => i.VarianteId).HasColumnName("variante_id");
        b.Property(i => i.NombreProductoCopia).HasColumnName("nombre_producto_copia").HasMaxLength(120).IsRequired();
        b.Property(i => i.TallaCopia).HasColumnName("talla_copia").HasMaxLength(20).IsRequired();
        b.Property(i => i.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(10, 2);
        b.Property(i => i.Cantidad).HasColumnName("cantidad");
        b.Property(i => i.TotalLinea).HasColumnName("total_linea").HasPrecision(10, 2);

        b.HasOne(i => i.Pedido).WithMany(o => o.Items)
            .HasForeignKey(i => i.PedidoId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<VarianteProducto>().WithMany()
            .HasForeignKey(i => i.VarianteId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class HistorialEstadoPedidoConfiguration : IEntityTypeConfiguration<HistorialEstadoPedido>
{
    public void Configure(EntityTypeBuilder<HistorialEstadoPedido> b)
    {
        b.ToTable("historial_estado_pedido");
        b.Property(h => h.Id).HasColumnName("id");
        b.Property(h => h.PedidoId).HasColumnName("pedido_id");
        b.Property(h => h.EstadoAnterior).HasColumnName("estado_anterior");
        b.Property(h => h.EstadoNuevo).HasColumnName("estado_nuevo");
        b.Property(h => h.CambiadoPorUsuarioId).HasColumnName("cambiado_por_usuario_id");
        b.Property(h => h.CambiadoUtc).HasColumnName("cambiado_utc");
        b.Property(h => h.Nota).HasColumnName("nota").HasMaxLength(300);

        b.HasOne(h => h.Pedido).WithMany(o => o.Historial)
            .HasForeignKey(h => h.PedidoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> b)
    {
        b.ToTable("pagos");
        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.PedidoId).HasColumnName("pedido_id");
        b.Property(p => p.Metodo).HasColumnName("metodo");
        b.Property(p => p.Estado).HasColumnName("estado");
        b.Property(p => p.Monto).HasColumnName("monto").HasPrecision(10, 2);
        b.Property(p => p.Referencia).HasColumnName("referencia").HasMaxLength(80).IsRequired();
        b.Property(p => p.CreadoUtc).HasColumnName("creado_utc");
        b.HasIndex(p => p.PedidoId).IsUnique();

        b.HasOne(p => p.Pedido).WithOne(o => o.Pago)
            .HasForeignKey<Pago>(p => p.PedidoId).OnDelete(DeleteBehavior.Cascade);
    }
}
