using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;

/// <summary>
///     A validation attribute of a request resource that names the stable code of its rule (e.g.
///     <c>field.invalid_email</c>). Attributes of the framework get their code from
///     <see cref="FieldViolationCodes"/>.
/// </summary>
public interface ICodedValidationAttribute
{
    /// <summary>Stable code of the rule.</summary>
    string ErrorCode { get; }

    /// <summary>Values of the rule a client needs to explain it, or null.</summary>
    IReadOnlyDictionary<string, object?>? ErrorParameters => null;
}

/// <summary>
///     A <see cref="ValidationResult"/> of an <see cref="IValidatableObject"/> resource that carries the stable code of
///     the broken rule.
/// </summary>
public sealed class CodedValidationResult(
    string code,
    string errorMessage,
    IEnumerable<string> memberNames,
    IReadOnlyDictionary<string, object?>? parameters = null)
    : ValidationResult(errorMessage, memberNames)
{
    public string Code { get; } = code;
    public IReadOnlyDictionary<string, object?>? Parameters { get; } = parameters;
}
