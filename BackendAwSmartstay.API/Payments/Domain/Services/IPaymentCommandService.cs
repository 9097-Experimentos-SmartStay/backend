using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Payments.Domain.Services;

/// <summary>
/// Defines the contract for services that handle payment state changes.
/// </summary>
public interface IPaymentCommandService
{
    /// <summary>
    /// Processes a payment command, simulating bank validation.
    /// </summary>
    /// <param name="command">The payment data.</param>
    /// <returns>The created payment transaction entity.</returns>
    Task<Payment?> Handle(ProcessPaymentCommand command);
}