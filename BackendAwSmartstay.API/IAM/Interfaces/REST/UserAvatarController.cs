using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST;

/// <summary>Picture of the multipart upload of a profile picture.</summary>
public record AvatarFormResource
{
    /// <summary>JPG, PNG or WebP, at most 512 KB.</summary>
    [Required]
    public IFormFile? File { get; init; }
}

/// <summary>
///     Profile picture of the signed-in user (any role). The picture is served by the API itself, so the client reads
///     it with its Bearer token (e.g. as a blob), not with a plain <c>&lt;img src&gt;</c>.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/users/me/avatar")]
[SwaggerTag("Users: profile picture of the signed-in user")]
public class UserAvatarController(UserAvatarCommandService avatarCommandService) : ControllerBase
{
    /// <summary>A little over the largest picture, for the multipart envelope.</summary>
    private const long MaxRequestBytes = UserAvatar.MaxSizeBytes + 16 * 1024;

    /// <summary>Gets the profile picture (the image itself). 404 when the user has none.</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get my profile picture", OperationId = "GetMyAvatar")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "image/jpeg", "image/png", "image/webp")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAvatar()
    {
        var avatar = await avatarCommandService.FindAsync(User.GetUserId());
        if (avatar is null) return NotFound();

        Response.Headers.CacheControl = "private, no-cache";
        return File(avatar.Content, avatar.ContentType, lastModified: avatar.UpdatedAt, entityTag: null);
    }

    /// <summary>Sets or replaces the profile picture.</summary>
    /// <remarks>
    ///     Multipart field <c>file</c>: JPG, PNG or WebP of at most 512 KB (the type is read from the content).
    ///     400 <c>avatar.required</c> | <c>avatar.too_large</c> | <c>avatar.file_type</c> on <c>file</c>. Rate limited
    ///     per user like the other uploads (429 + <c>Retry-After</c>).
    /// </remarks>
    [HttpPut]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [EnableRateLimiting(RateLimitPolicies.MediaUploads)]
    [SwaggerOperation(Summary = "Change my profile picture", OperationId = "ChangeMyAvatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ChangeMyAvatar([FromForm] AvatarFormResource form)
    {
        using var buffer = new MemoryStream();
        await form.File!.CopyToAsync(buffer);
        await avatarCommandService.ChangeAsync(User.GetUserId(), buffer.ToArray());
        return NoContent();
    }

    /// <summary>Removes the profile picture (204 also when there was none).</summary>
    [HttpDelete]
    [SwaggerOperation(Summary = "Remove my profile picture", OperationId = "RemoveMyAvatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMyAvatar()
    {
        await avatarCommandService.RemoveAsync(User.GetUserId());
        return NoContent();
    }
}
