using BlackCode.Web.Controllers;
using BlackCode.Web.Data;
using BlackCode.Web.Models;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlackCode.Tests;

[CollectionDefinition("MySQL", DisableParallelization = true)]
public class MySqlCollection;

[Collection("MySQL")]
public class CheckoutAndCustomerIntegrationTests
{
    private static ServiceProvider CreateServices()
    {
        var connection = Environment.GetEnvironmentVariable("BLACKCODE_TEST_CONNECTION_STRING")!;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<BlackCodeContext>(options => options.UseMySql(connection, ServerVersion.AutoDetect(connection)));
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<BlackCodeContext>().AddDefaultTokenProviders();
        return services.BuildServiceProvider();
    }

    [MySqlFact]
    public async Task RepetirElMismoCheckoutNoPuedeGuardarDosPedidos()
    {
        await using var provider = CreateServices();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BlackCodeContext>();
        await db.Database.MigrateAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = $"test-{Guid.NewGuid():N}@example.test", Email = $"test-{Guid.NewGuid():N}@example.test", FullName = "Cliente de prueba" };
        Assert.True((await userManager.CreateAsync(user, "Clave123!")).Succeeded);
        var product = new Product { Name = "Producto de prueba", Price = 20m, Description = "Prueba", ImageUrl = "/images/products/anillo-obsidiana.jpg" };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var token = Guid.NewGuid().ToString("N");
        var checkout = new CheckoutViewModel
        {
            CheckoutToken = token, ShippingName = "Cliente de prueba", ShippingPhone = "987654321",
            ShippingAddress = "Calle de prueba 123", ShippingDistrict = "Lima", ShippingCity = "Lima"
        };
        var cart = new CartSummary { Lines = [new CartLine { Product = product, Quantity = 1, Size = "8" }] };
        var orders = new OrderService(db);
        var first = await orders.CreateOrderAsync(user.Id, checkout, cart);
        Assert.Equal(first.Id, await orders.FindCheckoutOrderAsync(user.Id, token));

        db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<DbUpdateException>(() => orders.CreateOrderAsync(user.Id, checkout, cart));
        db.ChangeTracker.Clear();
        Assert.Equal(1, await db.Orders.CountAsync(o => o.CheckoutToken == token));
    }

    [MySqlFact]
    public async Task UnaCuentaDesactivadaNoPuedeIniciarSesion()
    {
        await using var provider = CreateServices();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BlackCodeContext>();
        await db.Database.MigrateAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = $"inactivo-{Guid.NewGuid():N}@example.test",
            Email = $"inactivo-{Guid.NewGuid():N}@example.test",
            FullName = "Cliente inactivo", IsActive = false
        };
        Assert.True((await userManager.CreateAsync(user, "Clave123!")).Succeeded);

        var controller = new CuentaController(null!, userManager);
        var result = await controller.Login(new LoginViewModel { Email = user.Email!, Password = "Clave123!" });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [MySqlFact]
    public async Task AdministrarClientesCreaYDesactivaSinModificarAdministradores()
    {
        await using var provider = CreateServices();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BlackCodeContext>();
        await db.Database.MigrateAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { Roles.Cliente, Roles.Admin })
            if (!await roleManager.RoleExistsAsync(role)) Assert.True((await roleManager.CreateAsync(new IdentityRole(role))).Succeeded);

        var controller = new AdminClientesController(db, userManager)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new MemoryTempDataProvider())
        };
        var email = $"nuevo-{Guid.NewGuid():N}@example.test";
        var created = await controller.Nuevo(new AdminCustomerFormViewModel
        {
            Email = email, Name = "Cliente nuevo", Phone = "987654321", Password = "Clave123!"
        });
        Assert.IsType<RedirectToActionResult>(created);

        var customer = await userManager.FindByEmailAsync(email);
        Assert.NotNull(customer);
        Assert.True(await userManager.IsInRoleAsync(customer, Roles.Cliente));
        var previousStamp = customer.SecurityStamp;

        var edited = await controller.Editar(customer.Id, new AdminCustomerFormViewModel
        {
            Id = customer.Id, Email = email, Name = "Cliente editado", Phone = "912345678", IsActive = false
        });
        Assert.IsType<RedirectToActionResult>(edited);
        Assert.False(customer.IsActive);
        Assert.Equal("Cliente editado", customer.FullName);
        Assert.NotEqual(previousStamp, customer.SecurityStamp);

        var admin = new ApplicationUser { UserName = $"admin-{Guid.NewGuid():N}@example.test", Email = $"admin-{Guid.NewGuid():N}@example.test", FullName = "Admin" };
        Assert.True((await userManager.CreateAsync(admin, "Clave123!")).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(admin, Roles.Admin)).Succeeded);
        Assert.IsType<NotFoundResult>(await controller.Editar(admin.Id));

        var list = Assert.IsType<ViewResult>(await controller.Index(email, null));
        var model = Assert.IsType<AdminCustomersViewModel>(list.Model);
        Assert.Contains(model.Customers, row => row.Id == customer.Id && !row.IsActive);
        Assert.DoesNotContain(model.Customers, row => row.Id == admin.Id);
    }

    private sealed class MemoryTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
