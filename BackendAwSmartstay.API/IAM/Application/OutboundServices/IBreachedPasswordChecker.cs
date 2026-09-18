namespace BackendAwSmartstay.API.IAM.Application.OutboundServices;

/// <summary>Answer of a breached password lookup.</summary>
public enum BreachedPasswordStatus
{
    /// <summary>The password was not found in the breach corpus.</summary>
    NotFound,
    /// <summary>The password appears in at least one known breach.</summary>
    Breached,
    /// <summary>The lookup could not be made (service down, timeout, check disabled).</summary>
    Unavailable
}

/// <summary>
///     Looks a password up in a corpus of passwords exposed in data breaches (NIST SP 800-63B-4 blocklist). The
///     adapter must never send the password, nor its full hash, outside the process.
/// </summary>
public interface IBreachedPasswordChecker
{
    Task<BreachedPasswordStatus> CheckAsync(string password, CancellationToken cancellationToken = default);
}
