using System.Text;
using System.Text.RegularExpressions;

namespace BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

/// <summary>
///     Shared error codes and the format every code follows: <c>context.reason</c> in lower snake case
///     (<c>booking.room_unavailable</c>, <c>field.required</c>). Codes are a public contract: once published they are
///     never renamed, only added.
/// </summary>
public static partial class ErrorCodes
{
    /// <summary>One or more input fields are invalid; each field violation carries its own code.</summary>
    public const string ValidationFailed = "validation.failed";

    /// <summary>Fallback of a <see cref="DomainValidationException"/> that has no specific code (tests only).</summary>
    public const string InvalidRequest = "request.invalid";

    /// <summary>Fallback of a <see cref="BusinessRuleViolationException"/> that has no specific code (tests only).</summary>
    public const string BusinessRuleViolated = "request.conflict";

    /// <summary>Fallback of an <see cref="OperationNotAllowedException"/> that has no specific code (tests only).</summary>
    public const string OperationNotAllowed = "request.forbidden";

    // Field rules shared by every context (model validation and value objects).
    public const string FieldRequired = "field.required";
    public const string FieldLength = "field.length";
    public const string FieldOutOfRange = "field.out_of_range";
    public const string FieldFormat = "field.invalid_format";
    public const string FieldEmail = "field.invalid_email";
    public const string FieldUrl = "field.invalid_url";
    public const string FieldPhone = "field.invalid_phone";
    public const string FieldNotAllowed = "field.not_allowed_value";
    public const string FieldInvalid = "field.invalid";

    // Paging of list queries.
    public const string PageInvalid = "paging.invalid_page";
    public const string PageSizeInvalid = "paging.invalid_page_size";

    /// <summary>Code of an entity that does not exist: <c>Booking</c> → <c>booking.not_found</c>.</summary>
    public static string NotFoundFor(string entityName) => $"{ToSnakeCase(entityName)}.not_found";

    /// <summary>Returns <paramref name="code"/> when it follows the format, otherwise throws (a programming error).</summary>
    public static string EnsureValid(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || !CodeFormat().IsMatch(code))
            throw new ArgumentException($"'{code}' is not a valid error code (expected context.reason in lower snake case).", nameof(code));
        return code;
    }

    private static string ToSnakeCase(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0) builder.Append('_');
            builder.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '_');
        }
        return builder.ToString();
    }

    [GeneratedRegex("^[a-z][a-z0-9_]*(\\.[a-z][a-z0-9_]*)+$")]
    private static partial Regex CodeFormat();
}
