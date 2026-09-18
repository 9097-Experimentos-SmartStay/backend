namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>US-52: start enrolling an authenticator app (returns the secret for the QR code).</summary>
/// <param name="UserId">The user of the challenge token.</param>
public record StartMfaEnrollmentCommand(int UserId);

/// <summary>US-52: confirm the enrollment with a code; enables MFA, returns recovery codes and signs in.</summary>
public record ConfirmMfaEnrollmentCommand(int UserId, string Code, bool RememberMe);

/// <summary>US-52: present the second factor, a code of the authenticator app or a recovery code, and sign in.</summary>
public record VerifyMfaCommand(int UserId, string? Code, string? RecoveryCode, bool RememberMe);

/// <summary>US-52 scenario 4: an administrator resets the second factor of a user.</summary>
public record ResetMfaCommand(int ActorUserId, int TargetUserId);

/// <summary>Close every session of the user on every device.</summary>
public record SignOutEverywhereCommand(int UserId);
