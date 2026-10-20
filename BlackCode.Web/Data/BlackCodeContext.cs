using BlackCode.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Data;

public class BlackCodeContext(DbContextOptions<BlackCodeContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>().HasIndex(p => p.Name);

        modelBuilder.Entity<Order>(order =>
        {
            order.HasIndex(o => o.Code).IsUnique();
            order.HasIndex(o => o.Status);
            order.HasIndex(o => o.CreatedAt);
            // Los estados se guardan como texto para que sean legibles en MySQL.
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
            order.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            order.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
            order.HasMany(o => o.Payments).WithOne(p => p.Order).HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
            order.HasMany(o => o.History).WithOne(h => h.Order).HasForeignKey(h => h.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>(payment =>
        {
            payment.Property(p => p.Method).HasConversion<string>().HasMaxLength(30);
            payment.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<OrderStatusHistory>()
            .Property(h => h.Status).HasConversion<string>().HasMaxLength(30);
    }
}
