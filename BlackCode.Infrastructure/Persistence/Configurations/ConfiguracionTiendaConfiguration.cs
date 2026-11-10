using BlackCode.Domain.Configuracion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlackCode.Infrastructure.Persistence.Configurations;

public sealed class ConfiguracionTiendaConfiguration : IEntityTypeConfiguration<ConfiguracionTienda>
{
    public void Configure(EntityTypeBuilder<ConfiguracionTienda> b)
    {
        b.ToTable("configuracion_tienda");
        b.HasKey(s => s.Clave);
        b.Property(s => s.Clave).HasColumnName("clave").HasMaxLength(80);
        b.Property(s => s.Valor).HasColumnName("valor").HasMaxLength(1000).IsRequired();
    }
}
