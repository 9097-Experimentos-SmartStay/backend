using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Queries;
using BackendAwSmartstay.API.Payments.Domain.Repositories;
using BackendAwSmartstay.API.Payments.Domain.Services;

namespace BackendAwSmartstay.API.Payments.Application.Internal.QueryServices;

/// <summary>
/// Service implementation for handling payment queries.
/// Retrieves payment data from the repository.
/// </summary>
public class PaymentQueryService(IPaymentRepository paymentRepository) : IPaymentQueryService
{
    /// <summary>
    /// Handles the query to retrieve a payment by booking identifier.
    /// </summary>
    /// <param name="query">The query containing the booking ID.</param>
    /// <returns>The payment associated with the booking or null if not found.</returns>
    public async Task<Payment?> Handle(GetPaymentByBookingIdQuery query)
    {
        return await paymentRepository.FindByBookingIdAsync(query.BookingId);
    }
}