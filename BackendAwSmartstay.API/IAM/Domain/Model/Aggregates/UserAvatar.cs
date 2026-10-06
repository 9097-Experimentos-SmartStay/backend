using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

/// <summary>
///     The profile picture of a user: one image per account, stored apart from <see cref="User"/> so signing in never
///     loads it. The web app crops and shrinks the picture before sending it (a few dozen KB); the type is decided by
///     the file's content (its signature), never by its name or the declared type.
/// </summary>
public class UserAvatar
{
    /// <summary>Largest picture accepted: 512 KB (the web app sends a 320×320 JPEG of ~30 KB).</summary>
    public const int MaxSizeBytes = 512 * 1024;

    /// <summary>EF Core constructor.</summary>
    protected UserAvatar()
    {
        ContentType = string.Empty;
        Content = [];
    }

    private UserAvatar(int userId)
    {
        if (userId <= 0) throw new DomainValidationException(IamErrorCodes.InternalInvariant, "A profile picture must belong to a user.");
        UserId = userId;
        ContentType = string.Empty;
        Content = [];
    }

    /// <summary>The account (also the key: one picture per user).</summary>
    public int UserId { get; private set; }

    /// <summary>image/jpeg, image/png or image/webp.</summary>
    public string ContentType { get; private set; }

    public byte[] Content { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>The first picture of a user.</summary>
    /// <exception cref="InvalidFieldException">Empty, too large, or not a JPEG, PNG or WebP image.</exception>
    public static UserAvatar Upload(int userId, byte[] content, DateTimeOffset now)
    {
        var avatar = new UserAvatar(userId);
        avatar.Replace(content, now);
        return avatar;
    }

    /// <summary>Replaces the picture.</summary>
    /// <exception cref="InvalidFieldException">Empty, too large, or not a JPEG, PNG or WebP image.</exception>
    public void Replace(byte[] content, DateTimeOffset now)
    {
        ContentType = Inspect(content);
        Content = content;
        UpdatedAt = now;
    }

    private static string Inspect(ReadOnlySpan<byte> content)
    {
        if (content.Length == 0)
            throw new InvalidFieldException("file", IamErrorCodes.AvatarRequired, "Choose a picture for your profile.");
        if (content.Length > MaxSizeBytes)
            throw new InvalidFieldException("file", IamErrorCodes.AvatarTooLarge, $"The profile picture cannot exceed {MaxSizeBytes / 1024} KB.");

        return content switch
        {
            [0xFF, 0xD8, 0xFF, ..] => "image/jpeg",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, ..] => "image/png",
            [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x45, 0x42, 0x50, ..] => "image/webp",
            _ => throw new InvalidFieldException("file", IamErrorCodes.AvatarFileType, "The profile picture must be a JPG, PNG or WebP image.")
        };
    }
}
