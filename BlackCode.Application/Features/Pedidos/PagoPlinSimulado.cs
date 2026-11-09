using BlackCode.Domain.Pedidos;

namespace BlackCode.Application.Features.Pedidos;

// SRP: simula únicamente el resultado de un pago con Plin.
public sealed class PagoPlinSimulado : IMetodoPago
{
    public string Codigo => "plin";
    public string Nombre => "Plin";
    public MetodoPago Metodo => MetodoPago.Plin;

    public Task<ResultadoPagoDto> ProcesarAsync(decimal monto, bool simularRechazo)
        => Task.FromResult(new ResultadoPagoDto(monto > 0 && !simularRechazo,
            "DEMO-PLIN-" + Guid.NewGuid().ToString("N")[..12], Metodo));
}
