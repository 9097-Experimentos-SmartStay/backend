using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
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

    /// <summary>Methods the hotel can register: an online card payment is only made by the guest.</summary>
    private static readonly string[] AllowedMethods =
        Enum.GetNames<PaymentMethod>().Where(m => m != nameof(PaymentMethod.OnlineCard)).ToArray();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Method)
            && (int.TryParse(Method, out _) || !Enum.TryParse<PaymentMethod>(Method, true, out var parsed)
                || !AllowedMethods.Contains(parsed.ToString())))
            yield return new CodedValidationResult(ErrorCodes.FieldNotAllowed,
                $"Method must be one of: {string.Join(", ", AllowedMethods)}.", ["method"],
                new Dictionary<string, object?> { ["allowed"] = AllowedMethods });
    }
}
