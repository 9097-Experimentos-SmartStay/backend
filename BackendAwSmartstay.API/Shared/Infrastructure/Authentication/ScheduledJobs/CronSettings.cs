using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;

/// <summary>
///     Shared secret of the external scheduler (GitHub Actions cron) that triggers scheduled jobs such as the demo
///     request follow-up (section <c>Cron</c>, env var <c>Cron__ApiKey</c>). Validated at startup.
/// </summary>
public class CronSettings
{
    public const string SectionName = "Cron";

    /// <summary>Value expected in the <c>X-Cron-Key</c> header. At least 32 characters (<c>openssl rand -hex 32</c>).</summary>
    [Required(ErrorMessage = "Cron:ApiKey is not configured. Set 'Cron__ApiKey' (at least 32 random characters).")]
    [MinLength(32, ErrorMessage = "Cron:ApiKey must have at least 32 characters.")]
    public string ApiKey { get; set; } = string.Empty;
}
