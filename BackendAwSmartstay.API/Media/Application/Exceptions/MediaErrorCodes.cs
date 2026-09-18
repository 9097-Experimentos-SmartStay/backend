using BackendAwSmartstay.API.Shared.Application.Exceptions;

namespace BackendAwSmartstay.API.Media.Application.Exceptions;

/// <summary>Stable error codes of the Media context (uploads of hotel images).</summary>
public static class MediaErrorCodes
{
    public const string UploadsNotConfigured = "media.uploads_not_configured";
}

/// <summary>This deployment has no media library credentials: images cannot be uploaded (503).</summary>
public class MediaUploadsNotConfiguredException() : FeatureNotConfiguredException(MediaErrorCodes.UploadsNotConfigured,
    "Image uploads are not configured on this server (Cloudinary credentials missing). Contact the administrator.");
