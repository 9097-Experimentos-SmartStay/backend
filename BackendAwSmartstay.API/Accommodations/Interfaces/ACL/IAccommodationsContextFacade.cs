namespace BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

/// <summary>A room as offered for booking, exposed to other bounded contexts (no domain entity leaks).</summary>
/// <param name="RoomId">The room.</param>
/// <param name="HotelId">Its hotel.</param>
/// <param name="RoomTypeId">Its room type.</param>
/// <param name="RoomTypeName">Name of the room type.</param>
/// <param name="PricePerNight">Price per night.</param>
/// <param name="Description">Description.</param>
/// <param name="Amenities">Amenities.</param>
/// <param name="Status">Current operational status (Available, Occupied or Cleaning).</param>
/// <param name="Number">Room number, unique in its hotel (what guests and staff see).</param>
/// <param name="HotelAcceptsBookings">False while the hotel has no payment methods (US-53): its rooms cannot be booked.</param>
public sealed record RoomOffer(int RoomId, int HotelId, int RoomTypeId, string RoomTypeName, decimal PricePerNight,
    string Description, IReadOnlyList<string> Amenities, string Status, string Number, bool HotelAcceptsBookings)
{
    /// <summary>A room under maintenance is never offered nor booked.</summary>
    public bool IsOfferedForBooking => Status != "Maintenance";
}

/// <summary>A hotel as shown in messages of other bounded contexts.</summary>
/// <param name="HotelId">The hotel.</param>
/// <param name="Name">Its name.</param>
/// <param name="Address">Street address, city and country.</param>
public sealed record HotelSummary(int HotelId, string Name, string Address);

/// <summary>
///     How the guests of a hotel pay their bookings (US-51 scenario 2), exposed to other bounded contexts
///     (booking e-mails and the booking page). Null members are methods the hotel does not offer.
/// </summary>
/// <param name="AccountHolder">Name the guests pay to.</param>
/// <param name="YapeNumber">Yape mobile number.</param>
/// <param name="PlinNumber">Plin mobile number.</param>
/// <param name="BankName">Bank of the transfer account.</param>
/// <param name="BankAccountNumber">Account number (only with <paramref name="BankName"/>).</param>
/// <param name="BankAccountCci">Interbank account code (CCI).</param>
public sealed record HotelPaymentInstructions(
    string AccountHolder,
    string? YapeNumber,
    string? PlinNumber,
    string? BankName,
    string? BankAccountNumber,
    string? BankAccountCci);

public interface IAccommodationsContextFacade
{
    /// <summary>Number of each of <paramref name="roomIds"/> (one query; unknown ids are omitted).</summary>
    Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IReadOnlyCollection<int> roomIds);

    /// <summary>The room, or null when it does not exist.</summary>
    Task<RoomOffer?> FetchRoomAsync(int roomId);

    /// <summary>The hotel, or null when it does not exist.</summary>
    Task<HotelSummary?> FetchHotelAsync(int hotelId);

    /// <summary>
    ///     How to pay a booking of <paramref name="hotelId"/> (the hotel's payment methods), or null when the hotel
    ///     does not exist or has none yet.
    /// </summary>
    Task<HotelPaymentInstructions?> FetchPaymentInstructionsAsync(int hotelId);

    Task<bool> HotelExistsAsync(int hotelId);

    /// <summary>True when the room exists.</summary>
    Task<bool> RoomExistsAsync(int roomId);

    /// <summary>The room's price per night, or null when the room does not exist.</summary>
    Task<decimal?> FetchRoomPricePerNightAsync(int roomId);

    /// <summary>The hotel the room belongs to, or null when the room does not exist.</summary>
    Task<int?> FetchHotelIdOfRoomAsync(int roomId);

    /// <summary>
    ///     Locks the room for the current transaction so no other booking of it can be created or moved concurrently
    ///     (R1) and returns it, or null when the room does not exist. Call it inside the booking transaction, before
    ///     checking availability.
    /// </summary>
    Task<RoomOffer?> LockRoomForBookingAsync(int roomId);

    /// <summary>
    ///     US-08: the guest of a completed check-in moves into the room, which becomes Occupied. Runs in the caller's
    ///     transaction (commits the shared unit of work).
    /// </summary>
    /// <exception cref="BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions.BusinessRuleViolationException">The room is not Available.</exception>
    Task OccupyRoomForCheckInAsync(int roomId, int? guestUserId, string? guestEmail);

    /// <summary>Rooms that can be offered for booking (not under maintenance), of one hotel or of every hotel.</summary>
    Task<IReadOnlyList<RoomOffer>> FetchRoomsOfferedForBookingAsync(int? hotelId);
}
