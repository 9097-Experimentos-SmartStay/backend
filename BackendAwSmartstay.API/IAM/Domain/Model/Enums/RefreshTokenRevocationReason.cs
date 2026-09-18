namespace BackendAwSmartstay.API.IAM.Domain.Model.Enums;

/// <summary>Why a refresh token stopped being usable.</summary>
public enum RefreshTokenRevocationReason
{
    /// <summary>It was exchanged for a new one (rotation). Presenting it again means it was stolen.</summary>
    Rotated,
    /// <summary>The user signed out.</summary>
    SignedOut,
    /// <summary>A rotated token of its session was reused: the whole session is revoked.</summary>
    ReuseDetected,
    /// <summary>
    ///     The user's sessions were revoked (role or hotel change, password change or reset, deactivation, sign-out
    ///     everywhere, MFA reset).
    /// </summary>
    SessionRevoked
}
