using BackendAwSmartstay.API.Audit.Application.OutboundServices;

namespace BackendAwSmartstay.API.Audit.Infrastructure.Http;

/// <summary>
///     Reads the client IP of the current HTTP request. Behind the reverse proxies it is the client address
///     resolved from <c>X-Forwarded-For</c> by the forwarded headers middleware (trusted proxies only).
/// </summary>
public class HttpRequestOriginProvider(IHttpContextAccessor httpContextAccessor) : IRequestOriginProvider
{
    public string? ClientIpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
