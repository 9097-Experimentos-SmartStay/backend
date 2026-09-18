using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Marketing.Application.Internal.Configuration;

/// <summary>
///     Demo request settings (sections <c>Sales</c> and <c>DemoRequests</c>), validated at startup.
/// </summary>
public class SalesSettings
{
    public const string SectionName = "Sales";

    /// <summary>Inbox of the sales team, notified of every demo request (<c>Sales__NotificationEmail</c>).</summary>
    [Required(ErrorMessage = "Sales:NotificationEmail is not configured. Set 'Sales__NotificationEmail' to the sales inbox.")]
    [EmailAddress]
    public string NotificationEmail { get; set; } = string.Empty;
}

/// <summary>Automatic follow-up of demo requests (US-27 scenario 4).</summary>
public class DemoRequestSettings
{
    public const string SectionName = "DemoRequests";

    /// <summary>Hours a request waits in Received before the automatic follow-up (<c>DemoRequests__FollowUpAfterHours</c>).</summary>
    [Range(1, 30 * 24)]
    public int FollowUpAfterHours { get; set; } = 48;

    public TimeSpan FollowUpAfter => TimeSpan.FromHours(FollowUpAfterHours);
}
