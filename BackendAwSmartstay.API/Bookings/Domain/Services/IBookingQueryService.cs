using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

/// <summary>
/// Defines the contract for services that handle booking queries.
/// </summary>
public interface IBookingQueryService
{
    /// <summary>The booking, or null when it does not exist or is not visible to the requester.</summary>
    Task<Booking?> Handle(GetBookingByIdQuery query);

    /// <summary>The bookings visible to the requester, newest first.</summary>
    Task<IEnumerable<Booking>> Handle(GetBookingsQuery query);

    /// <summary>The bookings of a room.</summary>
    Task<IEnumerable<Booking>> Handle(GetBookingsByRoomIdQuery query);
}
