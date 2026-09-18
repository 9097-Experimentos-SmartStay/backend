using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;

/// <summary>
///     Maps exceptions that escape the application layer to HTTP status codes. This is the only place where
///     the kind of a domain failure is translated into the HTTP contract.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><see cref="DomainValidationException"/>: invalid value / invariant on the input → 400</item>
///         <item><see cref="AuthenticationFailedException"/>: wrong credentials → 401</item>
///         <item><see cref="OperationNotAllowedException"/>: a domain rule forbids the requester → 403</item>
///         <item><see cref="EntityNotFoundException"/>: the entity does not exist for the requester → 404</item>
///         <item><see cref="BusinessRuleViolationException"/>: the current state does not allow it → 409</item>
///         <item><see cref="DbUpdateException"/>: a database constraint rejected the change → 409</item>
///     </list>
///     Anything else (including <see cref="ArgumentException"/> or <see cref="InvalidOperationException"/>
///     thrown by framework code) is a bug and becomes a 500.
/// </remarks>
public static class ExceptionStatusCodeMapper
{
    public static (int StatusCode, string Title) Map(Exception exception) => exception switch
    {
        DomainValidationException => (StatusCodes.Status400BadRequest, "Invalid request"),
        AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
        OperationNotAllowedException => (StatusCodes.Status403Forbidden, "Operation not allowed"),
        EntityNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        BusinessRuleViolationException => (StatusCodes.Status409Conflict, "Business rule violation"),
        BadHttpRequestException badRequest => (badRequest.StatusCode, "Invalid request"),
        DbUpdateException => (StatusCodes.Status409Conflict, "Conflict with existing data"),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
    };
}
