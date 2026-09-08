namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

public class UserNotFoundException : UserDomainException
{
    public UserNotFoundException(string username)
        : base($"User with username '{username}' not found") { }

    public UserNotFoundException(int userId)
        : base($"User with id '{userId}' not found") { }
}