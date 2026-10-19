using BlackCode.Application.Features.Autenticacion;
using BlackCode.Application.Features.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace BlackCode.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Aquí se registran los servicios de cada funcionalidad (una línea por servicio).
    /// Ejemplo: services.AddScoped&lt;ICatalogoService, CatalogoService&gt;();
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Features/Autenticacion
        services.AddScoped<IAutenticacionService, AutenticacionService>();

        // Features/Usuarios
        services.AddScoped<IGestionUsuariosService, GestionUsuariosService>();

        return services;
    }
}
