namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     Exception thrown when a user attempts an operation they are not authorized to perform.
/// </summary>
public class UnauthorizedOperationException : UserDomainException
{
    public UnauthorizedOperationException(string message)
        : base(message) { }
}