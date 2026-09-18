using System.Text.Json;
using System.Text.Json.Serialization;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;

/// <summary>
///     Gives every ProblemDetails the API writes a stable, machine-readable <c>code</c> extension, in one place
///     (it runs from <c>ProblemDetailsOptions.CustomizeProblemDetails</c>, so it covers the exception handler, model
///     validation, <c>NotFound()</c>/<c>Forbid()</c> results, status code pages, the bearer and scheduler challenges
///     and the rate limiter):
///     <list type="bullet">
///         <item>a domain exception → its <see cref="DomainException.Code"/> (+ <c>params</c> when it has any);</item>
///         <item>an invalid field (domain or model validation) → <c>validation.failed</c> + <c>violations</c>: one
///         <c>{ field, code, message, params? }</c> per broken rule, next to the usual <c>errors</c>;</item>
///         <item>a code already set by the writer (bearer events, scheduler key, rate limiter) is kept;</item>
///         <item>anything else → the generic code of its status (<see cref="ForStatus"/>).</item>
///     </list>
///     Clients branch and translate on <c>code</c> (and each violation's <c>code</c>); <c>detail</c> stays an
///     English explanation for developers.
/// </summary>
public static class ProblemCodes
{
    public const string CodeExtension = "code";
    public const string ParametersExtension = "params";
    public const string ViolationsExtension = "violations";

    // Codes of the HTTP pipeline (not of a bounded context)
    public const string RequestMalformed = "request.malformed";
    public const string RequestInvalid = ErrorCodes.InvalidRequest;
    public const string Unauthenticated = "auth.unauthenticated";
    public const string Forbidden = "auth.forbidden";
    public const string NotFound = "resource.not_found";
    public const string MethodNotAllowed = "request.method_not_allowed";
    public const string Conflict = ErrorCodes.BusinessRuleViolated;
    public const string Gone = "resource.expired";
    public const string UnsupportedMediaType = "request.unsupported_media_type";
    public const string DataConflict = "data.conflict";
    public const string RateLimited = "rate_limit.exceeded";
    public const string SchedulerKeyInvalid = "scheduler.key_invalid";
    public const string ServerError = "server.error";
    public const string ServiceUnavailable = "service.unavailable";

    public static void Apply(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        switch (context.Exception)
        {
            case InvalidFieldException invalidField:
                problem.Title = "One or more validation errors occurred.";
                problem.Extensions["errors"] = new Dictionary<string, string[]>
                {
                    [CamelCase(invalidField.Field)] = [invalidField.Message]
                };
                problem.Extensions[ViolationsExtension] = new[] { FieldViolationResource.From(invalidField.Violation) };
                problem.Extensions[CodeExtension] = invalidField.Code;
                return;
            case DomainException domain:
                problem.Extensions[CodeExtension] = domain.Code;
                if (domain.Parameters.Count > 0) problem.Extensions[ParametersExtension] = domain.Parameters;
                return;
            case DbUpdateException:
                problem.Extensions[CodeExtension] = DataConflict;
                return;
            case BadHttpRequestException:
                problem.Extensions[CodeExtension] = RequestMalformed;
                return;
        }

        if (problem is HttpValidationProblemDetails validation)
        {
            problem.Extensions[ViolationsExtension] = validation.Errors
                .SelectMany(entry => entry.Value.Select(message =>
                {
                    var rule = FieldViolationCodes.Find(context.HttpContext, message);
                    return new FieldViolationResource(FieldName(entry.Key), rule.Code, message, rule.Parameters);
                }))
                .ToList();
            problem.Extensions[CodeExtension] = ErrorCodes.ValidationFailed;
            return;
        }

        if (!problem.Extensions.ContainsKey(CodeExtension))
            problem.Extensions[CodeExtension] = ForStatus(problem.Status ?? context.HttpContext.Response.StatusCode);
    }

    /// <summary>Generic code of a status, for problems no domain rule explains.</summary>
    public static string ForStatus(int status) => status switch
    {
        StatusCodes.Status400BadRequest => RequestInvalid,
        StatusCodes.Status401Unauthorized => Unauthenticated,
        StatusCodes.Status403Forbidden => Forbidden,
        StatusCodes.Status404NotFound => NotFound,
        StatusCodes.Status405MethodNotAllowed => MethodNotAllowed,
        StatusCodes.Status409Conflict => Conflict,
        StatusCodes.Status410Gone => Gone,
        StatusCodes.Status415UnsupportedMediaType => UnsupportedMediaType,
        StatusCodes.Status429TooManyRequests => RateLimited,
        StatusCodes.Status503ServiceUnavailable => ServiceUnavailable,
        >= StatusCodes.Status500InternalServerError => ServerError,
        _ => RequestInvalid
    };

    /// <summary>Model state keys are JSON paths (<c>$.password</c>, <c>guest.email</c>); the violation uses the leaf.</summary>
    private static string FieldName(string key)
    {
        var leaf = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key;
        return leaf.Length == 0 ? key : leaf;
    }

    private static string CamelCase(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);

    /// <summary>One broken rule of an input field, as written in <c>violations</c>.</summary>
    /// <param name="Field">camelCase name (or JSON path) of the field; empty for the whole body.</param>
    /// <param name="Code">Stable code of the rule.</param>
    /// <param name="Message">English explanation for developers.</param>
    /// <param name="Params">Values of the rule (e.g. <c>minLength</c>), omitted when there are none.</param>
    public sealed record FieldViolationResource(
        string Field,
        string Code,
        string Message,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        IReadOnlyDictionary<string, object?>? Params)
    {
        public static FieldViolationResource From(FieldViolation violation) =>
            new(CamelCase(violation.Field), violation.Code, violation.Message, violation.Parameters);
    }
}
