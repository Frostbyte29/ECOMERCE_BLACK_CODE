using BlackCode.Domain.Pedidos;

namespace BlackCode.Application.Features.Pedidos;

// SRP: simula únicamente el resultado de un pago con Yape.
public sealed class PagoYapeSimulado : IMetodoPago
{
    public string Codigo => "yape";
    public string Nombre => "Yape";
    public MetodoPago Metodo => MetodoPago.Yape;

    public Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo)
    {
        if (monto <= 0 || simularRechazo) return Task.FromResult(new ResultadoPagoDto(false, "DEMO-YAPE-RECHAZADO", Metodo));
        return Task.FromResult(new ResultadoPagoDto(false, "DEMO-YAPE-" + Guid.NewGuid().ToString("N")[..12], Metodo, EsPendiente: true));
    }
}
