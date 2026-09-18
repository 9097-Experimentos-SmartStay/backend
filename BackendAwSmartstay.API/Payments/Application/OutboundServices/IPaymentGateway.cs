using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Payments.Application.OutboundServices;

/// <summary>A charge for the total of a booking.</summary>
public sealed record PaymentCharge(int BookingId, string BookingCode, decimal Amount, PaymentMethod Method, string? OperationNumber);

/// <summary>What the gateway answered: approved with its reference, or rejected with a reason.</summary>
public sealed record PaymentGatewayResult(bool Approved, string TransactionReference, string? FailureReason = null)
{
    public static PaymentGatewayResult Approve(string reference) => new(true, reference);
    public static PaymentGatewayResult Reject(string reason) => new(false, string.Empty, reason);
}

/// <summary>
///     Outbound port through which a payment is charged or attested. The domain and the application service only
///     know this port: today the <c>ManualPaymentGateway</c> adapter records payments the hotel already received
///     (Yape, Plin, transfer, cash, POS); an online gateway (e.g. a Mercado Pago adapter that creates the charge and
///     is confirmed by its webhook) plugs in by implementing this interface and registering it, without touching the
///     Payment aggregate or the booking rules.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(PaymentCharge charge, CancellationToken cancellationToken = default);
}
