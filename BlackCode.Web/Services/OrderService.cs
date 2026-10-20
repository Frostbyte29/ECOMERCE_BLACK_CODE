using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BlackCode.Web.Data;
using BlackCode.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Services;

public record OperationResult(bool Success, string Message);

public class OrderService(BlackCodeContext context)
{
    private static readonly TimeSpan PaymentWindow = TimeSpan.FromDays(3);
    // Cambios de estado permitidos desde el panel de administración.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Transitions = new()
    {
        [OrderStatus.PendientePago] = [OrderStatus.Cancelado],
        [OrderStatus.Pagado] = [OrderStatus.EnPreparacion, OrderStatus.Cancelado],
        [OrderStatus.EnPreparacion] = [OrderStatus.Enviado, OrderStatus.Cancelado],
        [OrderStatus.Enviado] = [OrderStatus.Entregado],
        [OrderStatus.Entregado] = [],
        [OrderStatus.Cancelado] = []
    };

    public static IReadOnlyList<OrderStatus> AllowedTransitions(OrderStatus status) => Transitions[status];

    public static bool CustomerCanCancel(Order order) => order.Status == OrderStatus.PendientePago;
    public static DateTime PaymentDueAt(Order order) => order.CreatedAt.Add(PaymentWindow);

    public IQueryable<Order> QueryWithDetails() => context.Orders
        .Include(o => o.User)
        .Include(o => o.Items)
        .Include(o => o.Payments)
        .Include(o => o.History);

    public Task<int?> FindCheckoutOrderAsync(string userId, string checkoutToken) => context.Orders.AsNoTracking()
        .Where(o => o.UserId == userId && o.CheckoutToken == checkoutToken)
        .Select(o => (int?)o.Id).FirstOrDefaultAsync();

    public async Task<Order> CreateOrderAsync(string userId, CheckoutViewModel checkout, CartSummary cart)
    {
        if (cart.IsEmpty) throw new InvalidOperationException("El carrito está vacío.");

        var order = new Order
        {
            Code = GenerateCode(),
            CheckoutToken = checkout.CheckoutToken,
            UserId = userId,
            Status = OrderStatus.PendientePago,
            Subtotal = cart.Subtotal,
            ShippingCost = cart.ShippingCost,
            Total = cart.Total,
            ShippingName = checkout.ShippingName.Trim(),
            ShippingPhone = checkout.ShippingPhone.Trim(),
            ShippingAddress = checkout.ShippingAddress.Trim(),
            ShippingDistrict = checkout.ShippingDistrict.Trim(),
            ShippingCity = checkout.ShippingCity.Trim(),
            ShippingReference = checkout.ShippingReference?.Trim(),
            Notes = checkout.Notes?.Trim(),
            Items = cart.Lines.Select(l => new OrderItem
            {
                ProductId = l.Product.Id,
                ProductName = l.Product.Name,
                ImageUrl = l.Product.ImageUrl,
                Size = l.Size,
                UnitPrice = l.Product.Price,
                Quantity = l.Quantity
            }).ToList()
        };
        order.History.Add(new OrderStatusHistory { Status = order.Status, ChangedBy = "Cliente", Note = "Pedido creado." });

        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order;
    }

    // Pasarela de pago SIMULADA. Reemplazar este método para integrar Culqi, Niubiz o Mercado Pago.
    public async Task<OperationResult> ProcessPaymentAsync(Order order, PaymentViewModel input)
    {
        if (order.Status != OrderStatus.PendientePago)
            return new(false, "Este pedido ya no está pendiente de pago.");

        if (PaymentDueAt(order) <= DateTime.UtcNow)
            return new(false, "El plazo de 3 días para pagar este pedido ya venció.");
        if (!Enum.IsDefined(input.Method))
            return new(false, "Elige un método de pago válido.");

        var payment = new Payment { Method = input.Method, Amount = order.Total };

        switch (input.Method)
        {
            case PaymentMethod.Tarjeta:
            {
                var error = ValidateCard(input);
                if (error is not null) return new(false, error);

                var digits = OnlyDigits(input.CardNumber);
                payment.CardLast4 = digits[^4..];
                // Tarjeta de prueba que siempre se rechaza (como en las pasarelas reales en modo test).
                if (digits.EndsWith("0002"))
                {
                    payment.Status = PaymentStatus.Rechazado;
                    payment.Message = "Pago de prueba rechazado. No hubo ningún cargo bancario.";
                }
                else
                {
                    payment.Status = PaymentStatus.Aprobado;
                    payment.TransactionId = NewTransactionId();
                    payment.Message = "Pago de prueba con tarjeta aprobado. No hubo ningún cargo bancario.";
                }
                break;
            }
            case PaymentMethod.YapePlin:
            {
                var code = OnlyDigits(input.OperationCode);
                if (code.Length is < 6 or > 10)
                    return new(false, "Ingresa el número de operación de Yape/Plin (6 a 10 dígitos).");
                payment.Status = PaymentStatus.Aprobado;
                payment.TransactionId = "YP-" + code;
                payment.Message = "Pago de prueba con Yape/Plin aprobado. No hubo transferencia real.";
                break;
            }
            case PaymentMethod.ContraEntrega:
                payment.Status = PaymentStatus.Pendiente;
                payment.Message = "Pedido de prueba registrado con pago contra entrega pendiente.";
                break;
        }

        if (payment.Status != PaymentStatus.Rechazado)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            var now = DateTime.UtcNow;
            var claimed = await context.Orders
                .Where(o => o.Id == order.Id && o.Status == OrderStatus.PendientePago && o.CreatedAt > now - PaymentWindow)
                .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.Status,
                    payment.Status == PaymentStatus.Aprobado ? OrderStatus.Pagado : OrderStatus.EnPreparacion));
            if (claimed == 0)
                return new(false, "El pedido ya se pagó, se canceló o venció su plazo de pago.");

            order.Payments.Add(payment);
            if (payment.Status == PaymentStatus.Aprobado)
                AddStatus(order, OrderStatus.Pagado, "Pasarela", payment.Message);
            else
                AddStatus(order, OrderStatus.EnPreparacion, "Cliente", "Pedido confirmado con pago contra entrega.");

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new(true, payment.Message ?? "");
        }

        order.Payments.Add(payment);
        await context.SaveChangesAsync();
        return new(false, payment.Message ?? "");
    }

    public async Task<int> ExpirePendingOrdersAsync(int? orderId = null, string? userId = null)
    {
        var expiredCount = 0;
        while (true)
        {
            var cutoff = DateTime.UtcNow - PaymentWindow;
            var query = context.Orders.AsNoTracking()
                .Where(o => o.Status == OrderStatus.PendientePago && o.CreatedAt <= cutoff);
            if (orderId.HasValue) query = query.Where(o => o.Id == orderId.Value);
            if (userId is not null) query = query.Where(o => o.UserId == userId);
            var ids = await query.OrderBy(o => o.Id).Select(o => o.Id).Take(100).ToListAsync();
            if (ids.Count == 0) break;

            foreach (var id in ids)
            {
                await using var transaction = await context.Database.BeginTransactionAsync();
                var changed = await context.Orders
                    .Where(o => o.Id == id && o.Status == OrderStatus.PendientePago && o.CreatedAt <= cutoff)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(o => o.Status, OrderStatus.Cancelado)
                        .SetProperty(o => o.UpdatedAt, DateTime.UtcNow));
                if (changed == 0) continue;

                await context.Payments
                    .Where(p => p.OrderId == id && p.Status == PaymentStatus.Pendiente)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.Status, PaymentStatus.Rechazado)
                        .SetProperty(p => p.Message, "Pago pendiente anulado al vencer el plazo de 3 días."));
                context.OrderStatusHistory.Add(new OrderStatusHistory
                {
                    OrderId = id,
                    Status = OrderStatus.Cancelado,
                    ChangedBy = "Sistema",
                    Note = "Pedido cancelado al vencer los 3 días para pagar."
                });
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                expiredCount++;
            }
            if (ids.Count < 100) break;
        }
        return expiredCount;
    }

    public async Task<OperationResult> ChangeStatusAsync(Order order, OrderStatus newStatus, string changedBy, string? note, string? trackingNumber)
    {
        if (!Transitions[order.Status].Contains(newStatus))
            return new(false, $"No se puede pasar de \"{DisplayName(order.Status)}\" a \"{DisplayName(newStatus)}\".");

        if (newStatus == OrderStatus.Enviado)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return new(false, "Ingresa el número de seguimiento para marcar el pedido como enviado.");
            order.TrackingNumber = trackingNumber.Trim();
        }

        if (newStatus == OrderStatus.Entregado)
        {
            // En contra entrega el pago se confirma cuando se entrega el pedido.
            foreach (var p in order.Payments.Where(p => p.Status == PaymentStatus.Pendiente))
            {
                p.Status = PaymentStatus.Aprobado;
                p.TransactionId ??= NewTransactionId();
                p.Message = "Pago contra entrega marcado como recibido en la demostración.";
            }
        }

        if (newStatus == OrderStatus.Cancelado)
            CancelPayments(order);

        AddStatus(order, newStatus, changedBy, note);
        await context.SaveChangesAsync();
        return new(true, $"Pedido {order.Code} actualizado a \"{DisplayName(newStatus)}\".");
    }

    public async Task<OperationResult> CancelByCustomerAsync(Order order)
    {
        if (!CustomerCanCancel(order))
            return new(false, "Solo puedes cancelar pedidos que aún no fueron pagados.");
        await using var transaction = await context.Database.BeginTransactionAsync();
        var changed = await context.Orders
            .Where(o => o.Id == order.Id && o.Status == OrderStatus.PendientePago)
            .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.Status, OrderStatus.Cancelado));
        if (changed == 0)
            return new(false, "El pedido ya se pagó o se canceló.");
        CancelPayments(order);
        AddStatus(order, OrderStatus.Cancelado, "Cliente", "Cancelado por el cliente.");
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return new(true, $"El pedido {order.Code} fue cancelado.");
    }

    public static string DisplayName(Enum value) =>
        value.GetType().GetMember(value.ToString()).FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();

    private static void CancelPayments(Order order)
    {
        foreach (var p in order.Payments)
        {
            if (p.Status == PaymentStatus.Aprobado) { p.Status = PaymentStatus.Reembolsado; p.Message = "Reembolso simulado por cancelación. No se movió dinero."; }
            else if (p.Status == PaymentStatus.Pendiente) { p.Status = PaymentStatus.Rechazado; p.Message = "Anulado por cancelación."; }
        }
    }

    private static void AddStatus(Order order, OrderStatus status, string changedBy, string? note)
    {
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        order.History.Add(new OrderStatusHistory { Status = status, ChangedBy = changedBy, Note = note });
    }

    private static string? ValidateCard(PaymentViewModel input)
    {
        var digits = OnlyDigits(input.CardNumber);
        if (digits.Length is < 13 or > 19 || !PassesLuhn(digits))
            return "El número de tarjeta no es válido.";
        if (string.IsNullOrWhiteSpace(input.CardHolder))
            return "Ingresa el nombre que figura en la tarjeta.";

        var parts = (input.CardExpiry ?? "").Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year)
            || month is < 1 or > 12 || year is < 0 or > 99)
            return "Ingresa el vencimiento en formato MM/AA.";
        var lastDay = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);
        if (lastDay < DateTime.Today)
            return "La tarjeta está vencida.";

        var cvv = OnlyDigits(input.CardCvv);
        if (cvv.Length is < 3 or > 4)
            return "El CVV no es válido.";
        return null;
    }

    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var doubleIt = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var d = digits[i] - '0';
            if (doubleIt) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
            doubleIt = !doubleIt;
        }
        return sum % 10 == 0;
    }

    private static string OnlyDigits(string? value) => new((value ?? "").Where(char.IsDigit).ToArray());

    private static string GenerateCode() =>
        $"BC-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

    private static string NewTransactionId() => "SIM-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
}
