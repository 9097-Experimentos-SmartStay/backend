using System.Text.RegularExpressions;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

/// <summary>
///     How the guests of a hotel pay their bookings (US-53, US-51 scenario 2): the account holder and at least one
///     method among Yape, Plin and a bank account (bank + account number, optionally the interbank code CCI).
///     A hotel without these settings does not accept bookings.
/// </summary>
/// <remarks>
///     Immutable: <see cref="Create"/> validates every rule and reports all the broken ones at once
///     (<see cref="InvalidFieldsException"/>). Yape and Plin numbers are Peruvian mobile numbers (9 digits starting
///     with 9); the CCI has 20 digits. Spaces (and hyphens in phone numbers and the CCI) are removed.
/// </remarks>
public sealed partial record HotelPaymentSettings
{
    public const int AccountHolderMinLength = 2;
    public const int AccountHolderMaxLength = 100;
    public const int BankNameMinLength = 2;
    public const int BankNameMaxLength = 60;
    public const int BankAccountMinDigits = 8;
    public const int BankAccountMaxDigits = 20;
    public const int BankAccountMaxLength = 30;
    public const int MobileNumberLength = 9;
    public const int CciLength = 20;

    /// <summary>EF Core constructor.</summary>
    private HotelPaymentSettings()
    {
        AccountHolder = string.Empty;
    }

    private HotelPaymentSettings(string accountHolder, string? yapeNumber, string? plinNumber, string? bankName,
        string? bankAccountNumber, string? bankAccountCci)
    {
        AccountHolder = accountHolder;
        YapeNumber = yapeNumber;
        PlinNumber = plinNumber;
        BankName = bankName;
        BankAccountNumber = bankAccountNumber;
        BankAccountCci = bankAccountCci;
    }

    /// <summary>Name the guest sees as the receiver of the payment.</summary>
    public string AccountHolder { get; private init; }

    /// <summary>Yape mobile number (9 digits), or null.</summary>
    public string? YapeNumber { get; private init; }

    /// <summary>Plin mobile number (9 digits), or null.</summary>
    public string? PlinNumber { get; private init; }

    /// <summary>Bank of the transfer account, or null.</summary>
    public string? BankName { get; private init; }

    /// <summary>Account number for transfers (digits and hyphens as the bank prints it), or null.</summary>
    public string? BankAccountNumber { get; private init; }

    /// <summary>Interbank account code (CCI, 20 digits), or null.</summary>
    public string? BankAccountCci { get; private init; }

    /// <summary>True when a bank transfer is offered (bank name and account number).</summary>
    public bool OffersBankTransfer => BankName is not null && BankAccountNumber is not null;

    /// <summary>
    ///     Builds the settings from the values typed by the administrator (blank = not set).
    /// </summary>
    /// <exception cref="InvalidFieldsException">One or more rules are broken; every violation is reported.</exception>
    public static HotelPaymentSettings Create(string? accountHolder, string? yapeNumber, string? plinNumber,
        string? bankName, string? bankAccountNumber, string? bankAccountCci)
    {
        var violations = new List<FieldViolation>();

        var holder = Blank(accountHolder);
        if (holder is null)
            violations.Add(new FieldViolation(nameof(AccountHolder), AccommodationErrorCodes.PaymentAccountHolderRequired,
                "Enter the name of the account holder the guests pay to."));
        else if (holder.Length is < AccountHolderMinLength or > AccountHolderMaxLength)
            violations.Add(new FieldViolation(nameof(AccountHolder), AccommodationErrorCodes.PaymentAccountHolderLength,
                $"The account holder must have {AccountHolderMinLength} to {AccountHolderMaxLength} characters.",
                new Dictionary<string, object?> { ["minLength"] = AccountHolderMinLength, ["maxLength"] = AccountHolderMaxLength }));

        var yape = MobileNumber(yapeNumber, nameof(YapeNumber), AccommodationErrorCodes.PaymentYapeNumberInvalid, "Yape", violations);
        var plin = MobileNumber(plinNumber, nameof(PlinNumber), AccommodationErrorCodes.PaymentPlinNumberInvalid, "Plin", violations);

        var bank = Blank(bankName);
        var account = Blank(bankAccountNumber)?.Replace(" ", string.Empty);
        var cci = Blank(bankAccountCci) is { } rawCci ? Digits(rawCci) : null;

        if (bank is not null && bank.Length is < BankNameMinLength or > BankNameMaxLength)
            violations.Add(new FieldViolation(nameof(BankName), AccommodationErrorCodes.PaymentBankNameLength,
                $"The bank name must have {BankNameMinLength} to {BankNameMaxLength} characters.",
                new Dictionary<string, object?> { ["minLength"] = BankNameMinLength, ["maxLength"] = BankNameMaxLength }));
        if (bank is null && (account is not null || cci is not null))
            violations.Add(new FieldViolation(nameof(BankName), AccommodationErrorCodes.PaymentBankNameRequired,
                "Enter the bank of the account."));

        if (account is null && (bank is not null || cci is not null))
            violations.Add(new FieldViolation(nameof(BankAccountNumber), AccommodationErrorCodes.PaymentBankAccountNumberRequired,
                "Enter the account number for bank transfers."));
        else if (account is not null && !IsBankAccountNumber(account))
            violations.Add(new FieldViolation(nameof(BankAccountNumber), AccommodationErrorCodes.PaymentBankAccountNumberInvalid,
                $"The account number must have {BankAccountMinDigits} to {BankAccountMaxDigits} digits (hyphens allowed).",
                new Dictionary<string, object?> { ["minDigits"] = BankAccountMinDigits, ["maxDigits"] = BankAccountMaxDigits }));

        if (cci is not null && !CciFormat().IsMatch(cci))
            violations.Add(new FieldViolation(nameof(BankAccountCci), AccommodationErrorCodes.PaymentCciInvalid,
                $"The CCI (interbank account code) has exactly {CciLength} digits."));

        if (Blank(yapeNumber) is null && Blank(plinNumber) is null && (bank is null || account is null))
            violations.Add(new FieldViolation("methods", AccommodationErrorCodes.PaymentMethodRequired,
                "Offer at least one payment method: Yape, Plin or a bank account (bank and account number)."));

        if (violations.Count > 0)
            throw new InvalidFieldsException(violations);

        return new HotelPaymentSettings(holder!, yape, plin, bank, account, cci);
    }

    private static string? MobileNumber(string? value, string field, string code, string method, List<FieldViolation> violations)
    {
        if (Blank(value) is not { } raw) return null;
        var number = raw.Replace(" ", string.Empty).Replace("-", string.Empty);
        if (MobileFormat().IsMatch(number)) return number;
        violations.Add(new FieldViolation(field, code,
            $"The {method} number must be a Peruvian mobile number: {MobileNumberLength} digits starting with 9."));
        return null;
    }

    private static bool IsBankAccountNumber(string account)
    {
        if (account.Length > BankAccountMaxLength || !BankAccountFormat().IsMatch(account)) return false;
        var digits = account.Count(char.IsAsciiDigit);
        return digits is >= BankAccountMinDigits and <= BankAccountMaxDigits;
    }

    private static string Digits(string value) => value.Replace(" ", string.Empty).Replace("-", string.Empty);

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex("^9[0-9]{8}$")]
    private static partial Regex MobileFormat();

    [GeneratedRegex("^[0-9]{20}$")]
    private static partial Regex CciFormat();

    [GeneratedRegex("^[0-9](?:[0-9-]*[0-9])?$")]
    private static partial Regex BankAccountFormat();
}
