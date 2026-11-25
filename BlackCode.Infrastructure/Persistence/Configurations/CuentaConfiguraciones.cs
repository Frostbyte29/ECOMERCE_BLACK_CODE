using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlackCode.Infrastructure.Persistence.Configurations;

public sealed class DireccionGuardadaConfiguration : IEntityTypeConfiguration<DireccionGuardada>
{
    public void Configure(EntityTypeBuilder<DireccionGuardada> b)
    {
        b.ToTable("direcciones_guardadas");
        b.Property(d => d.Id).HasColumnName("id");
        b.Property(d => d.UsuarioId).HasColumnName("usuario_id");
        b.HasIndex(d => d.UsuarioId);
        b.Property(d => d.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(120).IsRequired();
        b.Property(d => d.Telefono).HasColumnName("telefono").HasMaxLength(20).IsRequired();
        b.Property(d => d.Direccion).HasColumnName("direccion").HasMaxLength(200).IsRequired();
        b.Property(d => d.Distrito).HasColumnName("distrito").HasMaxLength(80).IsRequired();
        b.Property(d => d.Referencia).HasColumnName("referencia").HasMaxLength(200);
        b.Property(d => d.EsPredeterminada).HasColumnName("es_predeterminada");
        b.Property(d => d.CreadoUtc).HasColumnName("creado_utc");

        b.HasOne<Usuario>().WithMany()
            .HasForeignKey(d => d.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
