namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>How the guests of a hotel pay their bookings (US-53). Every member is null until it is configured.</summary>
/// <param name="HotelId">The hotel.</param>
/// <param name="AcceptsBookings">True when at least one payment method is configured: only then can the hotel be booked.</param>
/// <param name="AccountHolder">Name the guests pay to.</param>
/// <param name="YapeNumber">Yape mobile number (9 digits).</param>
/// <param name="PlinNumber">Plin mobile number (9 digits).</param>
/// <param name="BankName">Bank of the transfer account.</param>
/// <param name="BankAccountNumber">Account number for transfers.</param>
/// <param name="BankAccountCci">Interbank account code (CCI, 20 digits).</param>
public record HotelPaymentSettingsResource(
    int HotelId,
    bool AcceptsBookings,
    string? AccountHolder,
    string? YapeNumber,
    string? PlinNumber,
    string? BankName,
    string? BankAccountNumber,
    string? BankAccountCci);

/// <summary>
///     The payment methods of a hotel (replaces the current ones). The account holder and at least one method are
///     required: Yape, Plin or a bank account (bank name + account number, optional CCI). Blank = not offered.
///     Every broken rule comes back at once as a violation with its own code.
/// </summary>
public record UpdateHotelPaymentSettingsResource
{
    /// <summary>Name the guests pay to (2 to 100 characters).</summary>
    /// <example>Hotel Bolivar S.A.C.</example>
    public string? AccountHolder { get; init; }

    /// <summary>Yape number: Peruvian mobile, 9 digits starting with 9 (spaces and hyphens are ignored).</summary>
    /// <example>987654321</example>
    public string? YapeNumber { get; init; }

    /// <summary>Plin number: Peruvian mobile, 9 digits starting with 9 (spaces and hyphens are ignored).</summary>
    /// <example>987654321</example>
    public string? PlinNumber { get; init; }

    /// <summary>Bank of the transfer account (2 to 60 characters); required with an account number.</summary>
    /// <example>BCP</example>
    public string? BankName { get; init; }

    /// <summary>Account number: 8 to 20 digits, hyphens allowed.</summary>
    /// <example>191-12345678-0-12</example>
    public string? BankAccountNumber { get; init; }

    /// <summary>Interbank account code (CCI): 20 digits (spaces and hyphens are ignored).</summary>
    /// <example>00219100123456780123</example>
    public string? BankAccountCci { get; init; }
}
