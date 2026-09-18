namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

/// <summary>A booking.</summary>
/// <param name="Id">Technical id (use it in the URLs).</param>
/// <param name="Code">Unique human-readable code, e.g. SS-7KQ4M2XP (US-51).</param>
/// <param name="HotelId">Hotel of the room.</param>
/// <param name="RoomId">The booked room.</param>
/// <param name="GuestName">Name of the guest.</param>
/// <param name="GuestEmail">Where the booking e-mails go.</param>
/// <param name="GuestPhone">Phone of the guest, if given.</param>
/// <param name="CheckInDate">Check-in date (calendar date).</param>
/// <param name="CheckOutDate">Check-out date (calendar date).</param>
/// <param name="Nights">Nights of the stay.</param>
/// <param name="PricePerNight">Price per night agreed when booking.</param>
/// <param name="TotalPrice">Amount to pay: price per night × nights.</param>
/// <param name="Status">Pending, Confirmed, CheckedIn, Cancelled or Completed.</param>
/// <param name="CreatedAt">When it was made (UTC).</param>
/// <param name="PaymentDueAt">Payment deadline of a Pending booking (UTC); null otherwise.</param>
/// <param name="ConfirmedAt">When its payment was registered.</param>
/// <param name="CancelledAt">When it was cancelled.</param>
/// <param name="CancellationReason">GuestRequest, HotelRequest or PaymentNotReceived.</param>
/// <param name="CheckedInAt">When the guest checked in.</param>
/// <param name="GuestProfileId">The associated guest profile, if any.</param>
/// <param name="UserId">The guest account that owns the booking, if any.</param>
public record BookingResource(
    int Id,
    string Code,
    int HotelId,
    int RoomId,
    string GuestName,
    string GuestEmail,
    string? GuestPhone,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int Nights,
    decimal PricePerNight,
    decimal TotalPrice,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaymentDueAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    DateTimeOffset? CheckedInAt,
    Guid? GuestProfileId = null,
    int? UserId = null);
