using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Media.Infrastructure.Cloudinary;

/// <summary>
///     Validates <see cref="CloudinarySettings"/> when the host starts (<c>ValidateOnStart</c>):
///     <list type="bullet">
///         <item>Production must have the account (cloud name, API key and secret): admins upload the hotel images
///         there, so the application refuses to start without it;</item>
///         <item>anywhere, the three credentials go together, and the cloud name, preset and folder must be usable
///         in URLs.</item>
///     </list>
///     Development without credentials starts with uploads disabled.
/// </summary>
public partial class CloudinarySettingsValidator(IHostEnvironment environment) : IValidateOptions<CloudinarySettings>
{
    public ValidateOptionsResult Validate(string? name, CloudinarySettings settings)
    {
        var failures = new List<string>();

        if (!settings.IsConfigured)
        {
            if (environment.IsProduction() || settings.IsPartiallyConfigured)
                failures.Add("Cloudinary is not configured: set 'Cloudinary__CloudName', 'Cloudinary__ApiKey' and " +
                             "'Cloudinary__ApiSecret' (Cloudinary console → Settings → API Keys). They are required in Production and go together.");
        }
        else if (!Identifier().IsMatch(settings.CloudName!))
        {
            failures.Add("Cloudinary:CloudName may only contain letters, digits, hyphens and underscores.");
        }

        if (string.IsNullOrWhiteSpace(settings.HotelImagesPreset) || !Identifier().IsMatch(settings.HotelImagesPreset))
            failures.Add("Cloudinary:HotelImagesPreset must be the name of the signed upload preset (letters, digits, hyphens, underscores).");
        if (string.IsNullOrWhiteSpace(settings.HotelImagesFolder) || !Folder().IsMatch(settings.HotelImagesFolder))
            failures.Add("Cloudinary:HotelImagesFolder must be a folder path such as 'smartstay/hotels'.");
        if (!Uri.TryCreate(settings.ApiBaseUrl, UriKind.Absolute, out var api) || api.Scheme != Uri.UriSchemeHttps)
            failures.Add("Cloudinary:ApiBaseUrl must be an absolute https URL.");
        if (!Uri.TryCreate(settings.DeliveryBaseUrl, UriKind.Absolute, out var delivery) || delivery.Scheme != Uri.UriSchemeHttps)
            failures.Add("Cloudinary:DeliveryBaseUrl must be an absolute https URL.");

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    [GeneratedRegex("^[A-Za-z0-9_-]+$")]
    private static partial Regex Identifier();

    [GeneratedRegex("^[A-Za-z0-9_-]+(/[A-Za-z0-9_-]+)*$")]
    private static partial Regex Folder();
}
