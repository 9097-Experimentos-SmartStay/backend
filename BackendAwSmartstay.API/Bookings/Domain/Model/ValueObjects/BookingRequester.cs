namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>
///     Who asks for a booking operation, as seen by the Bookings context: a guest acting for themselves or hotel
///     staff acting for their hotel. Built by the Interfaces layer from the authenticated user and passed explicitly
///     in commands and queries.
/// </summary>
/// <param name="UserId">IAM user id of the requester.</param>
/// <param name="IsGuest">True when the requester is a guest.</param>
/// <param name="DisplayName">Login e-mail of a guest (fallback contact).</param>
/// <param name="GuestProfileId">The guest profile linked to the guest account, when known.</param>
/// <param name="StaffHotelId">The hotel a staff member works for (admin, reception, housekeeping, maintenance).</param>
/// <param name="AllHotels">True for a chain administrator, who operates every hotel.</param>
public sealed record BookingRequester(
    int UserId,
    bool IsGuest,
    string? DisplayName = null,
    Guid? GuestProfileId = null,
    int? StaffHotelId = null,
    bool AllHotels = false)
{
    public static BookingRequester Guest(int userId, string displayName) => new(userId, true, displayName);

    public static BookingRequester HotelStaff(int userId, int? hotelId, bool allHotels = false) =>
        new(userId, false, StaffHotelId: hotelId, AllHotels: allHotels);

    public BookingRequester WithGuestProfile(Guid? guestProfileId) => this with { GuestProfileId = guestProfileId };

    /// <summary>R4: staff operate the bookings of their own hotel; a chain administrator those of every hotel.</summary>
    public bool OperatesHotel(int hotelId) => !IsGuest && (AllHotels || StaffHotelId == hotelId);
}
