namespace BackendAwSmartstay.API.IAM.Domain.Model.Enums;

/// <summary>What a single-use account token (sent by e-mail as a link) allows.</summary>
public enum AccountTokenPurpose
{
    /// <summary>Confirms the e-mail of a new account (US-01).</summary>
    EmailVerification,
    /// <summary>Sets a new password without knowing the current one (US-04).</summary>
    PasswordReset
}
