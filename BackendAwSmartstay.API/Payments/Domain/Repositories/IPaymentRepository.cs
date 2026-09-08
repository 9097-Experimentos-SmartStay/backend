using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Payments.Domain.Repositories;

/// <summary>
/// Interface for Payment repository operations.
/// </summary>
public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<Payment?> FindByBookingIdAsync(int bookingId);
}