namespace BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

/// <summary>
///     Base type of every error raised by a domain model (aggregates, entities, value objects and domain services).
/// </summary>
/// <remarks>
///     The hierarchy expresses the <em>kind</em> of failure, not a transport concern. The Interfaces layer maps each
///     kind to an HTTP status code in a single place (the global exception handler), so no controller or
///     application service needs to translate exceptions by hand.
/// </remarks>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }

    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>A value or an input violates a domain invariant (e.g. an empty name, an invalid date range).</summary>
public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
}

/// <summary>
///     The request is well formed, but the current state of the model does not allow it
///     (e.g. confirming a cancelled booking, paying an already paid booking).
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}

/// <summary>The referenced entity does not exist (or must be treated as non-existent for the requester).</summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} {id} was not found.")
    {
        EntityName = entityName;
        EntityId = id;
    }

    protected EntityNotFoundException(string message) : base(message)
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
    public OperationNotAllowedException(string message) : base(message) { }
}

/// <summary>The presented credentials do not identify an account.</summary>
public class AuthenticationFailedException : DomainException
{
    public AuthenticationFailedException(string message) : base(message) { }
}

/// <summary>
///     The referenced resource existed but is no longer usable because its lifetime ended
///     (e.g. an expired password reset link).
/// </summary>
public class ResourceExpiredException : DomainException
{
    public ResourceExpiredException(string message) : base(message) { }
}
