using BlackCode.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Data;

public class BlackCodeContext(DbContextOptions<BlackCodeContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>().HasIndex(p => p.Name);
    }
}
