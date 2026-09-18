namespace BackendAwSmartstay.API.IAM.Domain.Model.Enums;

/// <summary>Result of presenting a second-factor code (US-52).</summary>
public enum MfaCodeOutcome
{
    /// <summary>The code is valid: the second factor is proven.</summary>
    Accepted,
    /// <summary>The code is wrong (counted toward the temporary lock).</summary>
    Rejected,
    /// <summary>The code is valid but its time step was already used (replay; counted toward the lock).</summary>
    Replayed,
    /// <summary>The code was rejected and this failure started the temporary lock.</summary>
    LockStarted
}
