using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

/// <summary>A payment received by the hotel for a booking (US-07 scenario 5). There is no amount: it is the booking total.</summary>
public record RegisterPaymentResource : IValidatableObject
{
    /// <summary>Yape, Plin, BankTransfer, Cash or CardAtFrontDesk.</summary>
    /// <example>Yape</example>
    [Required]
    public string? Method { get; init; }

    /// <summary>Operation number of the Yape/Plin payment, the transfer or the POS voucher. Not needed for Cash.</summary>
    /// <example>00123456</example>
    [MaxLength(50)]
    public string? OperationNumber { get; init; }

    /// <summary>Optional note.</summary>
    [MaxLength(300)]
    public string? Note { get; init; }

    /// <summary>The validated method (a method, so model validation never evaluates it).</summary>
    public PaymentMethod ToMethod() => Enum.Parse<PaymentMethod>(Method!, ignoreCase: true);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Method)
            && (int.TryParse(Method, out _) || !Enum.TryParse<PaymentMethod>(Method, true, out var parsed) || !Enum.IsDefined(parsed)))
            yield return new ValidationResult(
                $"Method must be one of: {string.Join(", ", Enum.GetNames<PaymentMethod>())}.", ["method"]);
    }
}
