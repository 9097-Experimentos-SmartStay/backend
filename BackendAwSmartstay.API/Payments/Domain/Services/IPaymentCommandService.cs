using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Payments.Domain.Services;

/// <summary>Payment use cases.</summary>
public interface IPaymentCommandService
{
    /// <summary>Registers the payment of a Pending booking and, when approved, confirms the booking.</summary>
    Task<Payment> Handle(RegisterPaymentCommand command);
}
