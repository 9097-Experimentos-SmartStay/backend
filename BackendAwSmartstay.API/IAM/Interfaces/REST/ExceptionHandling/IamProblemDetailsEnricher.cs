using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.ExceptionHandling;

/// <summary>
///     Extra members of the IAM problem responses:
///     <list type="bullet">
///         <item>409 "Email already registered" → <c>passwordRecoveryUrl</c> (US-01 scenario 2: suggest recovering the password);</item>
///         <item>401 account locked → <c>lockedUntil</c> (US-02 scenario 3);</item>
///         <item>403 e-mail not verified → <c>emailVerificationRequired: true</c> (US-01).</item>
///     </list>
/// </summary>
public class IamProblemDetailsEnricher(IOptions<ApplicationUrlsSettings> urls) : IProblemDetailsEnricher
{
    public void Enrich(ProblemDetailsContext context)
    {
        switch (context.Exception)
        {
            case EmailAlreadyRegisteredException alreadyRegistered:
                context.ProblemDetails.Extensions["passwordRecoveryUrl"] =
                    urls.Value.WebLink("forgot-password", ("email", alreadyRegistered.Email));
                break;
            case EmailNotVerifiedException:
                context.ProblemDetails.Extensions["emailVerificationRequired"] = true;
                break;
            case AccountTemporarilyLockedException locked:
                context.ProblemDetails.Extensions["lockedUntil"] = locked.LockedUntil;
                break;
        }
    }
}
