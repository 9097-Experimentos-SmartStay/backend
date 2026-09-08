namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

public class UsernameAlreadyExistsException : UserDomainException
{
    public UsernameAlreadyExistsException(string username) 
        : base($"Username {username} is already taken") { }
}