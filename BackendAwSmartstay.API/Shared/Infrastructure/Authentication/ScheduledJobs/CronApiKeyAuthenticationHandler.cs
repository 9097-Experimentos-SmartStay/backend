using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;

/// <summary>
///     Native authentication scheme for the external scheduler: the request must carry the shared secret in the
///     <c>X-Cron-Key</c> header. The comparison is constant-time (hashes of both values), so the response time does
///     not leak how much of the key was right.
/// </summary>
public class CronApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<CronSettings> cronSettings,
    IProblemDetailsService problemDetailsService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "CronApiKey";
    public const string HeaderName = "X-Cron-Key";
    public const string ScopeClaim = "scope";
    public const string ScheduledJobsScope = "scheduled-jobs";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var presented) || string.IsNullOrEmpty(presented))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!KeysMatch(presented.ToString(), cronSettings.Value.ApiKey))
            return Task.FromResult(AuthenticateResult.Fail("Invalid scheduler key."));

        var identity = new ClaimsIdentity([new Claim(ScopeClaim, ScheduledJobsScope), new Claim(ClaimTypes.Name, "scheduler")], SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = Context,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = $"A valid {HeaderName} header is required to run scheduled jobs."
            }
        });
    }

    private static bool KeysMatch(string presented, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(presented)),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
}
