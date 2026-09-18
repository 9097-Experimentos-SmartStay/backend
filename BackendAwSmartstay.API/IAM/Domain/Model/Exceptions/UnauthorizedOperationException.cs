using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     Exception thrown when a user attempts an operation that the IAM rules (role hierarchy, scope) do not allow.
/// </summary>
public class UnauthorizedOperationException : OperationNotAllowedException
{
    public UnauthorizedOperationException(string code, string message)
        : base(code, message) { }

    /// <summary>Kind-only constructor, kept for tests: production code always names its code.</summary>
    internal UnauthorizedOperationException(string message)
        : base(IamErrorCodes.OutsideHierarchy, message) { }
}
