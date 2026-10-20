using BlackCode.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(BlackCodeContext db)
    {
        if (await db.Products.AnyAsync()) return;

        db.Products.AddRange(
            new Product { Id = 1, Name = "Anillo Obsidiana", Price = 49.90m, Category = "Anillos", Description = "Diseño sobrio de líneas definidas. Una pieza versátil para el día a día.", Material = "Acero inoxidable 316L", Finish = "Negro mate", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/anillo-obsidiana.jpg" },
            new Product { Id = 2, Name = "Cadena Atlas", Price = 69.90m, Category = "Cadenas", Description = "Cadena de presencia discreta y acabado pulido, pensada para combinar con cualquier estilo.", Material = "Acero inoxidable 316L", Finish = "Plateado pulido", SizesCsv = "Única", ImageUrl = "/images/products/cadena-atlas.jpg" },
            new Product { Id = 3, Name = "Pulsera Nox", Price = 59.90m, Category = "Pulseras", Description = "Una silueta minimalista con carácter urbano y cierre práctico.", Material = "Acero inoxidable 316L", Finish = "Negro y plateado", SizesCsv = "Única", ImageUrl = "/images/products/pulsera-nox.jpg" },
            new Product { Id = 4, Name = "Collar Eclipse", Price = 74.90m, Category = "Collares", Description = "Un colgante de estética limpia que aporta un detalle sutil al conjunto.", Material = "Acero inoxidable 316L", Finish = "Negro satinado", SizesCsv = "Única", ImageUrl = "/images/products/collar-eclipse.jpg" },
            new Product { Id = 5, Name = "Anillo Monolito", Price = 54.90m, Category = "Anillos", Description = "Perfil geométrico y acabado metálico para un estilo atemporal.", Material = "Acero inoxidable 316L", Finish = "Plateado cepillado", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/anillo-monolito.jpg" }
        );

        await db.SaveChangesAsync();
    }

    // Crea los roles y un administrador de prueba (datos en appsettings.json → "SeedAdmin").
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { Roles.Admin, Roles.Cliente })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = email, Email = email, FullName = "Administrador", EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded) return;
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
            await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}
