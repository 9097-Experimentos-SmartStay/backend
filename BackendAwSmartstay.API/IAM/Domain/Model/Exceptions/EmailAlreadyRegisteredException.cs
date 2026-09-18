using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

public class EmailAlreadyRegisteredException : BusinessRuleViolationException
{
    public EmailAlreadyRegisteredException(string email)
        : base($"Email already registered: {email}") { }
}
