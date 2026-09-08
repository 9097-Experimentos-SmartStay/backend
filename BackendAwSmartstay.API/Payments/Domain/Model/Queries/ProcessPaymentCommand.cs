namespace BackendAwSmartstay.API.Payments.Domain.Model.Queries;

/// <summary>
/// Query to retrieve payment details for a specific booking.
/// </summary>
/// <param name="BookingId">The booking identifier.</param>
public record GetPaymentByBookingIdQuery(int BookingId);