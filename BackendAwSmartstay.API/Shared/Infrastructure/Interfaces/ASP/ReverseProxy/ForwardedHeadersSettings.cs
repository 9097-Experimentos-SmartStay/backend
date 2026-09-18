namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ReverseProxy;

/// <summary>
///     Proxies allowed to report the client address through <c>X-Forwarded-For</c> / <c>X-Forwarded-Proto</c>
///     (section <c>ForwardedHeaders</c>). Only hops coming from these networks are unwrapped; the first address,
///     walking the chain from right to left, that is not inside them is the client.
/// </summary>
public class ForwardedHeadersSettings
{
    public const string SectionName = "ForwardedHeaders";

    /// <summary>
    ///     Trusted proxy networks in CIDR notation. When configured (<c>ForwardedHeaders__TrustedNetworks__0</c>,
    ///     <c>__1</c>...) they REPLACE <see cref="DefaultTrustedNetworks"/>; leave empty to use the defaults.
    /// </summary>
    public string[] TrustedNetworks { get; set; } = [];

    /// <summary>
    ///     Render's path is Cloudflare edge → Render load balancer (private address) → container. Trusted by default:
    ///     loopback, the private / carrier-grade NAT ranges Render uses internally and Cloudflare's published ranges
    ///     (https://www.cloudflare.com/ips-v4, https://www.cloudflare.com/ips-v6).
    /// </summary>
    public static readonly IReadOnlyList<string> DefaultTrustedNetworks =
    [
        // Loopback
        "127.0.0.0/8",
        "::1/128",

        // Private networks (Render's internal load balancers, docker networks) and carrier-grade NAT
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
        "100.64.0.0/10",
        "fc00::/7",

        // Cloudflare IPv4
        "173.245.48.0/20",
        "103.21.244.0/22",
        "103.22.200.0/22",
        "103.31.4.0/22",
        "141.101.64.0/18",
        "108.162.192.0/18",
        "190.93.240.0/20",
        "188.114.96.0/20",
        "197.234.240.0/22",
        "198.41.128.0/17",
        "162.158.0.0/15",
        "104.16.0.0/13",
        "104.24.0.0/14",
        "172.64.0.0/13",
        "131.0.72.0/22",

        // Cloudflare IPv6
        "2400:cb00::/32",
        "2606:4700::/32",
        "2803:f800::/32",
        "2405:b500::/32",
        "2405:8100::/32",
        "2a06:98c0::/29",
        "2c0f:f248::/32"
    ];

    /// <summary>The networks in effect: the configured ones, or the defaults when none are configured.</summary>
    public IReadOnlyList<string> EffectiveTrustedNetworks =>
        TrustedNetworks is { Length: > 0 } ? TrustedNetworks : DefaultTrustedNetworks;
}
