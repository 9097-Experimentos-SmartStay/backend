using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Payments.Application.Internal.Configuration;

/// <summary>
///     How guests pay a Pending booking (section <c>Payments:Instructions</c>, env vars
///     <c>Payments__Instructions__*</c>): shown in the booking e-mail. At least one method must be configured.
/// </summary>
public class PaymentInstructionsSettings : IValidatableObject
{
    public const string SectionName = "Payments:Instructions";

    /// <summary>Name of the account holder shown to the guest.</summary>
    [Required(ErrorMessage = "Payments:Instructions:AccountHolder is not configured (Payments__Instructions__AccountHolder).")]
    public string AccountHolder { get; set; } = string.Empty;

    /// <summary>Yape phone number.</summary>
    public string? YapeNumber { get; set; }

    /// <summary>Plin phone number.</summary>
    public string? PlinNumber { get; set; }

    /// <summary>Bank of the transfer account.</summary>
    public string? BankName { get; set; }

    /// <summary>Account number for transfers.</summary>
    public string? BankAccountNumber { get; set; }

    /// <summary>Interbank account code (CCI).</summary>
    public string? BankAccountCci { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(YapeNumber) && string.IsNullOrWhiteSpace(PlinNumber)
            && string.IsNullOrWhiteSpace(BankAccountNumber))
            yield return new ValidationResult(
                "Configure at least one payment method: Payments__Instructions__YapeNumber, __PlinNumber or __BankAccountNumber.");
    }
}
