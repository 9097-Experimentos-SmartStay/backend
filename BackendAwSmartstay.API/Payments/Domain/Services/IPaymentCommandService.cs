using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Payments.Domain.Services;

/// <summary>Payment use cases.</summary>
public interface IPaymentCommandService
{
    /// <summary>Registers the payment of a Pending booking and, when approved, confirms the booking.</summary>
    Task<Payment> Handle(RegisterPaymentCommand command);

    /// <summary>
    ///     Charges the guest's card for their Pending booking and, when approved, confirms the booking. A declined card
    ///     keeps a Failed payment and throws <c>payment.card_declined</c>.
    /// </summary>
    Task<Payment> Handle(PayBookingWithCardCommand command);
}
