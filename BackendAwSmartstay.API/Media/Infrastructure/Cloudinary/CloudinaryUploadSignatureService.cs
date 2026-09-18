using BackendAwSmartstay.API.Media.Application.OutboundServices;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Media.Infrastructure.Cloudinary;

/// <summary>
///     Cloudinary adapter of <see cref="IUploadSignatureService"/>: signs uploads to the signed preset of the hotel
///     images (<see cref="CloudinarySettings.HotelImagesPreset"/>, folder
///     <see cref="CloudinarySettings.HotelImagesFolder"/>). The browser posts the file with these exact parameters to
///     the Upload API; Cloudinary accepts the signature for one hour after <c>timestamp</c>.
/// </summary>
public class CloudinaryUploadSignatureService(IOptions<CloudinarySettings> options, TimeProvider timeProvider)
    : IUploadSignatureService
{
    private CloudinarySettings Settings => options.Value;

    public bool IsConfigured => Settings.IsConfigured;

    public UploadSignature SignHotelImageUpload()
    {
        var settings = Settings;
        if (!settings.IsConfigured)
            throw new InvalidOperationException("Cloudinary is not configured.");

        var timestamp = timeProvider.GetUtcNow().ToUnixTimeSeconds();
        var parameters = new Dictionary<string, string>
        {
            ["folder"] = settings.HotelImagesFolder,
            ["timestamp"] = CloudinarySignature.Timestamp(timestamp),
            ["upload_preset"] = settings.HotelImagesPreset
        };

        return new UploadSignature(
            settings.CloudName!,
            settings.ApiKey!,
            timestamp,
            CloudinarySignature.Sign(parameters, settings.ApiSecret!),
            settings.HotelImagesPreset,
            settings.HotelImagesFolder,
            $"{settings.ApiBaseUrl.TrimEnd('/')}/{settings.CloudName}/image/upload");
    }

    public string? ImageDeliveryUrlPrefix =>
        Settings.IsConfigured ? $"{Settings.DeliveryBaseUrl.TrimEnd('/')}/{Settings.CloudName}/image/upload/" : null;
}
