namespace BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

/// <summary>
///     Query for a single booking, returned only when it belongs to the given guest user.
/// </summary>
/// <param name="BookingId">The booking identifier.</param>
/// <param name="UserId">The IAM user id of the guest.</param>
public record GetOwnedBookingByIdQuery(int BookingId, int UserId);
