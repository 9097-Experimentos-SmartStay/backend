using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

/// <summary>
/// Defines the contract for services that handle booking state changes (Create, Confirm, Cancel).
/// </summary>
public interface IBookingCommandService
{

    /// <summary>
    /// Handles the creation of a booking.
    /// </summary>
    /// <param name="command">The create command.</param>
    /// <returns>The created booking or null if creation failed.</returns>
    public Task<Booking?> Handle(CreateBookingCommand command);


    /// <summary>
    /// Handles the confirmation of a booking.
    /// </summary>
    /// <param name="command">The confirm command.</param>
    /// <returns>The confirmed booking or null if not found.</returns>
    public Task<Booking?> Handle(ConfirmBookingCommand command);


    /// <summary>
    /// Handles the cancellation of a booking.
    /// </summary>
    /// <param name="command">The cancel command.</param>
    /// <returns>The cancelled booking or null if not found.</returns>
    public Task<Booking?> Handle(CancelBookingCommand command);
}
