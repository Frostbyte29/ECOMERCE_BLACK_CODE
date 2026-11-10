using BlackCode.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlackCode.Infrastructure.Persistence.Configurations;

// Regla de MySQL: todo texto con índice NECESITA HasMaxLength (sin largo, MySQL no puede indexarlo).
public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuarios");
        b.Property(u => u.Id).HasColumnName("id");
        b.Property(u => u.Correo).HasColumnName("correo").HasMaxLength(254).IsRequired();
        b.HasIndex(u => u.Correo).IsUnique();
        b.Property(u => u.HashContrasena).HasColumnName("hash_contrasena").HasMaxLength(512).IsRequired();
        b.Property(u => u.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(120).IsRequired();
        b.Property(u => u.Telefono).HasColumnName("telefono").HasMaxLength(20);
        b.Property(u => u.Rol).HasColumnName("rol");
        b.Property(u => u.Activo).HasColumnName("activo");
        b.Property(u => u.IntentosFallidos).HasColumnName("intentos_fallidos");
        b.Property(u => u.BloqueadoHastaUtc).HasColumnName("bloqueado_hasta_utc");
        b.Property(u => u.UltimoAccesoUtc).HasColumnName("ultimo_acceso_utc");
        b.Property(u => u.TerminosAceptadosUtc).HasColumnName("terminos_aceptados_utc");
        b.Property(u => u.CreadoUtc).HasColumnName("creado_utc");
    }
}
