using BlackCode.Application;
using BlackCode.Application.Abstractions;
using BlackCode.Infrastructure;
using BlackCode.Infrastructure.Persistence;
using BlackCode.Web.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC + protección antiforgery en TODOS los POST ----------
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// ---------- Raíz de composición (DIP): aquí se conectan las abstracciones con sus implementaciones ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IUsuarioActual, UsuarioActualHttp>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- Sesión con cookie (no JWT: en MVC la cookie HttpOnly es lo correcto) ----------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "bc_auth";
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnRedirectToLogin = EventosCookie.RedirigirAlLogin;
        options.Events.OnValidatePrincipal = EventosCookie.ValidarSesion;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Aplica migraciones y carga datos de demo (los compañeros no necesitan correr Update-Database)
    using var scope = app.Services.CreateScope();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // sirve imágenes subidas en tiempo de ejecución (wwwroot/uploads)
app.UseRouting();

app.UseAuthentication();   // SIEMPRE antes de UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
