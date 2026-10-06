using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Payments.Infrastructure.Gateways;

/// <summary>
///     Simulated online card gateway: no money moves and no provider is contacted. Every valid card is approved except
///     these test numbers, which are declined with their reason:
///     <list type="bullet">
///         <item><c>4000 0000 0000 0002</c>: card declined</item>
///         <item><c>4000 0000 0000 9995</c>: insufficient funds</item>
///         <item><c>4000 0000 0000 0127</c>: incorrect CVV</item>
///     </list>
/// </summary>
public class SimulatedCardPaymentGateway : ICardPaymentGateway
{
    private static readonly Dictionary<string, string> DeclinedCards = new()
    {
        ["4000000000000002"] = "The card was declined.",
        ["4000000000009995"] = "The card has insufficient funds.",
        ["4000000000000127"] = "The card's security code is incorrect."
    };

    public Task<PaymentGatewayResult> ChargeCardAsync(PaymentCharge charge, CardDetails card,
        CancellationToken cancellationToken = default)
    {
        if (DeclinedCards.TryGetValue(card.Number, out var reason))
            return Task.FromResult(PaymentGatewayResult.Reject(reason));

        var reference = $"SIM-{card.Brand.ToUpperInvariant()}-{card.LastFour}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
        return Task.FromResult(PaymentGatewayResult.Approve(reference));
    }
}
