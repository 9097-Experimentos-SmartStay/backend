namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>
///     Who asks for a booking operation, as seen by the Bookings context: a guest acting for themselves or hotel
///     staff acting at the desk. Built by the Interfaces layer from the authenticated user and passed explicitly
///     in commands and queries.
/// </summary>
/// <param name="UserId">IAM user id of the requester.</param>
/// <param name="IsGuest">True when the requester is a guest.</param>
/// <param name="DisplayName">Name used for a guest's own booking when none is given (their login e-mail).</param>
/// <param name="GuestProfileId">The guest profile linked to the guest account, when known.</param>
public sealed record BookingRequester(int UserId, bool IsGuest, string? DisplayName = null, Guid? GuestProfileId = null)
{
    public static BookingRequester Guest(int userId, string displayName) => new(userId, true, displayName);

    public static BookingRequester HotelStaff(int userId) => new(userId, false);

    public BookingRequester WithGuestProfile(Guid? guestProfileId) => this with { GuestProfileId = guestProfileId };
}
