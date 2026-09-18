namespace BackendAwSmartstay.API.Media.Application.OutboundServices;

/// <summary>
///     Everything a browser needs to upload one file straight to the media library with a short-lived signature
///     (the API secret never leaves the server).
/// </summary>
/// <param name="CloudName">Account of the media library.</param>
/// <param name="ApiKey">Public API key of the account (not a secret).</param>
/// <param name="Timestamp">Unix time (seconds) the signature was issued at; the library rejects it after one hour.</param>
/// <param name="Signature">Signature of the upload parameters.</param>
/// <param name="UploadPreset">Signed upload preset (allowed formats, size limits, folder rules).</param>
/// <param name="Folder">Folder the file goes to.</param>
/// <param name="UploadUrl">Endpoint the browser posts the file to (multipart).</param>
public sealed record UploadSignature(
    string CloudName,
    string ApiKey,
    long Timestamp,
    string Signature,
    string UploadPreset,
    string Folder,
    string UploadUrl);

/// <summary>
///     Port to the media library that stores the images of the hotels (adapter: Cloudinary). Uploads are signed
///     by the server and sent by the browser; the server only signs and checks where images live.
/// </summary>
public interface IUploadSignatureService
{
    /// <summary>False when this deployment has no media library credentials (uploads disabled).</summary>
    bool IsConfigured { get; }

    /// <summary>Signs one upload of a hotel image. Only call it when <see cref="IsConfigured"/>.</summary>
    UploadSignature SignHotelImageUpload();

    /// <summary>
    ///     Every image delivered by this media library starts with this prefix (e.g. the <c>secure_url</c> of an
    ///     upload signed here); null when <see cref="IsConfigured"/> is false.
    /// </summary>
    string? ImageDeliveryUrlPrefix { get; }
}
