using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Configuration;

/// <summary>
///     Public URLs of the clients (section <c>App</c>, env vars <c>App__WebBaseUrl</c> and <c>App__LandingBaseUrl</c>).
///     E-mails link to these pages (e-mail verification, password reset, sign-in). Validated at startup.
/// </summary>
public class ApplicationUrlsSettings
{
    public const string SectionName = "App";

    /// <summary>Base URL of the web application, e.g. <c>https://app.smartstay.pe</c> (no trailing slash needed).</summary>
    [Required(ErrorMessage = "App:WebBaseUrl is not configured. Set 'App__WebBaseUrl' to the public URL of the web application.")]
    [Url(ErrorMessage = "App:WebBaseUrl must be an absolute http(s) URL.")]
    public string WebBaseUrl { get; set; } = string.Empty;

    /// <summary>Base URL of the public landing page, e.g. <c>https://smartstay.pe</c>.</summary>
    [Required(ErrorMessage = "App:LandingBaseUrl is not configured. Set 'App__LandingBaseUrl' to the public URL of the landing page.")]
    [Url(ErrorMessage = "App:LandingBaseUrl must be an absolute http(s) URL.")]
    public string LandingBaseUrl { get; set; } = string.Empty;

    /// <summary>Absolute link to a page of the web application, e.g. <c>WebLink("reset-password", ("token", t))</c>.</summary>
    public string WebLink(string path, params (string Name, string Value)[] query) => Link(WebBaseUrl, path, query);

    /// <summary>Absolute link to a page of the landing.</summary>
    public string LandingLink(string path, params (string Name, string Value)[] query) => Link(LandingBaseUrl, path, query);

    private static string Link(string baseUrl, string path, (string Name, string Value)[] query)
    {
        var url = $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        if (query.Length == 0) return url;
        return url + "?" + string.Join("&",
            query.Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.Value)}"));
    }
}
