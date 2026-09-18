using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

public class UsernameAlreadyExistsException : BusinessRuleViolationException
{
    public UsernameAlreadyExistsException(string username) 
        : base($"Username {username} is already taken") { }
}
