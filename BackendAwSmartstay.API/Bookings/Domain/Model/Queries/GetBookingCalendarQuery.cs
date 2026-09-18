using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

/// <summary>
///     US-07 scenario 1: the active bookings of a hotel sharing a night with <paramref name="Window"/>, organized by
///     date.
/// </summary>
/// <param name="Requester">Staff of the hotel or a chain administrator.</param>
/// <param name="HotelId">The hotel; null means the requester's hotel (a chain administrator: every hotel).</param>
/// <param name="Window">The dates shown (check-out date of the window excluded).</param>
public record GetBookingCalendarQuery(BookingRequester Requester, int? HotelId, DateRange Window);
