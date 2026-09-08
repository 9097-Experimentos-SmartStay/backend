using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Payments.Domain.Services;

/// <summary>
/// Defines the contract for services that retrieve payment data.
/// </summary>
public interface IPaymentQueryService
{
    Task<Payment?> Handle(GetPaymentByBookingIdQuery query);
}