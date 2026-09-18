namespace BackendAwSmartstay.API.Media.Infrastructure.Cloudinary;

/// <summary>
///     Cloudinary account of the project (section <c>Cloudinary</c>, env vars <c>Cloudinary__*</c>). The API secret
///     only lives in the server (Render); the browser receives a signature per upload. Required in Production;
///     in Development the uploads are disabled (503 <c>media.uploads_not_configured</c>) when it is missing.
/// </summary>
public class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    /// <summary>Cloud name of the account (Cloudinary console → Dashboard).</summary>
    public string? CloudName { get; set; }

    /// <summary>API key of the account (public: it is sent to the browser with each signature).</summary>
    public string? ApiKey { get; set; }

    /// <summary>API secret of the account. Signs the uploads; never sent to clients.</summary>
    public string? ApiSecret { get; set; }

    /// <summary>Signed upload preset of the hotel images (formats, incoming transformation, unique names).</summary>
    public string HotelImagesPreset { get; set; } = "smartstay-hotels";

    /// <summary>Folder of the hotel images (the same one the preset uses).</summary>
    public string HotelImagesFolder { get; set; } = "smartstay/hotels";

    /// <summary>Base of the Upload API (without the cloud name).</summary>
    public string ApiBaseUrl { get; set; } = "https://api.cloudinary.com/v1_1";

    /// <summary>Base of the delivery URLs (without the cloud name).</summary>
    public string DeliveryBaseUrl { get; set; } = "https://res.cloudinary.com";

    /// <summary>True when the account credentials are set.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(CloudName) && !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(ApiSecret);

    /// <summary>True when some credential is set (a half configuration is an error).</summary>
    public bool IsPartiallyConfigured =>
        !IsConfigured && (!string.IsNullOrWhiteSpace(CloudName) || !string.IsNullOrWhiteSpace(ApiKey) || !string.IsNullOrWhiteSpace(ApiSecret));
}
