using BackendAwSmartstay.API.Marketing.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Marketing.Interfaces.REST.Validation;

/// <summary>Name rule of <see cref="ContactDetails"/> as a field error.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ContactNameAttribute()
    : ValidationAttribute("The {0} field must have 2 to 50 letters (spaces, hyphens and apostrophes allowed)."), ICodedValidationAttribute
{
    public string ErrorCode => MarketingErrorCodes.NameInvalid;

    public override bool IsValid(object? value) =>
        value is not string text || string.IsNullOrWhiteSpace(text) || ContactDetails.IsValidName(text);
}

/// <summary>E-mail rule of <see cref="ContactDetails"/> (same as the landing: name@domain.tld) as a field error.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ContactEmailAttribute() : ValidationAttribute("Enter a valid e-mail address (for example name@domain.com)."), ICodedValidationAttribute
{
    public string ErrorCode => MarketingErrorCodes.EmailInvalid;

    public override bool IsValid(object? value) =>
        value is not string text || string.IsNullOrWhiteSpace(text) || ContactDetails.IsValidEmail(text);
}

/// <summary>Phone rule of <see cref="ContactDetails"/> as a field error.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ContactPhoneAttribute() : ValidationAttribute("The phone must have 7 to 15 digits and may start with +."), ICodedValidationAttribute
{
    public string ErrorCode => MarketingErrorCodes.PhoneInvalid;

    public override bool IsValid(object? value) => value is not string text || ContactDetails.IsValidPhone(text);
}

/// <summary>The value must be one of the codes of the landing contract (exact, lowercase).</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ContractCodeAttribute(params string[] codes)
    : ValidationAttribute($"The {{0}} field must be one of: {string.Join(", ", codes)}."), ICodedValidationAttribute
{
    public string ErrorCode => ErrorCodes.FieldNotAllowed;

    public IReadOnlyDictionary<string, object?> ErrorParameters => new Dictionary<string, object?> { ["allowed"] = codes };

    public override bool IsValid(object? value) =>
        value is not string text || string.IsNullOrEmpty(text) || codes.Contains(text, StringComparer.Ordinal);
}
