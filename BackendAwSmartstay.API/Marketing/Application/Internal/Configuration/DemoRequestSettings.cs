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

    /// <summary>Default scheduling page of the sales team (Cal.com).</summary>
    public const string DefaultSchedulingUrl = "https://cal.com/piero-sulca-sanchez-rhh1nt/demo-smartstay";

    /// <summary>
    ///     Public page where the visitor picks the day and time of the demo (<c>DemoRequests__SchedulingUrl</c>, US-27
    ///     scenario 2). The confirmation and follow-up e-mails link straight to it, prefilled with the visitor's name
    ///     and e-mail, instead of the landing form.
    /// </summary>
    [Required(ErrorMessage = "DemoRequests:SchedulingUrl is not configured. Set 'DemoRequests__SchedulingUrl' to the public scheduling page.")]
    [Url(ErrorMessage = "DemoRequests:SchedulingUrl must be an absolute http(s) URL.")]
    public string SchedulingUrl { get; set; } = DefaultSchedulingUrl;

    /// <summary>The scheduling page prefilled with the visitor's <paramref name="name"/> and <paramref name="email"/>.</summary>
    public string SchedulingLinkFor(string name, string email)
    {
        var separator = SchedulingUrl.Contains('?') ? '&' : '?';
        return $"{SchedulingUrl}{separator}name={Uri.EscapeDataString(name)}&email={Uri.EscapeDataString(email)}";
    }
}
