using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Payments.Infrastructure.Gateways;

/// <summary>
///     Adapter for payments received by the hotel itself: the staff member attests them (with the operation number
///     of Yape, Plin, the transfer or the POS voucher), so they are always approved. The reference keeps the method and
///     operation number; cash gets a generated one.
/// </summary>
public class ManualPaymentGateway : IPaymentGateway
{
    public Task<PaymentGatewayResult> ChargeAsync(PaymentCharge charge, CancellationToken cancellationToken = default)
    {
        var reference = charge.Method == PaymentMethod.Cash || string.IsNullOrWhiteSpace(charge.OperationNumber)
            ? $"CASH-{charge.BookingCode}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
            : $"{charge.Method.ToString().ToUpperInvariant()}-{charge.OperationNumber}";
        return Task.FromResult(PaymentGatewayResult.Approve(reference));
    }
}
