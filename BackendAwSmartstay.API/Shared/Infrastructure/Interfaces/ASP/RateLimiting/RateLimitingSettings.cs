using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;

/// <summary>
///     Limits of the anonymous endpoints, per client IP (section <c>RateLimiting</c>, env vars
///     <c>RateLimiting__*</c>). Behind a reverse proxy (Render) set <c>ASPNETCORE_FORWARDEDHEADERS_ENABLED=true</c>
///     so the client IP comes from <c>X-Forwarded-For</c>.
/// </summary>
public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    /// <summary>Requests per window to credential endpoints (sign-in, sign-up, verification, password recovery/reset).</summary>
    [Range(1, 10_000)]
    public int CredentialsPermitLimit { get; set; } = 10;

    [Range(1, 3600)]
    public int CredentialsWindowSeconds { get; set; } = 60;

    /// <summary>Requests per window to public forms (demo requests).</summary>
    [Range(1, 10_000)]
    public int PublicFormsPermitLimit { get; set; } = 5;

    [Range(1, 3600)]
    public int PublicFormsWindowSeconds { get; set; } = 300;
}
