namespace BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

/// <summary>
/// Query to retrieve a booking by its identifier.
/// </summary>
/// <param name="BookingId">The identifier of the booking to retrieve.</param>
public record GetBookingByIdQuery(int BookingId);
