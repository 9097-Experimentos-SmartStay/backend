using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

/// <summary>
///     Card the guest pays their booking with (simulated gateway: no money moves). There is no amount: it is the booking
///     total. Test cards declined: 4000000000000002 (declined), 4000000000009995 (insufficient funds),
///     4000000000000127 (incorrect CVV); any other valid number is approved, e.g. 4242424242424242.
/// </summary>
public record CardPaymentResource : IValidatableObject
{
    /// <summary>Card number, 13 to 19 digits (spaces and dashes allowed).</summary>
    /// <example>4242 4242 4242 4242</example>
    [Required]
    [MaxLength(30)]
    public string? CardNumber { get; init; }

    /// <summary>Name printed on the card.</summary>
    /// <example>ROSA QUISPE</example>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string? CardHolderName { get; init; }

    /// <summary>Expiry month, 1 to 12.</summary>
    /// <example>12</example>
    [Required]
    [Range(1, 12)]
    public int? ExpiryMonth { get; init; }

    /// <summary>Expiry year, four digits (two digits are read as 20YY).</summary>
    /// <example>2030</example>
    [Required]
    [Range(0, 2100)]
    public int? ExpiryYear { get; init; }

    /// <summary>Security code, 3 digits (4 for Amex).</summary>
    /// <example>123</example>
    [Required]
    [RegularExpression(@"^\d{3,4}$")]
    public string? Cvv { get; init; }

    private string Digits => new((CardNumber ?? string.Empty).Where(c => c is not (' ' or '-')).ToArray());

    private int FullExpiryYear => ExpiryYear is < 100 ? 2000 + ExpiryYear.Value : ExpiryYear ?? 0;

    /// <summary>The validated card (a method, so model validation never evaluates it).</summary>
    public CardDetails ToCard() => new(Digits, CardHolderName!.Trim(), ExpiryMonth!.Value, FullExpiryYear, Cvv!);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(CardNumber))
        {
            var digits = Digits;
            if (digits.Length is < 13 or > 19 || !digits.All(char.IsAsciiDigit) || !PassesLuhn(digits))
                yield return new CodedValidationResult(ErrorCodes.FieldFormat, "The card number is not valid.", ["cardNumber"]);
        }

        if (ExpiryMonth is >= 1 and <= 12 && ExpiryYear is not null)
        {
            var today = DateTime.UtcNow;
            if (FullExpiryYear < today.Year || (FullExpiryYear == today.Year && ExpiryMonth < today.Month))
                yield return new CodedValidationResult(ErrorCodes.FieldInvalid, "The card has expired.", ["expiryMonth", "expiryYear"]);
        }
    }

    /// <summary>Luhn checksum of card numbers.</summary>
    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var d = digits[digits.Length - 1 - i] - '0';
            if (i % 2 == 1 && (d *= 2) > 9) d -= 9;
            sum += d;
        }
        return sum % 10 == 0;
    }
}
