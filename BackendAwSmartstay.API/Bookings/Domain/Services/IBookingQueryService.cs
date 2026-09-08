using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

/// <summary>
/// Defines the contract for services that handle booking queries.
/// </summary>
public interface IBookingQueryService
{
    /// <summary>
    /// Handles the query to get a booking by its identifier.
    /// </summary>
    /// <param name="query">The query containing the booking ID.</param>
    /// <returns>The booking or null if not found.</returns>
    Task<Booking?> Handle(GetBookingByIdQuery query);

    /// <summary>
    /// Handles the query to get all bookings.
    /// </summary>
    /// <param name="query">The query to retrieve all bookings.</param>
    /// <returns>A collection of all bookings.</returns>
    Task<IEnumerable<Booking>> Handle(GetAllBookingsQuery query);

    /// <summary>
    /// Handles the query to get bookings by room identifier.
    /// </summary>
    /// <param name="query">The query containing the room ID.</param>
    /// <returns>A collection of bookings for the specified room.</returns>
    Task<IEnumerable<Booking>> Handle(GetBookingsByRoomIdQuery query);
}
