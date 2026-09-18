namespace BackendAwSmartstay.API.Payments.Domain.Model.Queries;

/// <summary>The payment of a booking.</summary>
/// <param name="BookingId">The booking.</param>
/// <param name="GuestUserId">When set, the requester is a guest and must own the booking.</param>
/// <param name="StaffHotelId">When set (staff other than a chain admin), the booking must be of this hotel.</param>
public record GetPaymentByBookingIdQuery(int BookingId, int? GuestUserId = null, int? StaffHotelId = null);
