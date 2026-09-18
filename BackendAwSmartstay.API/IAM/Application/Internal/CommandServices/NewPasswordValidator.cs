using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;

/// <summary>
///     Applies the password policy whenever a password is set (sign-up, account created by an administrator,
///     password change, reset through the recovery link, administrator update): the domain rules of
///     <see cref="PasswordPolicy"/> first, then the breached password lookup.
/// </summary>
/// <remarks>
///     The breach lookup <b>fails open</b>: when the service cannot be reached the password is accepted and the
///     event is logged as a warning. Failing closed would make registration and, worse, password recovery depend on a
///     third-party API, locking users out during its outages; the length rule and the local blocklist still apply,
///     and the warning lets operations notice a long outage.
/// </remarks>
public class NewPasswordValidator(IBreachedPasswordChecker breachedPasswordChecker, ILogger<NewPasswordValidator> logger)
{
    /// <summary>Throws <see cref="InvalidFieldException"/> for <paramref name="field"/> when the password is not acceptable.</summary>
    public async Task EnsureAcceptableAsync(string? password, Role role, Email? email, string field)
    {
        var check = PasswordPolicy.Check(password, role, email);
        if (!check.IsAcceptable)
            throw new InvalidFieldException(field, check.Problem!);

        switch (await breachedPasswordChecker.CheckAsync(PasswordPolicy.Normalize(password!)))
        {
            case BreachedPasswordStatus.Breached:
                throw new InvalidFieldException(field, PasswordPolicy.BreachedPasswordProblem);
            case BreachedPasswordStatus.Unavailable:
                logger.LogWarning("Breached password check unavailable: the new password was accepted without it (fail open).");
                break;
        }
    }
}
