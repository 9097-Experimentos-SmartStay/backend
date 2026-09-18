namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>
///     US-53: sets how the guests of <paramref name="HotelId"/> pay their bookings (replaces the previous settings).
///     Blank values mean "not offered".
/// </summary>
public record ConfigureHotelPaymentSettingsCommand(
    int HotelId,
    string? AccountHolder,
    string? YapeNumber,
    string? PlinNumber,
    string? BankName,
    string? BankAccountNumber,
    string? BankAccountCci);
