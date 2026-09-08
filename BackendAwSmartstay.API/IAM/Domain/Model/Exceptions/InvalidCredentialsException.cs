namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

public class InvalidCredentialsException : UserDomainException
{
    public InvalidCredentialsException()
        : base("Invalid credentials") { }
}