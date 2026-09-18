using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;

/// <summary>
///     Remembers, for the current request, the stable code of every model validation error, keyed by its message
///     (model state only keeps the message). The problem details writer reads them back to report one
///     <c>{ field, code, message }</c> violation per error.
/// </summary>
public static class FieldViolationCodes
{
    private const string ItemsKey = "SmartStay.FieldViolationCodes";

    /// <summary>A code and its parameters.</summary>
    public sealed record CodedRule(string Code, IReadOnlyDictionary<string, object?>? Parameters);

    public static void Record(HttpContext httpContext, string message, CodedRule rule)
    {
        if (httpContext.Items[ItemsKey] is not Dictionary<string, CodedRule> rules)
            httpContext.Items[ItemsKey] = rules = new Dictionary<string, CodedRule>(StringComparer.Ordinal);
        rules.TryAdd(message, rule);
    }

    /// <summary>The rule recorded for <paramref name="message"/>, or the generic <c>field.invalid</c>.</summary>
    public static CodedRule Find(HttpContext httpContext, string message) =>
        httpContext.Items[ItemsKey] is Dictionary<string, CodedRule> rules && rules.TryGetValue(message, out var rule)
            ? rule
            : new CodedRule(ErrorCodes.FieldInvalid, null);

    /// <summary>The code of a validation attribute: its own, or the one of its framework rule.</summary>
    public static CodedRule For(ValidationAttribute attribute) => attribute switch
    {
        ICodedValidationAttribute coded => new CodedRule(coded.ErrorCode, coded.ErrorParameters),
        RequiredAttribute => new CodedRule(ErrorCodes.FieldRequired, null),
        StringLengthAttribute length => new CodedRule(ErrorCodes.FieldLength,
            Parameters(("minLength", length.MinimumLength), ("maxLength", length.MaximumLength))),
        MaxLengthAttribute max => new CodedRule(ErrorCodes.FieldLength, Parameters(("maxLength", max.Length))),
        MinLengthAttribute min => new CodedRule(ErrorCodes.FieldLength, Parameters(("minLength", min.Length))),
        LengthAttribute length => new CodedRule(ErrorCodes.FieldLength,
            Parameters(("minLength", length.MinimumLength), ("maxLength", length.MaximumLength))),
        RangeAttribute range => new CodedRule(ErrorCodes.FieldOutOfRange,
            Parameters(("min", range.Minimum), ("max", range.Maximum))),
        EmailAddressAttribute => new CodedRule(ErrorCodes.FieldEmail, null),
        UrlAttribute => new CodedRule(ErrorCodes.FieldUrl, null),
        PhoneAttribute => new CodedRule(ErrorCodes.FieldPhone, null),
        RegularExpressionAttribute => new CodedRule(ErrorCodes.FieldFormat, null),
        AllowedValuesAttribute or DeniedValuesAttribute => new CodedRule(ErrorCodes.FieldNotAllowed, null),
        _ => new CodedRule(ErrorCodes.FieldInvalid, null)
    };

    private static IReadOnlyDictionary<string, object?> Parameters(params (string Name, object? Value)[] values) =>
        values.ToDictionary(value => value.Name, value => value.Value);
}
