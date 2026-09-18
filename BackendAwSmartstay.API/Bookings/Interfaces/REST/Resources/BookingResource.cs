namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

/// <summary>A booking.</summary>
/// <param name="Id">Technical id (use it in the URLs).</param>
/// <param name="Code">Unique human-readable code, e.g. SS-7KQ4M2XP (US-51).</param>
/// <param name="HotelId">Hotel of the room.</param>
/// <param name="RoomId">The booked room (technical id).</param>
/// <param name="RoomNumber">Number of the room, as guests and staff know it (US-53).</param>
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
/// <param name="PaymentInstructions">
///     How to pay it (the hotel's payment methods, US-53): only on the booking detail and the create response while
///     the booking is Pending; null otherwise.
/// </param>
public record BookingResource(
    int Id,
    string Code,
    int HotelId,
    int RoomId,
    string RoomNumber,
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
    int? UserId = null,
    BookingPaymentInstructionsResource? PaymentInstructions = null);

/// <summary>How the guest pays a Pending booking: the payment methods of its hotel. Null members are not offered.</summary>
/// <param name="AccountHolder">Name the guest pays to.</param>
/// <param name="YapeNumber">Yape mobile number (9 digits).</param>
/// <param name="PlinNumber">Plin mobile number (9 digits).</param>
/// <param name="BankName">Bank of the transfer account.</param>
/// <param name="BankAccountNumber">Account number for transfers.</param>
/// <param name="BankAccountCci">Interbank account code (CCI, 20 digits).</param>
public record BookingPaymentInstructionsResource(
    string AccountHolder,
    string? YapeNumber,
    string? PlinNumber,
    string? BankName,
    string? BankAccountNumber,
    string? BankAccountCci);
