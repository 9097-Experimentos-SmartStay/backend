using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Payments.Domain.Repositories;

/// <summary>
/// Interface for Payment repository operations.
/// </summary>
public interface IPaymentRepository : IBaseRepository<Payment>
{
    /// <summary>
    /// The payment of a booking: the completed one if any, otherwise the most recent attempt.
    /// </summary>
    Task<Payment?> FindByBookingIdAsync(int bookingId);

    /// <summary>True when the booking already has a completed payment.</summary>
    Task<bool> ExistsCompletedForBookingAsync(int bookingId);
}