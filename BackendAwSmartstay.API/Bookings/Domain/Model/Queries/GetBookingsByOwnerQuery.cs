namespace BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

/// <summary>
///     Query for the bookings owned by a guest user (created by them or attached to their guest profile).
/// </summary>
/// <param name="UserId">The IAM user id of the guest.</param>
public record GetBookingsByOwnerQuery(int UserId);
