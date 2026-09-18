using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace BackendAwSmartstay.API.Media.Infrastructure.Cloudinary;

/// <summary>
///     Cloudinary's documented signature of API requests ("Generating authentication signatures"):
///     <list type="number">
///         <item>take every parameter that is sent, except <c>file</c>, <c>cloud_name</c>, <c>resource_type</c>
///         and <c>api_key</c>, with a non-empty value;</item>
///         <item>sort them by name and join them as <c>name=value</c> pairs separated by <c>&amp;</c>
///         (values are not URL-encoded);</item>
///         <item>append the API secret and hash the string with SHA-1: the signature is its lower-case hex.</item>
///     </list>
///     Verified against the example of the documentation: <c>eager=w_400,h_300,c_pad|w_260,h_200,c_crop</c>,
///     <c>public_id=sample_image</c>, <c>timestamp=1315060510</c>, secret <c>abcd</c> →
///     <c>bfd09f95f331f558cbd1320e67aa8d488770583e</c>.
/// </summary>
/// <remarks>
///     Implemented here instead of taking the CloudinaryDotNet SDK (<c>Api.SignParameters</c>): the server only
///     signs (the browser uploads), the algorithm is a few lines and fully specified, and the SDK would add a large
///     dependency (its own HTTP client and models) for one hash.
/// </remarks>
public static class CloudinarySignature
{
    private static readonly HashSet<string> Unsigned = new(StringComparer.Ordinal) { "file", "cloud_name", "resource_type", "api_key" };

    /// <summary>The string that is hashed (parameters sorted and joined, without the secret).</summary>
    public static string StringToSign(IReadOnlyDictionary<string, string> parameters) =>
        string.Join('&', parameters
            .Where(parameter => !Unsigned.Contains(parameter.Key) && !string.IsNullOrEmpty(parameter.Value))
            .OrderBy(parameter => parameter.Key, StringComparer.Ordinal)
            .Select(parameter => $"{parameter.Key}={parameter.Value}"));

    /// <summary>SHA-1 signature (lower-case hex) of <paramref name="parameters"/> with <paramref name="apiSecret"/>.</summary>
    public static string Sign(IReadOnlyDictionary<string, string> parameters, string apiSecret)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiSecret);
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(StringToSign(parameters) + apiSecret));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>A Unix timestamp (seconds) as Cloudinary expects it.</summary>
    public static string Timestamp(long unixSeconds) => unixSeconds.ToString(CultureInfo.InvariantCulture);
}
