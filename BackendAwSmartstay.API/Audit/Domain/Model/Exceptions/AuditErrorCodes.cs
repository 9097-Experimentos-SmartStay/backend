namespace BackendAwSmartstay.API.Audit.Domain.Model.Exceptions;

/// <summary>Stable error codes of the Audit context (access audit log, US-03 scenario 4).</summary>
public static class AuditErrorCodes
{
    public const string PeriodInvalid = "audit.invalid_period";

    /// <summary>An invariant of the audit entry that only a programming error can break.</summary>
    public const string InternalInvariant = "audit.internal_invariant";
}
