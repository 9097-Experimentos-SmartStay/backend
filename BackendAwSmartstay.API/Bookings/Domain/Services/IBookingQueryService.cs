using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

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

    /// <summary>The bookings of a room visible to the requester.</summary>
    Task<IEnumerable<Booking>> Handle(GetBookingsByRoomIdQuery query);

    /// <summary>
    ///     The number of the room of each booking (one batch lookup in the Accommodations context), so clients never
    ///     resolve rooms themselves.
    /// </summary>
    Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IEnumerable<Booking> bookings);

    /// <summary>US-07 scenario 1: the calendar of a hotel.</summary>
    Task<BookingCalendar> Handle(GetBookingCalendarQuery query);
}
