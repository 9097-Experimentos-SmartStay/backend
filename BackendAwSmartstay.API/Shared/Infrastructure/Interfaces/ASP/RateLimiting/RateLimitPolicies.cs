namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;

/// <summary>Names of the rate limiting policies used with <c>[EnableRateLimiting]</c>.</summary>
public static class RateLimitPolicies
{
    /// <summary>Endpoints that accept credentials or send account e-mails (brute force, e-mail flooding).</summary>
    public const string Credentials = "credentials";

    /// <summary>Anonymous public forms (demo requests).</summary>
    public const string PublicForms = "public-forms";
}
