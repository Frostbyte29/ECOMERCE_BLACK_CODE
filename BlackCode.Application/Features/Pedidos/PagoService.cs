using BlackCode.Domain.Common;
using BlackCode.Domain.Pedidos;
namespace BlackCode.Application.Features.Pedidos;
public record MetodoPagoDto(string Codigo, string Nombre);
public interface IMetodoPago
{
    string Codigo { get; }
    string Nombre { get; }
    MetodoPago Metodo { get; }
    Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo);
}
public interface IPagoService
{
    IReadOnlyList<MetodoPagoDto> Metodos();
    Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo, string metodo = "tarjeta");
}
public sealed class PagoService(IEnumerable<IMetodoPago> metodos) : IPagoService
{
    public IReadOnlyList<MetodoPagoDto> Metodos() => metodos.Select(m => new MetodoPagoDto(m.Codigo, m.Nombre)).ToList();
    public async Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo, string metodo = "tarjeta")
    {
        var estrategia = metodos.SingleOrDefault(m => m.Codigo == metodo)
            ?? throw new DomainException("El método de pago seleccionado no está disponible.");
        var resultado = await estrategia.ProcesarAsync(monto, simularRechazo);
        return resultado with { Metodo = estrategia.Metodo };
    }
}
public sealed class PagoTarjetaSimulado : IMetodoPago
{
    public string Codigo => "tarjeta";
    public string Nombre => "Tarjeta";
    public MetodoPago Metodo => MetodoPago.Tarjeta;
    public Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo)
        => Task.FromResult(new ResultadoPagoDto(monto > 0 && !simularRechazo, "DEMO-TARJETA-" + Guid.NewGuid().ToString("N")[..12]));
}
public sealed class PagoTransferenciaSimulado : IMetodoPago
{
    public string Codigo => "transferencia";
    public string Nombre => "Transferencia";
    public MetodoPago Metodo => MetodoPago.Transferencia;
    public Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo)
        => Task.FromResult(new ResultadoPagoDto(monto > 0 && !simularRechazo, "DEMO-TRANSFERENCIA-" + Guid.NewGuid().ToString("N")[..12]));
}
