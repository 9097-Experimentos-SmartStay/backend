using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

/// <summary>
/// Defines the contract for services that handle booking state changes (Create, Confirm, Cancel).
/// Missing bookings raise <c>BookingNotFoundException</c>; rule violations raise domain exceptions.
/// </summary>
public interface IBookingCommandService
{
    Task<Booking> Handle(CreateBookingCommand command);

    Task<Booking> Handle(ConfirmBookingCommand command);

    Task<Booking> Handle(CancelBookingCommand command);
}
