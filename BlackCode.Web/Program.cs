using BlackCode.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("BlackCode")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BlackCode'.");

builder.Services.AddDbContext<BlackCodeContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
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
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex,
            "No se pudo comprobar o inicializar la base de datos. Verifica MySQL y aplica las migraciones.");
    }
}

app.Run();
