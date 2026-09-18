namespace BackendAwSmartstay.API.Payments.Domain.Model.Queries;

/// <summary>
/// Query to retrieve payment details for a specific booking.
/// </summary>
/// <param name="BookingId">The booking identifier.</param>
/// <param name="GuestUserId">When set, the requester is a guest and must own the booking.</param>
public record GetPaymentByBookingIdQuery(int BookingId, int? GuestUserId = null);
