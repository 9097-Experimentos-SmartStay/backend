using Microsoft.AspNetCore.Http;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;

/// <summary>
///     Lets a bounded context add extension members to the ProblemDetails written for one of its exceptions
///     (e.g. IAM adds <c>passwordRecoveryUrl</c> to "Email already registered"). Runs for every problem response;
///     implementations only act on the exceptions they know.
/// </summary>
public interface IProblemDetailsEnricher
{
    void Enrich(ProblemDetailsContext context);
}
