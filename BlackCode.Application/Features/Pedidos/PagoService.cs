using BlackCode.Domain.Common;
namespace BlackCode.Application.Features.Pedidos;
public record MetodoPagoDto(string Codigo, string Nombre);
public interface IMetodoPago
{
    string Codigo { get; }
    string Nombre { get; }
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
    public Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo, string metodo = "tarjeta")
    {
        var estrategia = metodos.SingleOrDefault(m => m.Codigo == metodo)
            ?? throw new DomainException("El método de pago seleccionado no está disponible.");
        return estrategia.ProcesarAsync(monto, simularRechazo);
    }
}
public sealed class PagoTarjetaSimulado : IMetodoPago
{
    public string Codigo => "tarjeta";
    public string Nombre => "Tarjeta — simulación";
    public Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo)
        => Task.FromResult(new ResultadoPagoDto(monto > 0 && !simularRechazo, "DEMO-TARJETA-" + Guid.NewGuid().ToString("N")[..12]));
}
public sealed class PagoTransferenciaSimulado : IMetodoPago
{
    public string Codigo => "transferencia";
    public string Nombre => "Transferencia — simulación";
    public Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo)
        => Task.FromResult(new ResultadoPagoDto(monto > 0 && !simularRechazo, "DEMO-TRANSFERENCIA-" + Guid.NewGuid().ToString("N")[..12]));
}
