using BackendAwSmartstay.API.Audit.Application.OutboundServices;

namespace BackendAwSmartstay.API.Audit.Infrastructure.Http;

/// <summary>
///     Reads the client IP of the current HTTP request. Behind a reverse proxy it is the forwarded client address
///     when <c>ASPNETCORE_FORWARDEDHEADERS_ENABLED=true</c>.
/// </summary>
public class HttpRequestOriginProvider(IHttpContextAccessor httpContextAccessor) : IRequestOriginProvider
{
    public string? ClientIpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
