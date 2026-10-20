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
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<SavedAddress> SavedAddresses => Set<SavedAddress>();
    public DbSet<ProductInventory> ProductInventories => Set<ProductInventory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>().HasIndex(p => p.Name);

        modelBuilder.Entity<Favorite>(favorite =>
        {
            favorite.HasIndex(f => new { f.UserId, f.ProductId }).IsUnique();
            favorite.HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            favorite.HasOne(f => f.Product).WithMany().HasForeignKey(f => f.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SavedAddress>(address =>
        {
            address.HasIndex(a => a.UserId);
            address.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductInventory>(inventory =>
        {
            inventory.HasKey(i => i.ProductId);
            inventory.HasOne(i => i.Product).WithOne().HasForeignKey<ProductInventory>(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
            inventory.HasMany(i => i.Movements).WithOne(m => m.Inventory).HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StockMovement>(movement =>
        {
            movement.HasIndex(m => m.ChangedAt);
            movement.HasOne(m => m.ChangedByUser).WithMany().HasForeignKey(m => m.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(order =>
        {
            order.HasIndex(o => o.Code).IsUnique();
            order.HasIndex(o => o.CheckoutToken).IsUnique();
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
