using System.Security.Cryptography;
using System.Text;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Passwords;

/// <summary>
///     Have I Been Pwned "Pwned Passwords" range API with k-anonymity: only the first 5 hexadecimal characters of the
///     SHA-1 of the password leave the process; the service answers every suffix of that range and the match is done
///     locally. <c>Add-Padding: true</c> pads the answer with fake entries (count 0) so its size does not reveal the
///     prefix either.
/// </summary>
public class PwnedPasswordsChecker(
    HttpClient httpClient,
    IOptions<PasswordPolicySettings> settings,
    ILogger<PwnedPasswordsChecker> logger) : IBreachedPasswordChecker
{
    public async Task<BreachedPasswordStatus> CheckAsync(string password, CancellationToken cancellationToken = default)
    {
        if (!settings.Value.BreachedPasswordCheckEnabled) return BreachedPasswordStatus.Unavailable;

        var hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(password)));
        var prefix = hash[..5];
        var suffix = hash[5..];

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"range/{prefix}");
            request.Headers.Add("Add-Padding", "true");
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Pwned Passwords answered {StatusCode}.", (int)response.StatusCode);
                return BreachedPasswordStatus.Unavailable;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            foreach (var line in body.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var separator = line.IndexOf(':');
                if (separator <= 0 || !line.AsSpan(0, separator).Equals(suffix, StringComparison.OrdinalIgnoreCase)) continue;
                // Padding entries have a count of 0.
                return int.TryParse(line.AsSpan(separator + 1), out var count) && count > 0
                    ? BreachedPasswordStatus.Breached
                    : BreachedPasswordStatus.NotFound;
            }
            return BreachedPasswordStatus.NotFound;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(exception, "Pwned Passwords could not be reached.");
            return BreachedPasswordStatus.Unavailable;
        }
    }
}
