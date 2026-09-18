using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

/// <summary>
/// Query to retrieve a booking by its identifier. With a requester, the booking is only returned when it is
/// visible to them (a guest only sees their own bookings).
/// </summary>
/// <param name="BookingId">The identifier of the booking to retrieve.</param>
/// <param name="Requester">Who asks; null for internal callers.</param>
public record GetBookingByIdQuery(int BookingId, BookingRequester? Requester = null);
