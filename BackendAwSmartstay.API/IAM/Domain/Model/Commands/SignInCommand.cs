namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>Sign in with e-mail and password (US-02).</summary>
/// <param name="Email">Login e-mail.</param>
/// <param name="Password">Password.</param>
/// <param name="RememberMe">Also start a remembered session (refresh token, US-02 scenario 4).</param>
public record SignInCommand(string Email, string Password, bool RememberMe = false);
