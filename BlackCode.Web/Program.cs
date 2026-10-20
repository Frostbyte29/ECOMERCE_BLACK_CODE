using BlackCode.Web.Data;
using BlackCode.Web.Models;
using BlackCode.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("BlackCode")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BlackCode'.");

// "MySqlServerVersion" (ej. 8.0.36-mysql) es opcional; permite crear migraciones sin conexión a MySQL.
var serverVersion = builder.Configuration["MySqlServerVersion"] is { Length: > 0 } version
    ? ServerVersion.Parse(version)
    : null;

builder.Services.AddDbContext<BlackCodeContext>(options =>
    options.UseMySql(
        connectionString,
        serverVersion ?? ServerVersion.AutoDetect(connectionString)));

// Usuarios y roles (Cliente / Admin). El módulo de login puede cambiar estas reglas.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<BlackCodeContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.LogoutPath = "/Cuenta/Logout";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
});

// Carrito en sesión.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inserta los productos de demostración solo cuando la tabla ya existe y está vacía.
// La base de datos y la tabla se crean aplicando primero las migraciones.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BlackCodeContext>();
    try
    {
        if (await db.Database.CanConnectAsync())
        {
            await DbInitializer.SeedAsync(db);
            await DbInitializer.SeedRolesAndAdminAsync(scope.ServiceProvider, app.Configuration);
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex,
            "No se pudo comprobar o inicializar la base de datos. Verifica MySQL y aplica las migraciones.");
    }
}

app.Run();
