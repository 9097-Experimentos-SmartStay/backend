using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using BackendAwSmartstay.API.Media.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Media.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Media.Interfaces.REST;

/// <summary>
///     Uploads of images to the project's media library (Cloudinary). The API only signs: the browser sends the
///     file straight to Cloudinary with a short-lived signature, so the API secret never leaves the server.
/// </summary>
[ApiController]
[Route("api/v1/media")]
[Produces("application/json")]
[SwaggerTag("Media: signed uploads of hotel images (Cloudinary)")]
public class MediaController(HotelImageUploadCommandService hotelImageUploadCommandService) : ControllerBase
{
    /// <summary>Signs one upload of a hotel image (US-53).</summary>
    /// <remarks>
    ///     Admin and chain_admin only; rate limited per user (429 <c>rate_limit.exceeded</c> + <c>Retry-After</c>).
    ///     The signature covers <c>folder</c>, <c>timestamp</c> and <c>upload_preset</c> and is valid for one hour.
    ///     The signed preset limits the file (jpg, png, webp; resized to at most 2000×2000) and gives it a unique name.
    ///     503 <c>media.uploads_not_configured</c> when this server has no Cloudinary credentials.
    /// </remarks>
    [HttpPost("hotel-images/signature")]
    [Authorize(Policy = Policies.UploadHotelImages)]
    [EnableRateLimiting(RateLimitPolicies.MediaUploads)]
    [SwaggerOperation(Summary = "Sign an upload of a hotel image", OperationId = "SignHotelImageUpload")]
    [ProducesResponseType(typeof(UploadSignatureResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public IActionResult SignHotelImageUpload()
    {
        var signature = hotelImageUploadCommandService.SignHotelImageUpload();
        return Ok(new UploadSignatureResource(signature.CloudName, signature.ApiKey, signature.Timestamp,
            signature.Signature, signature.UploadPreset, signature.Folder, signature.UploadUrl));
    }
}
