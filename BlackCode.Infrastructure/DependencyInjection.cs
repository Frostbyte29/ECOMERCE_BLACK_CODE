using BlackCode.Application.Abstractions;
using BlackCode.Infrastructure.Persistence;
using BlackCode.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlackCode.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// DIP: el único lugar donde se decide "la base es MySQL".
    /// Cambiar de proveedor (ej. a Pomelo) toca solo esta clase y el paquete NuGet.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Falta ConnectionStrings:Default en los secretos de usuario de BlackCode.Web.");

        services.AddDbContext<AppDbContext>(options => options.UseMySQL(connectionString));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IStockGateway, MySqlStockGateway>();
        services.AddSingleton<IContrasenaService, AspNetContrasenaService>();
        services.AddScoped<IAlmacenImagenesProducto, Storage.AlmacenImagenesProducto>();

        return services;
    }
}
