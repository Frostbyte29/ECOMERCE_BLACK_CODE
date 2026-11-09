using BlackCode.Application.Features.Autenticacion;
using BlackCode.Application.Features.Clientes;
using BlackCode.Application.Features.Cuenta;
using BlackCode.Application.Features.Catalogo;
using BlackCode.Application.Features.Carrito;
using BlackCode.Application.Features.Pedidos;
using BlackCode.Application.Features.Administracion;
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

        // Features/Cuenta
        services.AddScoped<ICuentaService, CuentaService>();

        // Features/Clientes
        services.AddScoped<IGestionClientesService, GestionClientesService>();

        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddSingleton<IFiltroProducto, FiltroTexto>();
        services.AddSingleton<IFiltroProducto, FiltroCategoria>();
        services.AddSingleton<IFiltroProducto, FiltroColor>();
        services.AddSingleton<IFiltroProducto, FiltroPrecio>();
        services.AddSingleton<IFiltroProducto, FiltroTalla>();
        services.AddSingleton<IFiltroProducto, FiltroMaterial>();
        services.AddScoped<ICarritoService, CarritoService>();
        services.AddScoped<ICalculoCarritoService, CalculoCarritoService>();
        services.AddSingleton<IReglaDescuento, SinDescuento>();
        services.AddSingleton<IMetodoPago, PagoTarjetaSimulado>();
        services.AddSingleton<IMetodoPago, PagoTransferenciaSimulado>();
        services.AddSingleton<IMetodoPago, PagoYapeSimulado>();
        services.AddSingleton<IMetodoPago, PagoPlinSimulado>();
        services.AddScoped<IPagoService, PagoService>();
        services.AddSingleton<INotificacionPedidoService, NotificacionPedidoService>();
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IAdministracionService, AdministracionService>();
        services.AddScoped<IInventarioService, InventarioService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddSingleton<IExportadorReporte, ExportadorCsv>();
        services.AddSingleton<IExportadorReporte, ExportadorJson>();
        return services;
    }
}
