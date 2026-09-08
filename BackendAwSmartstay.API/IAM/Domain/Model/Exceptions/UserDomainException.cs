namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

public abstract class UserDomainException : Exception
{
    protected UserDomainException(string message)
        : base(message) { }

    protected UserDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}