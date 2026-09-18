using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

/// <summary>
/// Booking use cases: place, confirm (payment), cancel, reschedule, expire unpaid, check in.
/// Missing bookings raise <c>BookingNotFoundException</c>; rule violations raise domain exceptions.
/// </summary>
public interface IBookingCommandService
{
    Task<Booking> Handle(CreateBookingCommand command);

    Task<Booking> Handle(ConfirmBookingCommand command);

    Task<Booking> Handle(CancelBookingCommand command);

    Task<Booking> Handle(RescheduleBookingCommand command);

    /// <returns>How many bookings expired.</returns>
    Task<int> Handle(ExpireUnpaidBookingsCommand command);
}
