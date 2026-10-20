using BlackCode.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(BlackCodeContext db)
    {
        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
            new Product { Id = 1, Name = "Anillo Obsidiana", Price = 49.90m, Category = "Anillos", Description = "Diseño sobrio de líneas definidas. Una pieza versátil para el día a día.", Material = "Acero inoxidable 316L", Finish = "Negro mate", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/anillo-obsidiana.jpg" },
            new Product { Id = 2, Name = "Cadena Atlas", Price = 69.90m, Category = "Cadenas", Description = "Cadena de presencia discreta y acabado pulido, pensada para combinar con cualquier estilo.", Material = "Acero inoxidable 316L", Finish = "Plateado pulido", SizesCsv = "Única", ImageUrl = "/images/products/cadena-atlas.jpg" },
            new Product { Id = 3, Name = "Pulsera Nox", Price = 59.90m, Category = "Pulseras", Description = "Una silueta minimalista con carácter urbano y cierre práctico.", Material = "Acero inoxidable 316L", Finish = "Negro y plateado", SizesCsv = "Única", ImageUrl = "/images/products/pulsera-nox.jpg" },
            new Product { Id = 4, Name = "Collar Eclipse", Price = 74.90m, Category = "Collares", Description = "Un colgante de estética limpia que aporta un detalle sutil al conjunto.", Material = "Acero inoxidable 316L", Finish = "Negro satinado", SizesCsv = "Única", ImageUrl = "/images/products/collar-eclipse.jpg" },
            new Product { Id = 5, Name = "Anillo Monolito", Price = 54.90m, Category = "Anillos", Description = "Perfil geométrico y acabado metálico para un estilo atemporal.", Material = "Acero inoxidable 316L", Finish = "Plateado cepillado", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/anillo-monolito.jpg" }
            );
            await db.SaveChangesAsync();
        }

        // Productos de demostración del prototipo de Angelo. No cambia los productos ya guardados.
        var demoProducts = new[]
        {
            new Product { Name = "Anillo Profano", Price = 39.00m, Category = "Anillos", Description = "Diseño sólido y atemporal que refleja carácter. Anillo Profano combina líneas geométricas y un acabado envejecido que resalta su esencia rebelde y única.", Material = "Acero inoxidable 316L", Finish = "Plateado", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/angelo/anillo-profano.svg" },
            new Product { Name = "Anillo Aureo", Price = 55.00m, Category = "Anillos", Description = "Anillo de acero inoxidable 316L.", Material = "Acero inoxidable 316L", Finish = "Negro", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/angelo/anillo-aureo.svg" },
            new Product { Name = "Anillo Bruma", Price = 49.00m, Category = "Anillos", Description = "Anillo de acero inoxidable 316L.", Material = "Acero inoxidable 316L", Finish = "Negro", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/angelo/anillo-bruma.svg" },
            new Product { Name = "Anillo Tizzo", Price = 45.00m, Category = "Anillos", Description = "Anillo de acero inoxidable 316L.", Material = "Acero inoxidable 316L", Finish = "Plateado", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/angelo/anillo-tizzo.svg" },
            new Product { Name = "Anillo Vértice", Price = 59.00m, Category = "Anillos", Description = "Anillo de acero inoxidable 316L.", Material = "Acero inoxidable 316L", Finish = "Negro", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/angelo/anillo-vertice.svg" },
            new Product { Name = "Anillo Eslabón", Price = 52.00m, Category = "Anillos", Description = "Anillo de acero inoxidable 316L.", Material = "Acero inoxidable 316L", Finish = "Plateado", SizesCsv = "7,8,9,10,11", ImageUrl = "/images/products/angelo/anillo-eslabon.svg" },
            new Product { Name = "Cadena Eslabones", Price = 89.00m, Category = "Cadenas", Description = "Eslabones de acero inoxidable. Una pieza versátil para expresar tu identidad.", Material = "Acero inoxidable 316L", Finish = "Plateado", SizesCsv = "Única", ImageUrl = "/images/products/angelo/cadena-eslabones.svg" },
            new Product { Name = "Pulsera Eslabones", Price = 69.00m, Category = "Pulseras", Description = "Eslabones de acero inoxidable. Una pieza versátil para expresar tu identidad.", Material = "Acero inoxidable 316L", Finish = "Plateado", SizesCsv = "Única", ImageUrl = "/images/products/angelo/pulsera-eslabones.svg" }
        };
        var existingNames = await db.Products.AsNoTracking().Select(p => p.Name).ToListAsync();
        db.Products.AddRange(demoProducts.Where(p => !existingNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase)));
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

    // Cuentas del prototipo para demostraciones locales. Las claves se guardan en User Secrets.
    public static async Task SeedDemoAccountsAsync(IServiceProvider services, IConfiguration configuration)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var accounts = new[]
        {
            (Email: "admin@blackcode.pe", Name: "Administrador Demo", Role: Roles.Admin, Password: configuration["SeedDemo:AdminPassword"]),
            (Email: "cliente@demo.pe", Name: "Cliente Demo", Role: Roles.Cliente, Password: configuration["SeedDemo:ClientePassword"])
        };

        foreach (var account in accounts)
        {
            if (string.IsNullOrWhiteSpace(account.Password)) continue;
            var user = await userManager.FindByEmailAsync(account.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = account.Email, Email = account.Email,
                    FullName = account.Name, EmailConfirmed = true
                };
                var created = await userManager.CreateAsync(user, account.Password);
                if (!created.Succeeded)
                    throw new InvalidOperationException($"No se pudo crear la cuenta de ejemplo {account.Email}.");
            }
            if (!await userManager.IsInRoleAsync(user, account.Role))
                await userManager.AddToRoleAsync(user, account.Role);
        }
    }
}
