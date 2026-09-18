namespace BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

/// <summary>
///     Base type of every error raised by a domain model (aggregates, entities, value objects and domain services).
/// </summary>
/// <remarks>
///     <para>
///         The hierarchy expresses the <em>kind</em> of failure, not a transport concern. The Interfaces layer maps each
///         kind to an HTTP status code in a single place (the global exception handler), so no controller or
///         application service needs to translate exceptions by hand.
///     </para>
///     <para>
///         Every domain exception also carries a stable, machine-readable <see cref="Code"/> (e.g.
///         <c>booking.room_unavailable</c>): part of the ubiquitous language, it never changes when the English
///         <see cref="Exception.Message"/> is reworded, so clients translate and branch on it instead of the text.
///         Each bounded context owns the catalog of its codes; <see cref="ErrorCodes"/> has the shared ones.
///     </para>
/// </remarks>
public abstract class DomainException : Exception
{
    private static readonly IReadOnlyDictionary<string, object?> NoParameters = new Dictionary<string, object?>();

    protected DomainException(string code, string message) : base(message)
    {
        Code = ErrorCodes.EnsureValid(code);
    }

    protected DomainException(string code, string message, Exception innerException) : base(message, innerException)
    {
        Code = ErrorCodes.EnsureValid(code);
    }

    /// <summary>Stable code of the failure (<c>context.reason</c>, lower snake case).</summary>
    public string Code { get; }

    /// <summary>
    ///     Values a client needs to explain the failure in its own words (e.g. the duplicated room number), keyed by
    ///     camelCase name. Empty when the code says it all.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Parameters { get; protected init; } = NoParameters;
}

/// <summary>A value or an input violates a domain invariant (e.g. an empty name, an invalid date range).</summary>
public class DomainValidationException : DomainException
{
    public DomainValidationException(string code, string message) : base(code, message) { }

    /// <summary>Kind-only constructor, kept for tests: production code always names its code.</summary>
    internal DomainValidationException(string message) : base(ErrorCodes.InvalidRequest, message) { }
}

/// <summary>
///     The request is well formed, but the current state of the model does not allow it
///     (e.g. confirming a cancelled booking, paying an already paid booking).
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string code, string message) : base(code, message) { }

    /// <summary>Kind-only constructor, kept for tests: production code always names its code.</summary>
    internal BusinessRuleViolationException(string message) : base(ErrorCodes.BusinessRuleViolated, message) { }
}

/// <summary>The referenced entity does not exist (or must be treated as non-existent for the requester).</summary>
/// <remarks>The code is derived from the entity name: <c>Booking</c> → <c>booking.not_found</c>.</remarks>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base(ErrorCodes.NotFoundFor(entityName), $"{entityName} {id} was not found.")
    {
        EntityName = entityName;
        EntityId = id;
    }

    protected EntityNotFoundException(string code, string message) : base(code, message)
    {
        EntityName = string.Empty;
        EntityId = string.Empty;
    }

    public string EntityName { get; }
    public object EntityId { get; }
}

/// <summary>
///     A domain rule forbids the requester from performing the operation
///     (e.g. an admin assigning a role higher than their own).
/// </summary>
public class OperationNotAllowedException : DomainException
{
    public OperationNotAllowedException(string code, string message) : base(code, message) { }

    /// <summary>Kind-only constructor, kept for tests: production code always names its code.</summary>
    internal OperationNotAllowedException(string message) : base(ErrorCodes.OperationNotAllowed, message) { }
}

/// <summary>The presented credentials do not identify an account.</summary>
public class AuthenticationFailedException : DomainException
{
    public AuthenticationFailedException(string code, string message) : base(code, message) { }
}

/// <summary>
///     The referenced resource existed but is no longer usable because its lifetime ended
///     (e.g. an expired password reset link).
/// </summary>
public class ResourceExpiredException : DomainException
{
    public ResourceExpiredException(string code, string message) : base(code, message) { }
}

/// <summary>
///     One input field has an invalid value (e.g. a password that breaks the password policy). The API reports it
///     as a validation problem keyed by that field, like model validation errors: the problem's code is
///     <see cref="ErrorCodes.ValidationFailed"/> and the <see cref="Violation"/> carries the rule the field broke.
/// </summary>
public class InvalidFieldException : DomainValidationException
{
    /// <param name="field">Name of the invalid input (the property of the command or request, any casing).</param>
    /// <param name="code">Stable code of the rule the value broke (e.g. <c>password.too_short</c>).</param>
    /// <param name="message">What is wrong and how to fix it.</param>
    /// <param name="parameters">Values of the rule a client needs to explain it (e.g. <c>minLength</c>).</param>
    public InvalidFieldException(string field, string code, string message,
        IReadOnlyDictionary<string, object?>? parameters = null)
        : base(ErrorCodes.ValidationFailed, message)
    {
        Violation = new FieldViolation(field, ErrorCodes.EnsureValid(code), message, parameters);
    }

    /// <summary>The broken rule of the field.</summary>
    public FieldViolation Violation { get; }

    /// <summary>Name of the invalid input.</summary>
    public string Field => Violation.Field;
}

/// <summary>A rule broken by one input field: which field, the stable code of the rule and the message.</summary>
/// <param name="Field">Name of the input (any casing; the API reports it in camelCase).</param>
/// <param name="Code">Stable code of the rule (e.g. <c>field.required</c>, <c>password.breached</c>).</param>
/// <param name="Message">English explanation for developers.</param>
/// <param name="Parameters">Values of the rule (e.g. <c>{ "minLength": 15 }</c>), or null.</param>
public sealed record FieldViolation(
    string Field,
    string Code,
    string Message,
    IReadOnlyDictionary<string, object?>? Parameters = null);

/// <summary>
///     Several input fields have invalid values at once (e.g. a value object built from a form, which reports every
///     broken rule instead of the first one). The API reports it like model validation: code
///     <see cref="ErrorCodes.ValidationFailed"/> and one violation per broken rule.
/// </summary>
public class InvalidFieldsException : DomainValidationException
{
    /// <param name="violations">The broken rules (at least one).</param>
    public InvalidFieldsException(IReadOnlyList<FieldViolation> violations)
        : base(ErrorCodes.ValidationFailed, Describe(violations))
    {
        Violations = violations;
    }

    /// <summary>Every broken rule, in the order the fields were checked.</summary>
    public IReadOnlyList<FieldViolation> Violations { get; }

    private static string Describe(IReadOnlyList<FieldViolation> violations)
    {
        if (violations.Count == 0)
            throw new ArgumentException("At least one violation is required.", nameof(violations));
        return string.Join(" ", violations.Select(violation => violation.Message));
    }
}
