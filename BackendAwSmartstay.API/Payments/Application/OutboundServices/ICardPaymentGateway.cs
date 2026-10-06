using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Payments.Application.OutboundServices;

/// <summary>
///     Outbound port through which a guest's card is charged online. Today the <c>SimulatedCardPaymentGateway</c>
///     adapter approves or declines test cards without contacting any provider; a real gateway (e.g. Mercado Pago or
///     Culqi) plugs in by implementing this interface and registering it.
/// </summary>
public interface ICardPaymentGateway
{
    Task<PaymentGatewayResult> ChargeCardAsync(PaymentCharge charge, CardDetails card, CancellationToken cancellationToken = default);
}
