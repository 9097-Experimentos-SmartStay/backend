using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Validation;

/// <summary>
///     Model validation with the rule of the <see cref="Email"/> value object, so a malformed e-mail is reported as
///     a field error (<c>errors.Email</c>, US-01 scenario 4) instead of a generic 400. Empty values are left to
///     <see cref="RequiredAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class AccountEmailAttribute() : ValidationAttribute("Enter a valid e-mail address (for example name@domain.com)."), ICodedValidationAttribute
{
    public string ErrorCode => IamErrorCodes.EmailInvalid;

    public override bool IsValid(object? value) =>
        value is null || (value is string text && (string.IsNullOrWhiteSpace(text) || Email.IsValid(text)));
}

/// <summary>Model validation with the rule of <see cref="PersonName"/> (letters, 2 to 50 characters).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class PersonNamePartAttribute()
    : ValidationAttribute("The {0} field must have 2 to 50 letters (spaces, hyphens and apostrophes allowed)."), ICodedValidationAttribute
{
    public string ErrorCode => IamErrorCodes.NameFormat;

    public override bool IsValid(object? value) =>
        value is null || (value is string text && (string.IsNullOrWhiteSpace(text) || PersonName.IsValidPart(text)));
}
