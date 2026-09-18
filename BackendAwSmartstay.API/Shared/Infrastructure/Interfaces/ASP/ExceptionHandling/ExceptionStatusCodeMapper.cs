using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;

/// <summary>
///     Maps exceptions that escape the controllers to HTTP status codes.
/// </summary>
/// <remarks>
///     Domain and application layers signal errors with standard exceptions:
///     <list type="bullet">
///         <item><see cref="ArgumentException"/> / <see cref="FormatException"/>: invalid input or value object → 400</item>
///         <item><see cref="KeyNotFoundException"/>: referenced resource does not exist → 404</item>
///         <item><see cref="UnauthorizedAccessException"/>: authenticated but not allowed → 403</item>
///         <item><see cref="InvalidOperationException"/>: business rule / state conflict → 409</item>
///         <item><see cref="DbUpdateException"/>: persistence constraint violation → 409</item>
///     </list>
///     <see cref="InvalidOperationException"/>s raised by EF Core itself are infrastructure bugs, not
///     business conflicts, so they are reported as 500.
/// </remarks>
public static class ExceptionStatusCodeMapper
{
    public static (int StatusCode, string Title) Map(Exception exception) => exception switch
    {
        InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
        UnauthorizedOperationException => (StatusCodes.Status403Forbidden, "Operation not allowed"),
        UserNotFoundException => (StatusCodes.Status404NotFound, "User not found"),
        UsernameAlreadyExistsException => (StatusCodes.Status409Conflict, "Username already exists"),
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Operation not allowed"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        BadHttpRequestException badRequest => (badRequest.StatusCode, "Invalid request"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
        FormatException => (StatusCodes.Status400BadRequest, "Invalid request"),
        DbUpdateException => (StatusCodes.Status409Conflict, "Conflict with existing data"),
        InvalidOperationException when IsFromEntityFramework(exception) =>
            (StatusCodes.Status500InternalServerError, "Unexpected error"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Business rule violation"),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
    };

    private static bool IsFromEntityFramework(Exception exception) =>
        exception.Source?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true
        || exception.Source?.StartsWith("Pomelo", StringComparison.Ordinal) == true;
}
