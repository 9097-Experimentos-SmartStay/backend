using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

/// <summary>The username/password pair does not identify an account (never says which one is wrong).</summary>
public class InvalidCredentialsException : AuthenticationFailedException
{
    public InvalidCredentialsException()
        : base("Invalid credentials") { }
}
