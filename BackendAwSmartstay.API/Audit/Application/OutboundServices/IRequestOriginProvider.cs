namespace BackendAwSmartstay.API.Audit.Application.OutboundServices;

/// <summary>Where the current request comes from (recorded with each audit entry).</summary>
public interface IRequestOriginProvider
{
    /// <summary>Client IP address, or null outside an HTTP request.</summary>
    string? ClientIpAddress { get; }
}
