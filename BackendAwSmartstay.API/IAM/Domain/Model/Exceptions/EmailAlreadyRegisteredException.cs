using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     US-01 scenario 2: the e-mail already identifies an account. The message is exactly the one the acceptance
///     criteria show ("Email already registered"); the API adds a link to recover the password.
/// </summary>
public class EmailAlreadyRegisteredException : BusinessRuleViolationException
{
    public const string DefaultMessage = "Email already registered";

    public EmailAlreadyRegisteredException(string email) : base(IamErrorCodes.EmailAlreadyRegistered, DefaultMessage)
    {
        Email = email;
    }

    public string Email { get; }
}
